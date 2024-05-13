﻿using MemoryPack;
using System.Net;
using System.Net.NetworkInformation;
using System.Text.Json.Serialization;

namespace cmonitor.plugins.tuntap.vea
{
    public interface ITuntapVea
    {
        public bool Running { get; }

        public Task<bool> Run(int proxyPort);
        public Task<bool> SetIp(IPAddress ip);
        public void Kill();

        public void AddRoute(TuntapVeaLanIPAddress[] ips, IPAddress ip);
        public void DelRoute(TuntapVeaLanIPAddress[] ips);
    }

    [MemoryPackable]
    public sealed partial class TuntapVeaLanIPAddress
    {
        /// <summary>
        /// ip，存小端
        /// </summary>
        public uint IPAddress { get; set; }
        public byte MaskLength { get; set; }
        public uint MaskValue { get; set; }
        public uint NetWork { get; set; }
        public uint Broadcast { get; set; }

    }

    [MemoryPackable]
    public sealed partial class TuntapVeaLanIPAddressList
    {
        public string MachineName { get; set; }
        public List<TuntapVeaLanIPAddress> IPS { get; set; }

    }

    public enum TuntapStatus : byte
    {
        Normal = 0,
        Starting = 1,
        Running = 2
    }

    [MemoryPackable]
    public sealed partial class TuntapInfo
    {
        public string MachineName { get; set; }

        public TuntapStatus Status { get; set; }

        [MemoryPackAllowSerialize]
        public IPAddress IP { get; set; }

        [MemoryPackAllowSerialize]
        public IPAddress[] LanIPs { get; set; } = Array.Empty<IPAddress>();

    }

    [MemoryPackable]
    public sealed partial class TuntapOnlineInfo
    {
        public string[] MachineNames { get; set; }
        public byte[] Status { get; set; }

        [JsonIgnore]
        [MemoryPackInclude]
        public List<Task<IPHostEntry>> HostTasks { get; set; }
        [JsonIgnore]
        [MemoryPackInclude]
        public List<Task<PingReply>> PingTasks { get; set; }
    }
}

