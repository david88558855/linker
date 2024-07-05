﻿using linker.client;
using linker.client.capi;
using linker.config;
using linker.plugins.tunnel.messenger;
using linker.server;
using linker.tunnel.adapter;
using linker.tunnel.transport;
using linker.libs.api;
using linker.libs.extends;
using MemoryPack;
using System.Collections.Concurrent;
using linker.tunnel.wanport;
using linker.client.config;

namespace linker.plugins.tunnel
{
    /// <summary>
    /// 管理接口
    /// </summary>
    public sealed class TunnelApiController : IApiClientController
    {
        private readonly ConfigWrap config;
        private readonly TunnelWanPortTransfer compactTransfer;
        private readonly ClientSignInState clientSignInState;
        private readonly MessengerSender messengerSender;
        private readonly TunnelConfigTransfer tunnelConfigTransfer;
        private readonly ITunnelAdapter tunnelMessengerAdapter;

        public TunnelApiController(ConfigWrap config, TunnelWanPortTransfer compactTransfer, ClientSignInState clientSignInState, MessengerSender messengerSender, TunnelConfigTransfer tunnelConfigTransfer, ITunnelAdapter tunnelMessengerAdapter)
        {
            this.config = config;
            this.compactTransfer = compactTransfer;
            this.clientSignInState = clientSignInState;
            this.messengerSender = messengerSender;
            this.tunnelConfigTransfer = tunnelConfigTransfer;
            this.tunnelMessengerAdapter = tunnelMessengerAdapter;
        }

        /// <summary>
        /// 获取所有人的隧道信息
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public TunnelListInfo Get(ApiControllerParamsInfo param)
        {
            uint hashCode = uint.Parse(param.Content);
            uint _hashCode = tunnelConfigTransfer.ConfigVersion;
            if (_hashCode != hashCode)
            {
                return new TunnelListInfo
                {
                    List = tunnelConfigTransfer.Config,
                    HashCode = _hashCode
                };
            }
            return new TunnelListInfo { HashCode = _hashCode };
        }
        /// <summary>
        /// 刷新隧道信息
        /// </summary>
        /// <param name="param"></param>
        public void Refresh(ApiControllerParamsInfo param)
        {
            tunnelConfigTransfer.RefreshConfig();
        }

        /// <summary>
        /// 获取所有外网端口协议
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public List<TunnelWanPortTypeInfo> GetTypes(ApiControllerParamsInfo param)
        {
            return compactTransfer.GetTypes();
        }
        /// <summary>
        /// 设置外网端口协议
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public bool SetServers(ApiControllerParamsInfo param)
        {
            List<TunnelWanPortInfo> info = param.Content.DeJson<List<TunnelWanPortInfo>>();
            tunnelMessengerAdapter.SetTunnelWanPortProtocols(info);
            return true;

        }
        /// <summary>
        /// 设置网关层级
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public async Task<bool> SetRouteLevel(ApiControllerParamsInfo param)
        {
            TunnelTransportRouteLevelInfo tunnelTransportConfigWrapInfo = param.Content.DeJson<TunnelTransportRouteLevelInfo>();

            if (tunnelTransportConfigWrapInfo.MachineId == config.Data.Client.Id)
            {
                tunnelConfigTransfer.OnLocalRouteLevel(tunnelTransportConfigWrapInfo);
            }
            else
            {
                await messengerSender.SendOnly(new MessageRequestWrap
                {
                    Connection = clientSignInState.Connection,
                    MessengerId = (ushort)TunnelMessengerIds.RouteLevelForward,
                    Payload = MemoryPackSerializer.Serialize(tunnelTransportConfigWrapInfo)
                }).ConfigureAwait(false);
            }

            return true;
        }
        /// <summary>
        /// 获取打洞协议
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public List<TunnelTransportItemInfo> GetTransports(ApiControllerParamsInfo param)
        {
            return tunnelMessengerAdapter.GetTunnelTransports();
        }
        /// <summary>
        /// 设置打洞协议
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public bool SetTransports(ApiControllerParamsInfo param)
        {
            List<TunnelTransportItemInfo> info = param.Content.DeJson<List<TunnelTransportItemInfo>>();
            tunnelMessengerAdapter.SetTunnelTransports(info);
            return true;
        }

        public ExcludeIPItem[] GetExcludeIPs(ApiControllerParamsInfo param)
        {
            return tunnelConfigTransfer.GetExcludeIPs();
        }
        public void SetExcludeIPs(ApiControllerParamsInfo param)
        {
            ExcludeIPItem[] info = param.Content.DeJson<ExcludeIPItem[]>();
            tunnelConfigTransfer.SettExcludeIPs(info);
        }


        public sealed class TunnelListInfo
        {
            public ConcurrentDictionary<string, TunnelTransportRouteLevelInfo> List { get; set; }
            public uint HashCode { get; set; }
        }

    }

}
