﻿using linker.config;
using linker.plugins.relay.client.transport;
using linker.tunnel.connection;
using linker.libs;
using linker.libs.extends;
using System.Collections.Concurrent;

namespace linker.plugins.relay.client
{
    /// <summary>
    /// 中继
    /// </summary>
    public sealed class RelayTransfer
    {
        public List<ITransport> Transports { get; private set; }


        private readonly FileConfig fileConfig;

        private ConcurrentDictionary<string, bool> connectingDic = new ConcurrentDictionary<string, bool>();
        private Dictionary<string, List<Action<ITunnelConnection>>> OnConnected { get; } = new Dictionary<string, List<Action<ITunnelConnection>>>();

        public RelayTransfer(FileConfig fileConfig)
        {
            this.fileConfig = fileConfig;
        }

        public void LoadTransports(List<ITransport> list)
        {
            Transports = list;
        }

        /// <summary>
        /// 设置中继成功回调
        /// </summary>
        /// <param name="transactionId">事务</param>
        /// <param name="callback"></param>
        public void SetConnectedCallback(string transactionId, Action<ITunnelConnection> callback)
        {
            if (OnConnected.TryGetValue(transactionId, out List<Action<ITunnelConnection>> callbacks) == false)
            {
                callbacks = new List<Action<ITunnelConnection>>();
                OnConnected[transactionId] = callbacks;
            }
            callbacks.Add(callback);
        }
        /// <summary>
        /// 一处中继成功回调
        /// </summary>
        /// <param name="transactionId">事务</param>
        /// <param name="callback"></param>
        public void RemoveConnectedCallback(string transactionId, Action<ITunnelConnection> callback)
        {
            if (OnConnected.TryGetValue(transactionId, out List<Action<ITunnelConnection>> callbacks))
            {
                callbacks.Remove(callback);
            }
        }
        /// <summary>
        /// 中继连接对方
        /// </summary>
        /// <param name="fromMachineId">自己的id</param>
        /// <param name="remoteMachineId">对方id</param>
        /// <param name="transactionId">事务</param>
        /// <returns></returns>
        public async Task<ITunnelConnection> ConnectAsync(string fromMachineId, string remoteMachineId, string transactionId, string nodeId = "")
        {
            if (connectingDic.TryAdd(remoteMachineId, true) == false)
            {
                return null;
            }
            try
            {
                ITransport transport = Transports.FirstOrDefault(c => c.Type == fileConfig.Data.Client.Relay.Server.RelayType && fileConfig.Data.Client.Relay.Server.Disabled == false);
                if (transport == null)
                {
                    return null;
                }

                transport.RelayInfo relayInfo = new transport.RelayInfo
                {
                    FlowingId = 0,
                    FromMachineId = fromMachineId,
                    FromMachineName = string.Empty,
                    RemoteMachineId = remoteMachineId,
                    RemoteMachineName = string.Empty,
                    SecretKey = fileConfig.Data.Client.Relay.Server.SecretKey,
                    TransactionId = transactionId,
                    TransportName = transport.Name,
                    SSL = fileConfig.Data.Client.Relay.Server.SSL,
                    NodeId = nodeId
                };

                if (LoggerHelper.Instance.LoggerLevel <= LoggerTypes.DEBUG)
                    LoggerHelper.Instance.Info($"relay to {relayInfo.RemoteMachineId}->{relayInfo.RemoteMachineName} {relayInfo.ToJson()}");
                ITunnelConnection connection = await transport.RelayAsync(relayInfo).ConfigureAwait(false);
                if (connection != null)
                {
                    if (LoggerHelper.Instance.LoggerLevel <= LoggerTypes.DEBUG)
                        LoggerHelper.Instance.Debug($"relay to {relayInfo.RemoteMachineId}->{relayInfo.RemoteMachineName} success,{relayInfo.ToJson()}");
                    ConnectedCallback(relayInfo, connection);
                    return connection;
                }
                else
                {
                    if (LoggerHelper.Instance.LoggerLevel <= LoggerTypes.DEBUG)
                        LoggerHelper.Instance.Error($"relay to {relayInfo.RemoteMachineId}->{relayInfo.RemoteMachineName} fail,{relayInfo.ToJson()}");
                }

            }
            catch (Exception ex)
            {
                LoggerHelper.Instance.Error(ex);
            }
            finally
            {
                connectingDic.TryRemove(remoteMachineId, out _);
            }

            return null;
        }
        /// <summary>
        /// 收到对方的中继请求
        /// </summary>
        /// <param name="relayInfo"></param>
        /// <returns></returns>
        public async Task<bool> OnBeginAsync(transport.RelayInfo relayInfo)
        {
            if (connectingDic.TryAdd(relayInfo.FromMachineId, true) == false)
            {
                return false;
            }

            try
            {
                ITransport _transports = Transports.FirstOrDefault(c => c.Name == relayInfo.TransportName);
                if (_transports == null) return false;

                await _transports.OnBeginAsync(relayInfo, (connection) =>
                {
                    if (connection != null)
                    {
                        if (LoggerHelper.Instance.LoggerLevel <= LoggerTypes.DEBUG)
                            LoggerHelper.Instance.Debug($"relay from {relayInfo.RemoteMachineId}->{relayInfo.RemoteMachineName} success,{relayInfo.ToJson()}");
                        ConnectedCallback(relayInfo, connection);
                    }
                    else
                    {
                        if (LoggerHelper.Instance.LoggerLevel <= LoggerTypes.DEBUG)
                            LoggerHelper.Instance.Error($"relay from {relayInfo.RemoteMachineId}->{relayInfo.RemoteMachineName} error,{relayInfo.ToJson()}");
                    }
                }).ConfigureAwait(false);
                return true;
            }
            catch (Exception ex)
            {
                if (LoggerHelper.Instance.LoggerLevel <= LoggerTypes.DEBUG)
                {
                    LoggerHelper.Instance.Error(ex);
                }
            }
            finally
            {
                connectingDic.TryRemove(relayInfo.FromMachineId, out _);
            }
            return false;
        }

        /// <summary>
        /// 回调
        /// </summary>
        /// <param name="relayInfo"></param>
        /// <param name="connection"></param>
        private void ConnectedCallback(transport.RelayInfo relayInfo, ITunnelConnection connection)
        {
            if (OnConnected.TryGetValue(Helper.GlobalString, out List<Action<ITunnelConnection>> callbacks))
            {
                foreach (var item in callbacks)
                {
                    item(connection);
                }
            }
            if (OnConnected.TryGetValue(connection.TransactionId, out callbacks))
            {
                foreach (var callabck in callbacks)
                {
                    callabck(connection);
                }
            }
        }


    }
}