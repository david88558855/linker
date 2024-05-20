﻿using common.libs;
using common.libs.extends;
using System.Buffers;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;

namespace cmonitor.client.tunnel
{
    public class TunnelProxy
    {
        private ConcurrentDictionary<int, AsyncUserToken> userTokens = new ConcurrentDictionary<int, AsyncUserToken>();
        private ConcurrentDictionary<int, AsyncUserUdpToken> udpClients = new ConcurrentDictionary<int, AsyncUserUdpToken>();

        private Socket socket;

        private readonly NumberSpace ns = new NumberSpace();
        private readonly ConcurrentDictionary<ConnectId, Socket> dic = new ConcurrentDictionary<ConnectId, Socket>();
        private ConcurrentDictionary<ConnectIdUdp, Socket> dicUdp = new(new ConnectIdUdpComparer());

        public IPEndPoint LocalEndpoint => socket?.LocalEndPoint as IPEndPoint ?? new IPEndPoint(IPAddress.Any, 0);

        public TunnelProxy()
        {
        }

        public void Start(int port)
        {
            try
            {
                //Stop();

                IPEndPoint localEndPoint = new IPEndPoint(NetworkHelper.IPv6Support ? IPAddress.IPv6Any : IPAddress.Any, port);
                socket = new Socket(localEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                socket.IPv6Only(localEndPoint.AddressFamily, false);
                socket.ReuseBind(localEndPoint);
                socket.Listen(int.MaxValue);
                AsyncUserToken userToken = new AsyncUserToken
                {
                    ListenPort = port,
                    Socket = socket
                };
                SocketAsyncEventArgs acceptEventArg = new SocketAsyncEventArgs
                {
                    UserToken = userToken,
                    SocketFlags = SocketFlags.None,
                };
                userToken.Saea = acceptEventArg;

                acceptEventArg.Completed += IO_Completed;
                StartAccept(acceptEventArg);


                UdpClient udpClient = new UdpClient(new IPEndPoint(IPAddress.Any, localEndPoint.Port));
                AsyncUserUdpToken asyncUserUdpToken = new AsyncUserUdpToken
                {
                    ListenPort = port,
                    SourceSocket = udpClient,
                    Proxy = new ProxyInfo { Step = ProxyStep.Forward, ConnectId = 0, Protocol = ProxyProtocol.Udp, Direction = ProxyDirection.Forward }
                };
                udpClient.Client.EnableBroadcast = true;
                udpClient.Client.WindowsUdpBug();
                IAsyncResult result = udpClient.BeginReceive(ReceiveCallbackUdp, asyncUserUdpToken);


                userTokens.AddOrUpdate(port, userToken, (a, b) => userToken);
                udpClients.AddOrUpdate(port, asyncUserUdpToken, (a, b) => asyncUserUdpToken);
            }
            catch (Exception ex)
            {
                Logger.Instance.Error(ex);
            }
        }

        private async void ReceiveCallbackUdp(IAsyncResult result)
        {
            try
            {
                AsyncUserUdpToken token = result.AsyncState as AsyncUserUdpToken;

                IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, IPEndPoint.MinPort);
                byte[] bytes = token.SourceSocket.EndReceive(result, ref endPoint);

                token.Proxy.SourceEP = endPoint;
                token.Proxy.Data = bytes;
                await ConnectUdp(token);
                if (token.Connection != null && token.Proxy.TargetEP != null)
                {
                    //发送连接请求包
                    await SendToConnection(token).ConfigureAwait(false);
                }

                result = token.SourceSocket.BeginReceive(ReceiveCallbackUdp, null);
            }
            catch (Exception)
            {
            }
        }
        protected virtual async Task ConnectUdp(AsyncUserUdpToken token)
        {
            await Task.CompletedTask;
        }

        private async Task SendToConnection(AsyncUserUdpToken token)
        {
            byte[] connectData = token.Proxy.ToBytes(out int length);
            try
            {
                await token.Connection.SendAsync(connectData.AsMemory(0, length)).ConfigureAwait(false);
            }
            catch (Exception)
            {
                CloseClientSocket(token);
            }
            finally
            {
                token.Proxy.Return(connectData);
            }
        }


