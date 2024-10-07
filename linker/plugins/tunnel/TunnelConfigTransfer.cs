﻿using linker.client.config;
using linker.config;
using linker.libs;
using linker.plugins.client;
using linker.plugins.messenger;
using linker.plugins.tunnel.messenger;
using linker.tunnel;
using linker.tunnel.adapter;
using linker.tunnel.transport;
using linker.tunnel.wanport;
using MemoryPack;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Quic;

namespace linker.plugins.tunnel
{
    public sealed class TunnelConfigTransfer
    {
        private readonly FileConfig config;
        private readonly RunningConfig running;
        private readonly ClientSignInState clientSignInState;
        private readonly MessengerSender messengerSender;
        private readonly ITunnelAdapter tunnelAdapter;
        private readonly TransportTcpPortMap transportTcpPortMap;
        private readonly TransportUdpPortMap transportUdpPortMap;
        private readonly TunnelUpnpTransfer upnpTransfer;

        public VersionManager Version { get; } = new VersionManager();
        public ConcurrentDictionary<string, TunnelTransportRouteLevelInfo> Config { get; } = new ConcurrentDictionary<string, TunnelTransportRouteLevelInfo>();

        public TunnelConfigTransfer(FileConfig config, RunningConfig running, ClientSignInState clientSignInState, MessengerSender messengerSender, ITunnelAdapter tunnelAdapter, TransportTcpPortMap transportTcpPortMap, TransportUdpPortMap transportUdpPortMap, TunnelUpnpTransfer upnpTransfer)
        {
            this.config = config;
            this.running = running;
            this.clientSignInState = clientSignInState;
            this.messengerSender = messengerSender;
            this.tunnelAdapter = tunnelAdapter;
            this.transportTcpPortMap = transportTcpPortMap;
            this.transportUdpPortMap = transportUdpPortMap;
            this.upnpTransfer = upnpTransfer;

            InitRouteLevel();

            InitConfig();

            TestQuic();

            RefreshPortMap();
        }

        private void RefreshRouteLevel()
        {
            TimerHelper.Async(() =>
            {
                config.Data.Client.Tunnel.RouteLevel = NetworkHelper.GetRouteLevel(config.Data.Client.ServerInfo.Host, out List<IPAddress> ips);
                config.Data.Client.Tunnel.RouteIPs = ips.ToArray();
                config.Data.Client.Tunnel.LocalIPs = NetworkHelper.GetIPV6().Concat(NetworkHelper.GetIPV4()).ToArray();
            });
        }

        private void InitConfig()
        {
            bool updateVersion = false;
            List<TunnelWanPortInfo> server = config.Data.Client.Tunnel.Servers;
            if (server.FirstOrDefault(c => c.Type == TunnelWanPortType.Linker && c.ProtocolType == TunnelWanPortProtocolType.Udp) == null)
            {
                server.Add(new TunnelWanPortInfo
                {
                    Name = "Linker Udp",
                    Type = TunnelWanPortType.Linker,
                    ProtocolType = TunnelWanPortProtocolType.Udp,
                    Disabled = false,
                    Host = config.Data.Client.ServerInfo.Host,
                });
                updateVersion = true;
            }
            if (server.FirstOrDefault(c => c.Type == TunnelWanPortType.Linker && c.ProtocolType == TunnelWanPortProtocolType.Tcp) == null)
            {
                server.Add(new TunnelWanPortInfo
                {
                    Name = "Linker Tcp",
                    Type = TunnelWanPortType.Linker,
                    ProtocolType = TunnelWanPortProtocolType.Tcp,
                    Disabled = false,
                    Host = config.Data.Client.ServerInfo.Host,
                });
                updateVersion = true;
            }
            tunnelAdapter.SetTunnelWanPortProtocols(server, updateVersion);
        }

