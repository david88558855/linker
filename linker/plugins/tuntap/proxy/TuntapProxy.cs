﻿using linker.client.config;
using linker.config;
using linker.plugins.relay;
using linker.tunnel;
using linker.tunnel.connection;
using linker.libs;
using linker.libs.extends;
using System.Collections.Concurrent;
using linker.plugins.tuntap.config;
using linker.tun;
using System.Buffers.Binary;
using linker.plugins.client;

namespace linker.plugins.tuntap.proxy
{
    public sealed class TuntapProxy : ILinkerTunDeviceCallback, ITunnelConnectionReceiveCallback
    {
        private readonly TunnelTransfer tunnelTransfer;
        private readonly RelayTransfer relayTransfer;
        private readonly RunningConfig runningConfig;
        private readonly FileConfig config;
        private readonly LinkerTunDeviceAdapter linkerTunDeviceAdapter;
        private readonly ClientSignInTransfer clientSignInTransfer;

        private uint[] maskValues = Array.Empty<uint>();
        private readonly ConcurrentDictionary<uint, string> ip2MachineDic = new ConcurrentDictionary<uint, string>();
        private readonly ConcurrentDictionary<string, ITunnelConnection> connections = new ConcurrentDictionary<string, ITunnelConnection>();
        private readonly ConcurrentDictionary<uint, ITunnelConnection> ipConnections = new ConcurrentDictionary<uint, ITunnelConnection>();

        private readonly OperatingMultipleManager operatingMultipleManager = new OperatingMultipleManager();
        public VersionManager Version { get; } = new VersionManager();

        public TuntapProxy(TunnelTransfer tunnelTransfer, RelayTransfer relayTransfer, RunningConfig runningConfig, FileConfig config, LinkerTunDeviceAdapter linkerTunDeviceAdapter, ClientSignInTransfer clientSignInTransfer)
        {
            this.tunnelTransfer = tunnelTransfer;
            this.relayTransfer = relayTransfer;
            this.runningConfig = runningConfig;
            this.config = config;
            this.linkerTunDeviceAdapter = linkerTunDeviceAdapter;
            this.clientSignInTransfer = clientSignInTransfer;

            //监听打洞连接成功
            tunnelTransfer.SetConnectedCallback("tuntap", OnConnected);
            //监听中继连接成功
            relayTransfer.SetConnectedCallback("tuntap", OnConnected);

        }

        /// <summary>
        /// 将隧道拦截成功的连接对象缓存起来，下次有连接需要隧道就直接拿，不用再去打洞或中继
        /// </summary>
        /// <param name="connection"></param>
        private void OnConnected(ITunnelConnection connection)
        {
            if (LoggerHelper.Instance.LoggerLevel <= LoggerTypes.DEBUG)
                LoggerHelper.Instance.Warning($"tuntap add connection {connection.GetHashCode()} {connection.ToJson()}");
            if (connections.TryGetValue(connection.RemoteMachineId, out ITunnelConnection connectionOld))
            {
                LoggerHelper.Instance.Error($"new tunnel del {connection.Equals(connectionOld)}->{connectionOld.GetHashCode()}:{connectionOld.IPEndPoint}->{connection.GetHashCode()}:{connection.IPEndPoint}");
                //connectionOld?.Dispose();
            }
            connections.AddOrUpdate(connection.RemoteMachineId, connection, (a, b) => connection);

            connection.BeginReceive(this, null);

            Version.Add();
        }
        public async Task Receive(ITunnelConnection connection, ReadOnlyMemory<byte> buffer, object state)
        {
            linkerTunDeviceAdapter.Write(buffer);
            await Task.CompletedTask;
        }
        public async Task Closed(ITunnelConnection connection, object state)
        {
            Version.Add();
            await Task.CompletedTask;
        }

        public async Task Callback(LinkerTunDevicPacket packet)
        {
            //if(LoggerHelper.Instance.LoggerLevel <= LoggerTypes.DEBUG) LoggerHelper.Instance.Debug($"got packet to {packet.Dist}");

            //IPV4广播组播
            if (packet.IPV4Broadcast)
            {
                if (connections.IsEmpty == false)
                {
                    await Task.WhenAll(connections.Values.Where(c => c != null && c.Connected).Select(c => c.SendAsync(packet.Packet)));
                }
                return;
            }
            //IPV6 多播
            else if (packet.IPV6Multicast)
            {
                if (connections.IsEmpty == false)
                {
                    await Task.WhenAll(connections.Values.Where(c => c != null && c.Connected).Select(c => c.SendAsync(packet.Packet)));
                }
                return;
            }

            //IPV4+IPV6 单播
            uint ip = BinaryPrimitives.ReadUInt32BigEndian(packet.DistIPAddress.Span[^4..]);
            if (ipConnections.TryGetValue(ip, out ITunnelConnection connection) == false || connection == null || connection.Connected == false)
            {

                if (operatingMultipleManager.StartOperation(ip) == false)
                {
                    return;
                }

                //if (LoggerHelper.Instance.LoggerLevel <= LoggerTypes.DEBUG) LoggerHelper.Instance.Debug($"got packet to {packet.Dist} 1");
                _ = ConnectTunnel(ip).ContinueWith((result, state) =>
                {
                    operatingMultipleManager.StopOperation((uint)state);
                    if (result.Result != null)
                    {
                        ipConnections.AddOrUpdate((uint)state, result.Result, (a, b) => result.Result);
                    }
                }, ip);
                return;
            }
            //if (LoggerHelper.Instance.LoggerLevel <= LoggerTypes.DEBUG) LoggerHelper.Instance.Debug($"got packet to {packet.Dist} 2")
            await connection.SendAsync(packet.Packet);
        }

