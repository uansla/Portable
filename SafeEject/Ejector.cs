using System;
using System.Runtime.InteropServices;

namespace SafeEject
{
    internal static class Ejector
    {
        private const int CR_SUCCESS = 0;
        private const int IOCTL_STORAGE_EJECT_MEDIA = 0x2D4808;

        [DllImport("cfgmgr32.dll", CharSet = CharSet.Unicode)]
        private static extern int CM_Locate_DevNodeW(out uint devInst, string deviceId, int flags);

        [DllImport("cfgmgr32.dll", CharSet = CharSet.Unicode)]
        private static extern int CM_Request_Device_EjectW(
            uint devInst,
            out int vetoType,
            [Out] char[] vetoName,
            int vetoNameLength,
            int flags);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern IntPtr CreateFile(
            string name, uint access, uint share, IntPtr security,
            uint creation, uint flags, IntPtr template);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool DeviceIoControl(
            IntPtr handle, uint code, IntPtr inBuf, uint inLen,
            IntPtr outBuf, uint outLen, out uint returned, IntPtr overlapped);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CloseHandle(IntPtr handle);

        public static bool TryEject(DeviceInfo device, out string message)
        {
            message = "";

            if (device == null || string.IsNullOrEmpty(device.DeviceId))
            {
                message = "未找到有效设备。";
                return false;
            }

            uint devInst;
            var locate = CM_Locate_DevNodeW(out devInst, device.DeviceId, 0);
            if (locate == CR_SUCCESS)
            {
                int veto;
                var vetoName = new char[260];
                var eject = CM_Request_Device_EjectW(
                    devInst, out veto, vetoName, vetoName.Length, 0);

                if (eject == CR_SUCCESS)
                {
                    message = "设备已安全弹出，可以拔出。";
                    return true;
                }

                var vetoText = new string(vetoName).TrimEnd('\0');
                if (!string.IsNullOrWhiteSpace(vetoText))
                    message = "Windows 拒绝弹出：" + vetoText;
                else
                    message = "Windows 拒绝弹出，设备可能仍被程序占用。";
            }
            else
            {
                message = "无法定位设备的 PnP 实例。";
            }

            foreach (var raw in (device.Letters ?? "").Split(
                new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries))
            {
                var drive = raw.Trim().TrimEnd('\\');
                if (drive.Length < 2)
                    continue;

                var handle = CreateFile(
                    @"\\." + "\\" + drive,
                    0x80000000 | 0x40000000,
                    3,
                    IntPtr.Zero,
                    3,
                    0,
                    IntPtr.Zero);

                if (handle == new IntPtr(-1))
                    continue;

                try
                {
                    uint returned;
                    if (DeviceIoControl(
                        handle,
                        IOCTL_STORAGE_EJECT_MEDIA,
                        IntPtr.Zero,
                        0,
                        IntPtr.Zero,
                        0,
                        out returned,
                        IntPtr.Zero))
                    {
                        message = "设备已发送安全弹出请求。";
                        return true;
                    }
                }
                finally
                {
                    CloseHandle(handle);
                }
            }

            return false;
        }
    }
}