        /// <summary>
        /// 刷新关于隧道的配置信息，也就是获取自己的和别的客户端的，方便查看
        /// </summary>
        public void RefreshConfig()
        {
            GetRemoteRouteLevel();
        }
        /// <summary>
        /// 修改自己的网关层级信息
        /// </summary>
        /// <param name="tunnelTransportFileConfigInfo"></param>
        public void OnLocalRouteLevel(TunnelTransportRouteLevelInfo tunnelTransportFileConfigInfo)
        {
            running.Data.Tunnel.RouteLevelPlus = tunnelTransportFileConfigInfo.RouteLevelPlus;
            running.Data.Tunnel.PortMapWan = tunnelTransportFileConfigInfo.PortMapWan;
            running.Data.Tunnel.PortMapLan = tunnelTransportFileConfigInfo.PortMapLan;
            running.Data.Update();
            GetRemoteRouteLevel();
            RefreshPortMap();
        }
        /// <summary>
        /// 收到别人的信息
        /// </summary>
        /// <param name="tunnelTransportFileConfigInfo"></param>
        /// <returns></returns>
        public TunnelTransportRouteLevelInfo OnRemoteRouteLevel(TunnelTransportRouteLevelInfo tunnelTransportFileConfigInfo)
        {
            Config.AddOrUpdate(tunnelTransportFileConfigInfo.MachineId, tunnelTransportFileConfigInfo, (a, b) => tunnelTransportFileConfigInfo);
            Version.Add();
            return GetLocalRouteLevel();
        }
        private void GetRemoteRouteLevel()
        {
            TunnelTransportRouteLevelInfo config = GetLocalRouteLevel();
            messengerSender.SendReply(new MessageRequestWrap
            {
                Connection = clientSignInState.Connection,
                MessengerId = (ushort)TunnelMessengerIds.ConfigForward,
                Timeout = 10000,
                Payload = MemoryPackSerializer.Serialize(config)
            }).ContinueWith((result) =>
            {
                if (result.Result.Code == MessageResponeCodes.OK)
                {
                    List<TunnelTransportRouteLevelInfo> list = MemoryPackSerializer.Deserialize<List<TunnelTransportRouteLevelInfo>>(result.Result.Data.Span);
                    foreach (var item in list)
                    {
                        Config.AddOrUpdate(item.MachineId, item, (a, b) => item);
                    }
                    TunnelTransportRouteLevelInfo config = GetLocalRouteLevel();
                    Config.AddOrUpdate(config.MachineId, config, (a, b) => config);
                    Version.Add();
                }
            });
        }
        private TunnelTransportRouteLevelInfo GetLocalRouteLevel()
        {
            return new TunnelTransportRouteLevelInfo
            {
                MachineId = config.Data.Client.Id,
                RouteLevel = config.Data.Client.Tunnel.RouteLevel,
                RouteLevelPlus = running.Data.Tunnel.RouteLevelPlus,
                PortMapWan = running.Data.Tunnel.PortMapWan,
                PortMapLan = running.Data.Tunnel.PortMapLan,
                NeedReboot = false
            };
        }
        private void InitRouteLevel()
        {
            clientSignInState.NetworkEnabledHandle += (times) =>
            {
                RefreshRouteLevel();
                GetRemoteRouteLevel();
            };
        }


        public void SetInterface(IPAddress ip)
        {
            running.Data.Tunnel.Interface = ip;
            running.Data.Update();
        }

        private void TestQuic()
        {
            if (OperatingSystem.IsWindows())
            {
                if (QuicListener.IsSupported == false)
                {
                    try
                    {
                        if (File.Exists("msquic-openssl.dll"))
                        {
                            LoggerHelper.Instance.Warning($"copy msquic-openssl.dll -> msquic.dll，please restart linker");
                            File.Move("msquic.dll", "msquic.dll.temp", true);
                            File.Move("msquic-openssl.dll", "msquic.dll", true);

                            if (Environment.UserInteractive == false && OperatingSystem.IsWindows())
                            {
                                Environment.Exit(1);
                            }
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
                try
                {
                    if (File.Exists("msquic.dll.temp"))
                    {
                        File.Delete("msquic.dll.temp");
                    }
                    if (File.Exists("msquic-openssl.dll"))
                    {
                        File.Delete("msquic-openssl.dll");
                    }
                }
                catch (Exception)
                {
                }
            }
        }


        private void RefreshPortMap()
        {
            try
            {
                int port = 18180;
                int ip = Dns.GetHostEntry(Dns.GetHostName()).AddressList.FirstOrDefault(c => c.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork).GetAddressBytes()[3];
                upnpTransfer.SetMap(port, port + ip);
            }
            catch (Exception)
            {
            }

            if (running.Data.Tunnel.PortMapLan > 0)
            {
                _ = transportTcpPortMap.Listen(running.Data.Tunnel.PortMapLan);
                _ = transportUdpPortMap.Listen(running.Data.Tunnel.PortMapLan);
            }
            else if (upnpTransfer.MapInfo != null)
            {
                _ = transportTcpPortMap.Listen(upnpTransfer.MapInfo.PrivatePort);
                _ = transportUdpPortMap.Listen(upnpTransfer.MapInfo.PrivatePort);
            }

        }
    }
}
