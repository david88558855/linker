﻿using linker.libs;
using linker.libs.extends;
using linker.plugins.signin.messenger;

namespace linker.plugins.signIn.args
{
    public interface ISignInArgs
    {
        public bool Invoke(Dictionary<string, string> args);
        public bool Verify(SignInfo signInfo, SignCacheInfo cache, out string msg);
    }


    /// <summary>
    /// 给登录加一个唯一ID的参数
    /// </summary>
    public sealed class SignInArgsMachineKey : ISignInArgs
    {
        public bool Invoke(Dictionary<string, string> args)
        {
            string machineKey = GetMachineKey();
            if (LoggerHelper.Instance.LoggerLevel <= LoggerTypes.DEBUG)
                LoggerHelper.Instance.Debug($"machine key :{machineKey}");
            if (string.IsNullOrWhiteSpace(machineKey))
            {
                return false;
            }

            args.TryAdd("machineKey", machineKey.Md5());
            return true;
        }

        /// <summary>
        /// 验证参数
        /// </summary>
        /// <param name="signInfo">新登录参数</param>
        /// <param name="cache">之前的登录信息</param>
        /// <returns></returns>
        public bool Verify(SignInfo signInfo, SignCacheInfo cache, out string msg)
        {
            msg = string.Empty;
            //放宽条件，只有已经登录时不能再次登录
            if (cache.Connected)
            {
                signInfo.Args.TryGetValue("machineKey", out string keyNew);
                cache.Args.TryGetValue("machineKey", out string keyOld);

                //之前的登录有唯一编号的，则验证，唯一编号不一样，不允许登录
                if (string.IsNullOrWhiteSpace(keyOld) == false && keyNew != keyOld)
                {
                    msg = "your machine key is already online";
                    return false;
                }
            }
            return true;
        }

        private string GetMachineKey()
        {
            return OperatingSystem.IsWindows() ? GetMachineKeyWindows() : OperatingSystem.IsLinux() ? GetMachineKeyLinux() : GetMachineKeyOSX();
        }

        private string GetMachineKeyWindows()
        {
            string cpu = CommandHelper.Execute("wmic", "csproduct get UUID", [], out string error).TrimNewLineAndWhiteSapce().Split(Environment.NewLine)[1];
            string username = CommandHelper.Execute("whoami", string.Empty, [], out error).TrimNewLineAndWhiteSapce().Trim();
            return $"{cpu}-{username}-{System.Runtime.InteropServices.RuntimeInformation.OSDescription}";
        }
        private string GetMachineKeyLinux()
        {
            string cpu = CommandHelper.Linux(string.Empty, ["cat /sys/class/dmi/id/product_uuid"]).TrimNewLineAndWhiteSapce();
            if (string.IsNullOrWhiteSpace(cpu) || cpu.Contains("No such file or directory"))
            {
                LoggerHelper.Instance.Error(cpu);
                return string.Empty;
            }
            string username = CommandHelper.Linux(string.Empty, ["whoami"]).TrimNewLineAndWhiteSapce();
            return $"{cpu}-{username}-{System.Runtime.InteropServices.RuntimeInformation.OSDescription}";
        }
        private string GetMachineKeyOSX()
        {
            string cpu = CommandHelper.Osx(string.Empty, ["system_profiler SPHardwareDataType | grep \"Hardware UUID\""]).TrimNewLineAndWhiteSapce();
            string username = CommandHelper.Osx(string.Empty, ["whoami"]).TrimNewLineAndWhiteSapce();
            return $"{cpu.Trim()}-{username}-{System.Runtime.InteropServices.RuntimeInformation.OSDescription}";

        }
    }
}