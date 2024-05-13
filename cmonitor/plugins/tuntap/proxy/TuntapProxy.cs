﻿using cmonitor.client.tunnel;
using cmonitor.plugins.relay;
using cmonitor.plugins.tunnel;
using cmonitor.plugins.tuntap.vea;
using common.libs;
using common.libs.socks5;
using System.Buffers.Binary;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;

namespace cmonitor.plugins.tuntap.proxy
{
    public sealed class TuntapProxy : TunnelProxy
    {
        private readonly TunnelTransfer tunnelTransfer;
        private readonly RelayTransfer relayTransfer;

        private IPEndPoint proxyEP;

        private uint maskValue = NetworkHelper.MaskValue(24);
        private readonly ConcurrentDictionary<uint, string> dic = new ConcurrentDictionary<uint, string>();
        private readonly ConcurrentDictionary<string, ITunnelConnection> dicConnections = new ConcurrentDictionary<string, ITunnelConnection>();

        public TuntapProxy(TunnelTransfer tunnelTransfer, RelayTransfer relayTransfer)
        {
            this.tunnelTransfer = tunnelTransfer;
            this.relayTransfer = relayTransfer;

            Start(0);
            proxyEP = new IPEndPoint(IPAddress.Any, LocalEndpoint.Port);
            Logger.Instance.Info($"start tuntap proxy, listen port : {LocalEndpoint}");


            tunnelTransfer.SetConnectCallback("tuntap", BindConnectionReceive);
            relayTransfer.SetConnectCallback("tuntap", BindConnectionReceive);
        }

        public void SetIPs(List<TuntapVeaLanIPAddressList> ips)
        {
            dic.Clear();
            foreach (var item in ips)
            {
                foreach (var ip in item.IPS)
                {
                    dic.AddOrUpdate(ip.NetWork, item.MachineName, (a, b) => item.MachineName);
                }
            }
        }

        protected override async Task<bool> ConnectTcp(AsyncUserToken token)
        {
            token.Proxy.TargetEP = null;
            token.Proxy.Rsv = (byte)Socks5EnumStep.Request;

            //步骤，request
            bool result = await ReceiveCommandData(token);
            if (result == false) return false;
            await token.Socket.SendAsync(new byte[] { 0x05, 0x00 });
            token.Proxy.Rsv = (byte)Socks5EnumStep.Command;
            token.Proxy.Data = Helper.EmptyArray;

            //步骤，command
            result = await ReceiveCommandData(token);
            if (result == false)
            {
                return false;
            }
            Socks5EnumRequestCommand command = (Socks5EnumRequestCommand)token.Proxy.Data.Span[1];

            //获取远端地址
            Memory<byte> ipArray = Socks5Parser.GetRemoteEndPoint(token.Proxy.Data, out Socks5EnumAddressType addressType, out ushort port, out int index);
            token.Proxy.TargetEP = new IPEndPoint(new IPAddress(ipArray.Span), port);
            token.Proxy.Data = token.Proxy.Data.Slice(index);
            //不支持域名
            if (addressType == Socks5EnumAddressType.Domain)
            {
                token.Proxy.TargetEP = null;
                byte[] response1 = Socks5Parser.MakeConnectResponse(proxyEP, (byte)Socks5EnumResponseCommand.AddressNotAllow);
                await token.Socket.SendAsync(response1.AsMemory());
                return false;
            }
            //是UDP中继，不做连接操作，等UDP数据过去的时候再绑定
            if (token.Proxy.TargetEP.Address.Equals(IPAddress.Any) || command == Socks5EnumRequestCommand.UdpAssociate)
            {
                token.Proxy.TargetEP = null;
                byte[] response1 = Socks5Parser.MakeConnectResponse(proxyEP, (byte)Socks5EnumResponseCommand.ConnecSuccess);
                await token.Socket.SendAsync(response1.AsMemory());
                return true;
            }

            token.Connection = await ConnectTunnel(ipArray);

            Socks5EnumResponseCommand resp = token.Connection != null && token.Connection.Connected ? Socks5EnumResponseCommand.ConnecSuccess : Socks5EnumResponseCommand.NetworkError;
            byte[] response = Socks5Parser.MakeConnectResponse(proxyEP, (byte)resp);
            await token.Socket.SendAsync(response.AsMemory());

            return token.Connection != null && token.Connection.Connected;
        }
        protected override async Task ConnectUdp(AsyncUserUdpToken token)
        {
            Memory<byte> ipArray = Socks5Parser.GetRemoteEndPoint(token.Proxy.Data, out Socks5EnumAddressType addressType, out ushort port, out int index);
            token.Proxy.TargetEP = new IPEndPoint(new IPAddress(ipArray.Span), port);
            //解析出udp包的数据部分
            token.Proxy.Data = Socks5Parser.GetUdpData(token.Proxy.Data);
            token.Connection = await ConnectTunnel(ipArray);
        }
        protected override async Task<bool> ConnectionReceiveUdp(AsyncUserToken token, UdpClient udpClient)
        {
            byte[] data = Socks5Parser.MakeUdpResponse(token.Proxy.TargetEP, token.Proxy.Data, out int length);
            try
            {
                await udpClient.SendAsync(data.AsMemory(0, length), token.Proxy.SourceEP);
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                Socks5Parser.Return(data);
            }
            return true;
        }


