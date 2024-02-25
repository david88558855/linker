﻿using ManagedNativeWifi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cmonitor.client.reports.wlan
{
    public class WlanWindows : IWlan
    {
        public List<string> WlanEnums()
        {
            return NativeWifi.EnumerateAvailableNetworks().Where(c => string.IsNullOrWhiteSpace(c.ProfileName) == false).Select(c => c.ProfileName).ToList();
        }

        public async Task<bool> WlanConnect(string name)
        {
            var wifi = NativeWifi.EnumerateAvailableNetworks().FirstOrDefault(c => c.ProfileName == name);
            if (wifi == null)
            {
                return false;
            }
            return await NativeWifi.ConnectNetworkAsync(wifi.Interface.Id, wifi.ProfileName, wifi.BssType, TimeSpan.FromSeconds(5));
        }
    }
}