        private void StartAccept(SocketAsyncEventArgs acceptEventArg)
        {
            acceptEventArg.AcceptSocket = null;
            AsyncUserToken token = (AsyncUserToken)acceptEventArg.UserToken;
            try
            {
                if (token.Socket.AcceptAsync(acceptEventArg) == false)
                {
                    ProcessAccept(acceptEventArg);
                }
            }
            catch (Exception)
            {
                token.Clear();
            }
        }
        private void IO_Completed(object sender, SocketAsyncEventArgs e)
        {
            switch (e.LastOperation)
            {
                case SocketAsyncOperation.Accept:
                    ProcessAccept(e);
                    break;
                case SocketAsyncOperation.Receive:
                    ProcessReceive(e);
                    break;
                default:
                    break;
            }
        }
        private void ProcessAccept(SocketAsyncEventArgs e)
        {
            if (e.AcceptSocket != null)
            {
                BindReceive(e);
                StartAccept(e);
            }
        }
        private void BindReceive(SocketAsyncEventArgs e)
        {
            try
            {
                AsyncUserToken token = (AsyncUserToken)e.UserToken;
                var socket = e.AcceptSocket;

                if (socket == null || socket.RemoteEndPoint == null)
                {
                    return;
                }

                socket.KeepAlive();
                AsyncUserToken userToken = new AsyncUserToken
                {
                    Socket = socket,
                    ListenPort = token.ListenPort,
                    Proxy = new ProxyInfo { Data = Helper.EmptyArray, Step = ProxyStep.Request, ConnectId = ns.Increment() }
                };

                SocketAsyncEventArgs readEventArgs = new SocketAsyncEventArgs
                {
                    UserToken = userToken,
                    SocketFlags = SocketFlags.None,
                };
                userToken.Saea = readEventArgs;

                readEventArgs.SetBuffer(new byte[8 * 1024], 0, 8 * 1024);
                readEventArgs.Completed += IO_Completed;
                if (socket.ReceiveAsync(readEventArgs) == false)
                {
                    ProcessReceive(readEventArgs);
                }
            }
            catch (Exception ex)
            {
                if (Logger.Instance.LoggerLevel <= LoggerTypes.DEBUG)
                    Logger.Instance.Error(ex);
            }
        }
        private async void ProcessReceive(SocketAsyncEventArgs e)
        {
            AsyncUserToken token = (AsyncUserToken)e.UserToken;
            try
            {
                if (e.BytesTransferred > 0 && e.SocketError == SocketError.Success)
                {
                    int offset = e.Offset;
                    int length = e.BytesTransferred;
                    await ReadPacket(token, e.Buffer.AsMemory(offset, length)).ConfigureAwait(false);
                    if (token.Socket.Available > 0)
                    {
                        while (token.Socket.Available > 0)
                        {
                            length = token.Socket.Receive(e.Buffer);
                            if (length > 0)
                            {
                                await ReadPacket(token, e.Buffer.AsMemory(0, length));
                            }
                            else
                            {
                                CloseClientSocket(token);
                                return;
                            }
                        }
                    }

                    if (token.Socket.Connected == false)
                    {
                        CloseClientSocket(token);
                        return;
                    }

                    if (token.Socket.ReceiveAsync(e) == false)
                    {
                        ProcessReceive(e);
                    }
                }
                else
                {
                    CloseClientSocket(token);
                }
            }
            catch (Exception ex)
            {
                if (Logger.Instance.LoggerLevel <= LoggerTypes.DEBUG)
                    Logger.Instance.Error(ex);
                CloseClientSocket(token);
            }
        }
        private async Task ReadPacket(AsyncUserToken token, Memory<byte> data)
        {
            token.Proxy.Data = data;
            if (token.Proxy.Step == ProxyStep.Request)
            {
                bool closeConnect = await ConnectTcp(token);
                if (token.Connection != null)
                {
                    Memory<byte> tempData = token.Proxy.Data;

                    if (token.Proxy.TargetEP != null)
                    {
                        token.Proxy.Data = Helper.EmptyArray;
                        //发送连接请求包
                        await SendToConnection(token).ConfigureAwait(false);
                    }

                    token.Proxy.Step = ProxyStep.Forward;
                    token.Proxy.TargetEP = null;

                    if (tempData.Length > 0)
                    {
                        //发送后续数据包
                        token.Proxy.Data = tempData;
                        await SendToConnection(token).ConfigureAwait(false);
                    }

                    //绑定
                    dic.TryAdd(new ConnectId(token.Proxy.ConnectId, token.Connection.GetHashCode()), token.Socket);
                }
                else if (closeConnect)
                {
                    CloseClientSocket(token);
                }
            }
            else
            {
                await SendToConnection(token).ConfigureAwait(false);
            }
        }
        /// <summary>
        /// 连接到TCP转发
        /// </summary>
        /// <param name="token"></param>
        /// <returns>当未获得通道连接对象，是否关闭连接</returns>
        protected virtual async Task<bool> ConnectTcp(AsyncUserToken token)
        {
            return await Task.FromResult(false);
        }
        private async Task SendToConnection(AsyncUserToken token)
        {
            byte[] connectData = token.Proxy.ToBytes(out int length);
            try
            {
                await token.Connection.SendAsync(connectData.AsMemory(0, length)).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                if (Logger.Instance.LoggerLevel <= LoggerTypes.DEBUG)
                    Logger.Instance.Error(ex);
                CloseClientSocket(token);
            }
            finally
            {
                token.Proxy.Return(connectData);
            }
        }

