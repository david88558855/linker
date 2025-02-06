﻿using linker.libs;
using System.Net;
using linker.tun;

namespace linker.messenger.tuntap
{
    public sealed class TuntapTransfer
    {
        private readonly LinkerTunDeviceAdapter linkerTunDeviceAdapter;

        private OperatingManager operatingManager = new OperatingManager();
        public TuntapStatus Status => operatingManager.Operating ? TuntapStatus.Operating : (TuntapStatus)(byte)linkerTunDeviceAdapter.Status;
        public string SetupError => linkerTunDeviceAdapter.SetupError;
        public string NatError => linkerTunDeviceAdapter.NatError;

        public Action OnSetupBefore { get; set; } = () => { };
        public Action OnSetupAfter { get; set; } = () => { };
        public Action OnSetupSuccess { get; set; } = () => { };
        public Action OnShutdownBefore { get; set; } = () => { };
        public Action OnShutdownAfter { get; set; } = () => { };
        public Action OnShutdownSuccess { get; set; } = () => { };

        public TuntapTransfer(LinkerTunDeviceAdapter linkerTunDeviceAdapter)
        {
            this.linkerTunDeviceAdapter = linkerTunDeviceAdapter;
        }

        bool inited = false;
        public void Init(string name, ILinkerTunDeviceCallback linkerTunDeviceCallback)
        {
            if (inited) return;
            inited = true;

            linkerTunDeviceAdapter.Initialize(name, linkerTunDeviceCallback);
            AppDomain.CurrentDomain.ProcessExit += (s, e) => linkerTunDeviceAdapter.Shutdown();
            Console.CancelKeyPress += (s, e) => linkerTunDeviceAdapter.Shutdown();
        }
        public bool Write(ReadOnlyMemory<byte> buffer)
        {
            return linkerTunDeviceAdapter.Write(buffer);
        }

        /// <summary>
        /// 运行网卡
        /// </summary>
        public void Setup(IPAddress ip, byte prefixLength)
        {
            if (operatingManager.StartOperation() == false)
            {
                return;
            }
            TimerHelper.Async(() =>
            {
                OnSetupBefore();
                try
                {
                    if (ip.Equals(IPAddress.Any))
                    {
                        return;
                    }
                    linkerTunDeviceAdapter.Setup(ip, prefixLength, 1400);
                    if (string.IsNullOrWhiteSpace(linkerTunDeviceAdapter.SetupError) == false)
                    {
                        LoggerHelper.Instance.Error(linkerTunDeviceAdapter.SetupError);
                        return;
                    }
                    linkerTunDeviceAdapter.SetNat();
                    if (string.IsNullOrWhiteSpace(linkerTunDeviceAdapter.NatError) == false)
                    {
                        LoggerHelper.Instance.Error(linkerTunDeviceAdapter.NatError);
                    }
                    OnSetupSuccess();
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
                    operatingManager.StopOperation();
                    OnSetupAfter();
                }
            });
        }

        /// <summary>
        /// 停止网卡
        /// </summary>
        public void Shutdown()
        {
            if (operatingManager.StartOperation() == false)
            {
                return;
            }
            try
            {
                OnShutdownBefore();
                linkerTunDeviceAdapter.Shutdown();
                linkerTunDeviceAdapter.RemoveNat();
                linkerTunDeviceAdapter.Clear();
                OnShutdownSuccess();
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
                operatingManager.StopOperation();
                OnShutdownAfter();
            }
        }

        public void AddForward(List<LinkerTunDeviceForwardItem> forward)
        {
            linkerTunDeviceAdapter.AddForward(forward);
        }
        public void RemoveForward(List<LinkerTunDeviceForwardItem> forward)
        {
            linkerTunDeviceAdapter.RemoveForward(forward);
        }

        public void AddRoute(LinkerTunDeviceRouteItem[] ips, IPAddress ip)
        {
            linkerTunDeviceAdapter.AddRoute(ips, ip);
        }
        public void DelRoute(LinkerTunDeviceRouteItem[] ips)
        {
            linkerTunDeviceAdapter.DelRoute(ips);
        }
        public async Task<bool> CheckAvailable()
        {
            return await linkerTunDeviceAdapter.CheckAvailable();
        }
    }
}
