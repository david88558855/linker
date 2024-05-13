﻿using cmonitor.client.running;
using cmonitor.client.tunnel;
using cmonitor.plugins.relay;
using cmonitor.plugins.tunnel;
using common.libs;

namespace cmonitor.plugins.viewer.proxy
{
    public sealed class ViewerProxy : TunnelProxy
    {
        private readonly RunningConfig runningConfig;
        private readonly TunnelTransfer tunnelTransfer;
        private readonly RelayTransfer relayTransfer;

        private ITunnelConnection connection;

        public ViewerProxy(RunningConfig runningConfig, TunnelTransfer tunnelTransfer, RelayTransfer relayTransfer)
        {
            this.runningConfig = runningConfig;
            this.tunnelTransfer = tunnelTransfer;
            this.relayTransfer = relayTransfer;

            Start(0);
            Logger.Instance.Info($"start viewer proxy, listen port : {LocalEndpoint}");

            tunnelTransfer.SetConnectCallback("viewer", BindConnectionReceive);
            relayTransfer.SetConnectCallback("viewer", BindConnectionReceive);
        }

        protected override async Task<bool> ConnectTcp(AsyncUserToken token)
        {
            token.Proxy.TargetEP = runningConfig.Data.Viewer.ConnectEP;
            token.Connection = connection;
            if (connection == null || connection.Connected == false)
            {
                Logger.Instance.Debug($"viewer tunnel to {runningConfig.Data.Viewer.ServerMachine}");
                connection = await tunnelTransfer.ConnectAsync(runningConfig.Data.Viewer.ServerMachine, "viewer");
                if (connection != null)
                {
                    Logger.Instance.Debug($"viewer tunnel to {runningConfig.Data.Viewer.ServerMachine} success");
                }
                if (connection == null)
                {
                    Logger.Instance.Debug($"viewer relay to {runningConfig.Data.Viewer.ServerMachine}");
                    connection = await relayTransfer.ConnectAsync(runningConfig.Data.Viewer.ServerMachine, "viewer");
                    if (connection != null)
                    {
                        Logger.Instance.Debug($"viewer relay to {runningConfig.Data.Viewer.ServerMachine} success");
                    }
                }
                if (connection != null)
                {
                    BindConnectionReceive(connection);
                    token.Connection = connection;
                }
            }
            return token.Connection != null && token.Connection.Connected;
        }
    }
}