        protected void BindConnectionReceive(ITunnelConnection connection)
        {
            connection.BeginReceive(InputConnectionData, CloseConnection, new AsyncUserToken
            {
                Connection = connection,
                Buffer = new ReceiveDataBuffer(),
                Proxy = new ProxyInfo { }
            });
        }
        protected async Task InputConnectionData(ITunnelConnection connection, Memory<byte> memory, object userToken)
        {
            AsyncUserToken token = userToken as AsyncUserToken;
            //是一个完整的包
            if (token.Buffer.Size == 0 && memory.Length > 4)
            {
                int packageLen = memory.ToInt32();
                if (packageLen == memory.Length - 4)
                {
                    token.Proxy.DeBytes(memory.Slice(0, packageLen + 4));
                    await ReadConnectionPack(token).ConfigureAwait(false);
                    return;
                }
            }

            //不是完整包
            token.Buffer.AddRange(memory);
            do
            {
                int packageLen = token.Buffer.Data.ToInt32();
                if (packageLen > token.Buffer.Size - 4)
                {
                    break;
                }
                token.Proxy.DeBytes(token.Buffer.Data.Slice(0, packageLen + 4));
                await ReadConnectionPack(token).ConfigureAwait(false);

                token.Buffer.RemoveRange(0, packageLen + 4);
            } while (token.Buffer.Size > 4);
        }
        protected async Task CloseConnection(ITunnelConnection connection, object userToken)
        {
            CloseClientSocket(userToken as AsyncUserToken);
            await Task.CompletedTask;
        }
        private async Task ReadConnectionPack(AsyncUserToken token)
        {
            if (token.Proxy.Step == ProxyStep.Request)
            {
                await ConnectBind(token).ConfigureAwait(false);
            }
            else
            {
                await SendToSocket(token).ConfigureAwait(false);
            }
        }
        private async Task ConnectBind(AsyncUserToken token)
        {
            Socket socket = new Socket(token.Proxy.TargetEP.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            socket.KeepAlive();
            await socket.ConnectAsync(token.Proxy.TargetEP);

            dic.TryAdd(new ConnectId(token.Proxy.ConnectId, token.Connection.GetHashCode()), socket);

            BindReceiveTarget(new AsyncUserToken
            {
                Connection = token.Connection,
                Socket = socket,
                Proxy = new ProxyInfo
                {
                    ConnectId = token.Proxy.ConnectId,
                    Step = ProxyStep.Forward
                }
            });
        }
        private async Task SendToSocket(AsyncUserToken token)
        {
            if (token.Proxy.Protocol == ProxyProtocol.Tcp)
            {
                ConnectId connectId = new ConnectId(token.Proxy.ConnectId, token.Connection.GetHashCode());
                if (token.Proxy.Data.Length > 0)
                {
                    if (dic.TryGetValue(connectId, out Socket source))
                    {
                        try
                        {
                            await source.SendAsync(token.Proxy.Data);
                        }
                        catch (Exception)
                        {
                            CloseClientSocket(token);
                        }
                    }
                }
                else
                {

                    if (dic.TryRemove(connectId, out Socket source))
                    {
                        CloseClientSocket(token);
                    }
                }
            }
            else
            {
                if (token.Proxy.Direction == ProxyDirection.Forward)
                {
                    ConnectIdUdp connectId = new ConnectIdUdp(token.Proxy.ConnectId, token.Proxy.SourceEP, token.Connection.GetHashCode());
                    try
                    {

                        if (dicUdp.TryGetValue(connectId, out Socket socket))
                        {
                            await socket.SendToAsync(token.Proxy.Data, token.Proxy.TargetEP);
                            return;
                        }

                        socket = new Socket(token.Proxy.TargetEP.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
                        socket.WindowsUdpBug();
                        AsyncUserUdpTokenTarget udpToken = new AsyncUserUdpTokenTarget
                        {
                            Proxy = new ProxyInfo
                            {
                                ConnectId = token.Proxy.ConnectId,
                                Direction = ProxyDirection.Reverse,
                                Protocol = token.Proxy.Protocol,
                                SourceEP = token.Proxy.SourceEP,
                                TargetEP = token.Proxy.TargetEP,
                                Step = token.Proxy.Step,
                            },
                            TargetSocket = socket,
                            ConnectId = connectId,
                            Connection = token.Connection
                        };
                        udpToken.Proxy.Direction = ProxyDirection.Reverse;
                        udpToken.PoolBuffer = new byte[65535];
                        dicUdp.AddOrUpdate(connectId, socket, (a, b) => socket);

                        await udpToken.TargetSocket.SendToAsync(token.Proxy.Data, SocketFlags.None, token.Proxy.TargetEP);
                        IAsyncResult result = socket.BeginReceiveFrom(udpToken.PoolBuffer, 0, udpToken.PoolBuffer.Length, SocketFlags.None, ref udpToken.TempRemoteEP, ReceiveCallbackUdpTarget, udpToken);
                    }
                    catch (Exception)
                    {
                        if (dicUdp.TryRemove(connectId, out Socket socket))
                        {
                            socket?.SafeClose();
                        }
                    }
                }
                else
                {
                    if (udpClients.TryGetValue(token.ListenPort, out AsyncUserUdpToken asyncUserUdpToken))
                    {
                        try
                        {
                            if (await ConnectionReceiveUdp(token, asyncUserUdpToken) == false)
                            {
                                await asyncUserUdpToken.SourceSocket.SendAsync(token.Proxy.Data, token.Proxy.SourceEP);
                            }
                        }
                        catch (Exception)
                        {
                        }
                    }
                }
            }
        }
        protected virtual async Task<bool> ConnectionReceiveUdp(AsyncUserToken token, AsyncUserUdpToken asyncUserUdpToken)
        {
            return await Task.FromResult(false);
        }


        private async void ReceiveCallbackUdpTarget(IAsyncResult result)
        {
            AsyncUserUdpTokenTarget token = result.AsyncState as AsyncUserUdpTokenTarget;
            try
            {
                int length = token.TargetSocket.EndReceiveFrom(result, ref token.TempRemoteEP);

                if (length > 0)
                {
                    token.Proxy.Data = token.PoolBuffer.AsMemory(0, length);

                    token.Update();
                    await SendToConnection(token);
                    token.Proxy.Data = Helper.EmptyArray;
                }
                result = token.TargetSocket.BeginReceiveFrom(token.PoolBuffer, 0, token.PoolBuffer.Length, SocketFlags.None, ref token.TempRemoteEP, ReceiveCallbackUdp, token);
            }
            catch (Exception ex)
            {
                if (Logger.Instance.LoggerLevel <= LoggerTypes.DEBUG)
                {
                    Logger.Instance.Error($"socks5 forward udp -> receive" + ex);
                }
                CloseClientSocket(token);
            }
        }
        private async Task SendToConnection(AsyncUserUdpTokenTarget token)
        {
            byte[] connectData = token.Proxy.ToBytes(out int length);
            try
            {
                await token.Connection.SendAsync(connectData.AsMemory(0, length)).ConfigureAwait(false);
            }
            catch (Exception)
            {
                CloseClientSocket(token);
            }
            finally
            {
                token.Proxy.Return(connectData);
            }
        }


        private void IO_CompletedTarget(object sender, SocketAsyncEventArgs e)
        {
            switch (e.LastOperation)
            {
                case SocketAsyncOperation.Receive:
                    ProcessReceiveTarget(e);
                    break;
                default:
                    break;
            }
        }
        private void BindReceiveTarget(AsyncUserToken userToken)
        {
            try
            {
                SocketAsyncEventArgs readEventArgs = new SocketAsyncEventArgs
                {
                    UserToken = userToken,
                    SocketFlags = SocketFlags.None,
                };
                readEventArgs.SetBuffer(new byte[8 * 1024], 0, 8 * 1024);
                readEventArgs.Completed += IO_CompletedTarget;
                if (userToken.Socket.ReceiveAsync(readEventArgs) == false)
                {
                    ProcessReceiveTarget(readEventArgs);
                }
            }
            catch (Exception ex)
            {
                if (Logger.Instance.LoggerLevel <= LoggerTypes.DEBUG)
                    Logger.Instance.Error(ex);
            }
        }
        private async void ProcessReceiveTarget(SocketAsyncEventArgs e)
        {
            AsyncUserToken token = (AsyncUserToken)e.UserToken;
            try
            {
                if (e.BytesTransferred > 0 && e.SocketError == SocketError.Success)
                {
                    int offset = e.Offset;
                    int length = e.BytesTransferred;

                    token.Proxy.Data = e.Buffer.AsMemory(offset, length);
                    await SendToConnection(token).ConfigureAwait(false);

                    if (token.Socket.Available > 0)
                    {
                        while (token.Socket.Available > 0)
                        {
                            length = token.Socket.Receive(e.Buffer);

                            if (length > 0)
                            {
                                token.Proxy.Data = e.Buffer.AsMemory(0, length);
                                await SendToConnection(token).ConfigureAwait(false);
                            }
                            else
                            {
                                //token.Proxy.Data = Helper.EmptyArray;
                                //await SendToConnection(token).ConfigureAwait(false);
                                CloseClientSocket(token);
                                return;
                            }
                        }
                    }

                    if (token.Connection.Connected == false)
                    {
                        //token.Proxy.Data = Helper.EmptyArray;
                        //await SendToConnection(token).ConfigureAwait(false);
                        CloseClientSocket(token);
                        return;
                    }

                    if (token.Socket.ReceiveAsync(e) == false)
                    {
                        ProcessReceiveTarget(e);
                    }
                }
                else
                {
                    //token.Proxy.Data = Helper.EmptyArray;
                    //await SendToConnection(token).ConfigureAwait(false);
                    CloseClientSocket(token);
                }
            }
            catch (Exception ex)
            {
                if (Logger.Instance.LoggerLevel <= LoggerTypes.DEBUG)
                    Logger.Instance.Error(ex);
                //token.Proxy.Data = Helper.EmptyArray;
               // await SendToConnection(token).ConfigureAwait(false);
                CloseClientSocket(token);
            }
        }

        private void CloseClientSocket(AsyncUserToken token)
        {
            if (token == null) return;
            if (token.Connection != null)
            {
                int code = token.Connection.GetHashCode();
                if (token.Connection.Connected == false)
                {
                    foreach (ConnectId item in dic.Keys.Where(c => c.hashCode == code).ToList())
                    {
                        if (dic.TryRemove(item, out Socket socket))
                        {
                            socket?.SafeClose();
                        }
                    }
                }
                else
                {
                    dic.TryRemove(new ConnectId(token.Proxy.ConnectId, code), out _);
                }
            }
            token.Clear();
        }
        private void CloseClientSocket(AsyncUserUdpToken token)
        {
            if (token == null) return;
            token.Clear();
        }
        private void CloseClientSocket(AsyncUserUdpTokenTarget token)
        {
            if (token == null) return;
            if (dicUdp.TryRemove(token.ConnectId, out _))
            {
                token.Clear();
            }
            token.Clear();
        }
        public virtual void Stop()
        {
            foreach (var item in userTokens)
            {
                CloseClientSocket(item.Value);
            }
            userTokens.Clear();
            foreach (var item in udpClients)
            {
                item.Value.Clear();
            }
            udpClients.Clear();

            foreach (var item in dic)
            {
                item.Value?.SafeClose();
            }
            dic.Clear();

            foreach (var item in dicUdp)
            {
                item.Value?.SafeClose();
            }
            dicUdp.Clear();
        }

        public virtual void Stop(int port)
        {
            if (userTokens.TryRemove(port, out AsyncUserToken userToken))
            {
                CloseClientSocket(userToken);
            }
            if (udpClients.TryRemove(port, out AsyncUserUdpToken udpClient))
            {
                udpClient.Clear();
            }

            if (userTokens.Count == 0 && udpClients.Count == 0)
            {
                foreach (var item in dic)
                {
                    item.Value?.SafeClose();
                }
                dic.Clear();

                foreach (var item in dicUdp)
                {
                    item.Value?.SafeClose();
                }
                dicUdp.Clear();
            }
        }

    }

    public enum ProxyStep : byte
    {
        Request = 1,
        Forward = 2
    }
    public enum ProxyProtocol : byte
    {
        Tcp = 0,
        Udp = 1
    }
    public enum ProxyDirection : byte
    {
        Forward = 0,
        Reverse = 1
    }
    public record struct ConnectId
    {
        public ulong connectId;
        public int hashCode;

        public ConnectId(ulong connectId, int hashCode)
        {
            this.connectId = connectId;
            this.hashCode = hashCode;
        }
    }
    public sealed class ProxyInfo
    {
        public ulong ConnectId { get; set; }
        public ProxyStep Step { get; set; } = ProxyStep.Request;
        public ProxyProtocol Protocol { get; set; } = ProxyProtocol.Tcp;
        public ProxyDirection Direction { get; set; } = ProxyDirection.Forward;
        public IPEndPoint SourceEP { get; set; }
        public IPEndPoint TargetEP { get; set; }

        public byte Rsv { get; set; }

        public Memory<byte> Data { get; set; }

        public byte[] ToBytes(out int length)
        {
            int sourceLength = SourceEP == null ? 0 : (SourceEP.AddressFamily == AddressFamily.InterNetwork ? 4 : 16) + 2;
            int targetLength = TargetEP == null ? 0 : (TargetEP.AddressFamily == AddressFamily.InterNetwork ? 4 : 16) + 2;

            length = 4 + 8 + 1 + 1 + 1
                + 1 + sourceLength
                + 1 + targetLength
                + Data.Length;

            byte[] bytes = ArrayPool<byte>.Shared.Rent(length);
            Memory<byte> memory = bytes.AsMemory();

            int index = 0;

            (length - 4).ToBytes(memory);
            index += 4;


            ConnectId.ToBytes(memory.Slice(index));
            index += 8;

            bytes[index] = (byte)Step;
            index += 1;

            bytes[index] = (byte)Protocol;
            index += 1;

            bytes[index] = (byte)Direction;
            index += 1;

            bytes[index] = (byte)sourceLength;
            index += 1;

            if (sourceLength > 0)
            {
                SourceEP.Address.TryWriteBytes(memory.Slice(index).Span, out int writeLength);
                index += writeLength;

                ((ushort)SourceEP.Port).ToBytes(memory.Slice(index));
                index += 2;
            }


            bytes[index] = (byte)targetLength;
            index += 1;

            if (targetLength > 0)
            {
                TargetEP.Address.TryWriteBytes(memory.Slice(index).Span, out int writeLength);
                index += writeLength;

                ((ushort)TargetEP.Port).ToBytes(memory.Slice(index));
                index += 2;
            }

            Data.CopyTo(memory.Slice(index));

            return bytes;

        }

        public void Return(byte[] bytes)
        {
            ArrayPool<byte>.Shared.Return(bytes);
        }

        public void DeBytes(Memory<byte> memory)
        {
            int index = 4;
            Span<byte> span = memory.Span;

            ConnectId = memory.Slice(index).ToUInt64();
            index += 8;

            Step = (ProxyStep)span[index];
            index += 1;

            Protocol = (ProxyProtocol)span[index];
            index += 1;

            Direction = (ProxyDirection)span[index];
            index += 1;

            byte sourceLength = span[index];
            index += 1;
            if (sourceLength > 0)
            {
                IPAddress ip = new IPAddress(span.Slice(index, sourceLength - 2));
                index += sourceLength;
                ushort port = span.Slice(index - 2).ToUInt16();
                SourceEP = new IPEndPoint(ip, port);
            }

            byte targetLength = span[index];
            index += 1;
            if (targetLength > 0)
            {
                IPAddress ip = new IPAddress(span.Slice(index, targetLength - 2));
                index += targetLength;
                ushort port = span.Slice(index - 2).ToUInt16();
                TargetEP = new IPEndPoint(ip, port);
            }
            Data = memory.Slice(index);
        }
    }

    public sealed class AsyncUserUdpToken
    {
        public int ListenPort { get; set; }
        public UdpClient SourceSocket { get; set; }
        public ITunnelConnection Connection { get; set; }
        public ProxyInfo Proxy { get; set; }

        public void Clear()
        {
            SourceSocket?.Close();
            SourceSocket = null;
            GC.Collect();
        }
    }
    public sealed class AsyncUserUdpTokenTarget
    {
        public Socket TargetSocket { get; set; }
        public byte[] PoolBuffer { get; set; }

        public ITunnelConnection Connection { get; set; }
        public ProxyInfo Proxy { get; set; }

        public ConnectIdUdp ConnectId { get; set; }

        public int LastTime { get; set; } = Environment.TickCount;
        public EndPoint TempRemoteEP = new IPEndPoint(IPAddress.Any, IPEndPoint.MinPort);
        public void Clear()
        {
            TargetSocket?.SafeClose();
            PoolBuffer = Helper.EmptyArray;
            GC.Collect();
            GC.SuppressFinalize(this);
        }
        public void Update()
        {
            LastTime = Environment.TickCount;
        }
    }

    public sealed class AsyncUserToken
    {
        public int ListenPort { get; set; }
        public Socket Socket { get; set; }
        public ITunnelConnection Connection { get; set; }

        public ProxyInfo Proxy { get; set; }

        public ReceiveDataBuffer Buffer { get; set; }

        public SocketAsyncEventArgs Saea { get; set; }

        public void Clear()
        {
            Socket?.SafeClose();

            //Buffer?.Clear();

            Saea?.Dispose();

            GC.Collect();
        }
    }

    public sealed class ConnectIdUdpComparer : IEqualityComparer<ConnectIdUdp>
    {
        public bool Equals(ConnectIdUdp x, ConnectIdUdp y)
        {
            return x.Source != null && x.Source.Equals(y.Source) && x.ConnectId == y.ConnectId && x.HashCode == y.HashCode;
        }
        public int GetHashCode(ConnectIdUdp obj)
        {
            if (obj.Source == null) return 0;
            return obj.Source.GetHashCode() ^ obj.ConnectId.GetHashCode() ^ obj.HashCode;
        }
    }
    public readonly struct ConnectIdUdp
    {
        public readonly IPEndPoint Source { get; }
        public readonly ulong ConnectId { get; }
        public int HashCode { get; }

        public ConnectIdUdp(ulong connectId, IPEndPoint source, int hashCode)
        {
            ConnectId = connectId;
            Source = source;
            HashCode = hashCode;
        }
    }
}