        /// <summary>
        /// 设置IP，等下有连接进来，用IP匹配，才能知道这个连接是要连谁
        /// </summary>
        /// <param name="ips"></param>
        public void SetIPs(List<TuntapVeaLanIPAddressList> ips)
        {
            ip2MachineDic.Clear();
            ipConnections.Clear();
            foreach (var item in ips)
            {
                foreach (var ip in item.IPS)
                {
                    ip2MachineDic.AddOrUpdate(ip.NetWork, item.MachineId, (a, b) => item.MachineId);
                }
            }
            maskValues = ips.SelectMany(c => c.IPS.Select(c => c.MaskValue)).Distinct().OrderBy(c => c).ToArray();
        }
        public void SetIP(string machineId, uint ip)
        {
            ip2MachineDic.AddOrUpdate(ip, machineId, (a, b) => machineId);
        }

        /// <summary>
        /// 打洞或者中继
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        private async Task<ITunnelConnection> ConnectTunnel(uint ip)
        {
            //直接按IP查找
            if (ip2MachineDic.TryGetValue(ip, out string machineId))
            {
                return await ConnectTunnel(machineId).ConfigureAwait(false);
            }

            //匹配掩码查找
            for (int i = 0; i < maskValues.Length; i++)
            {
                uint network = ip & maskValues[i];
                if (ip2MachineDic.TryGetValue(network, out machineId))
                {
                    ip2MachineDic.TryAdd(ip, machineId);
                    return await ConnectTunnel(machineId).ConfigureAwait(false);
                }
            }
            //if (LoggerHelper.Instance.LoggerLevel <= LoggerTypes.DEBUG) LoggerHelper.Instance.Debug($"got packet ConnectTunnel {machineId}  not found");
            return null;

        }
        /// <summary>
        /// 打洞或者中继
        /// </summary>
        /// <param name="machineId"></param>
        /// <returns></returns>
        private async Task<ITunnelConnection> ConnectTunnel(string machineId)
        {
            //if (LoggerHelper.Instance.LoggerLevel <= LoggerTypes.DEBUG) LoggerHelper.Instance.Debug($"got packet ConnectTunnel {machineId} ");

            if (config.Data.Client.Id == machineId)
            {
                return null;
            }

            if (connections.TryGetValue(machineId, out ITunnelConnection connection) && connection.Connected)
            {
                //if (LoggerHelper.Instance.LoggerLevel <= LoggerTypes.DEBUG) LoggerHelper.Instance.Debug($"got packet ConnectTunnel {machineId}  1");

                return connection;
            }

            if (operatingMultipleManager.StartOperation(machineId) == false)
            {
                return null;
            }
            //if (LoggerHelper.Instance.LoggerLevel <= LoggerTypes.DEBUG) LoggerHelper.Instance.Debug($"got packet ConnectTunnel {machineId} 2");

            try
            {
                if (await clientSignInTransfer.GetOnline(machineId) == false)
                {
                    //if (LoggerHelper.Instance.LoggerLevel <= LoggerTypes.DEBUG) LoggerHelper.Instance.Debug($"got packet ConnectTunnel {machineId} 3");
                    return null;
                }

                if (LoggerHelper.Instance.LoggerLevel <= LoggerTypes.DEBUG) LoggerHelper.Instance.Debug($"tuntap tunnel to {machineId}");

                connection = await tunnelTransfer.ConnectAsync(machineId, "tuntap", TunnelProtocolType.Quic).ConfigureAwait(false);
                if (connection != null)
                {
                    if (LoggerHelper.Instance.LoggerLevel <= LoggerTypes.DEBUG) LoggerHelper.Instance.Debug($"tuntap tunnel success,{connection.ToString()}");
                }
                if (connection == null)
                {
                    if (LoggerHelper.Instance.LoggerLevel <= LoggerTypes.DEBUG) LoggerHelper.Instance.Debug($"tuntap relay to {machineId}");

                    connection = await relayTransfer.ConnectAsync(config.Data.Client.Id, machineId, "tuntap").ConfigureAwait(false);
                    if (connection != null)
                    {
                        if (LoggerHelper.Instance.LoggerLevel <= LoggerTypes.DEBUG) LoggerHelper.Instance.Debug($"tuntap relay success,{connection.ToString()}");
                    }
                    //tunnelTransfer.StartBackground(machineId, "tuntap", TunnelProtocolType.Quic,()=> connections.TryGetValue(machineId, out ITunnelConnection connection) && connection.Connected);
                }
                if (connection != null)
                {
                    connections.AddOrUpdate(machineId, connection, (a, b) => connection);
                }
            }
            catch (Exception)
            {
            }
            finally
            {
                operatingMultipleManager.StopOperation(machineId);
            }
            return connection;
        }

        public ConcurrentDictionary<string, ITunnelConnection> GetConnections()
        {
            return connections;
        }
        public void RemoveConnection(string machineId)
        {
            if (connections.TryRemove(machineId, out ITunnelConnection _connection))
            {
                try
                {
                    _connection.Dispose();
                }
                catch (Exception)
                {
                }
                Version.Add();
            }
        }


    }
}