        private async Task<ITunnelConnection> ConnectTunnel(Memory<byte> ipArray)
        {
            uint network = BinaryPrimitives.ReadUInt32BigEndian(ipArray.Span) & maskValue;
            if (dic.TryGetValue(network, out string targetName) == false)
            {
                return null;
            }
            if (dicConnections.TryGetValue(targetName, out ITunnelConnection connection) && connection.Connected)
            {
                return connection;
            }

            Logger.Instance.Debug($"tuntap tunnel to {targetName}");
            connection = await tunnelTransfer.ConnectAsync(targetName, "viewer");
            if (connection != null)
            {
                Logger.Instance.Debug($"tuntap tunnel to {targetName} success");
            }
            if (connection == null)
            {
                Logger.Instance.Debug($"tuntap relay to {targetName}");
                connection = await relayTransfer.ConnectAsync(targetName, "viewer");
                if (connection != null)
                {
                    Logger.Instance.Debug($"tuntap relay to {targetName} success");
                }
            }
            if (connection != null)
            {
                BindConnectionReceive(connection);
                dicConnections.AddOrUpdate(targetName, connection, (a, b) => connection);
            }
            return connection;
        }

        private async Task<bool> ReceiveCommandData(AsyncUserToken token)
        {
            int totalLength = token.Proxy.Data.Length;
            EnumProxyValidateDataResult validate = ValidateData(token.Proxy);
            if ((validate & EnumProxyValidateDataResult.TooShort) == EnumProxyValidateDataResult.TooShort)
            {
                //太短
                while ((validate & EnumProxyValidateDataResult.TooShort) == EnumProxyValidateDataResult.TooShort)
                {
                    totalLength += await token.Socket.ReceiveAsync(token.Saea.Buffer.AsMemory(token.Saea.Offset + totalLength), SocketFlags.None);
                    token.Proxy.Data = token.Saea.Buffer.AsMemory(token.Saea.Offset, totalLength);
                    validate = ValidateData(token.Proxy);
                }
            }

            //不短，又不相等，直接关闭连接
            if ((validate & EnumProxyValidateDataResult.Equal) != EnumProxyValidateDataResult.Equal)
            {
                return false;
            }
            return true;
        }
        public EnumProxyValidateDataResult ValidateData(ProxyInfo info)
        {
            return (Socks5EnumStep)info.Rsv switch
            {
                Socks5EnumStep.Request => Socks5Parser.ValidateRequestData(info.Data),
                Socks5EnumStep.Command => Socks5Parser.ValidateCommandData(info.Data),
                Socks5EnumStep.Auth => Socks5Parser.ValidateAuthData(info.Data, Socks5EnumAuthType.Password),
                Socks5EnumStep.Forward => EnumProxyValidateDataResult.Equal,
                Socks5EnumStep.ForwardUdp => EnumProxyValidateDataResult.Equal,
                _ => EnumProxyValidateDataResult.Equal
            };
        }
    }
}
