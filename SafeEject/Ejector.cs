using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;

namespace SafeEject
{
    internal static class Ejector
    {
        private const int IOCTL_STORAGE_EJECT_MEDIA = 0x2D4808;

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern IntPtr CreateFile(string name, uint access, uint share, IntPtr security, uint creation, uint flags, IntPtr template);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool DeviceIoControl(IntPtr handle, uint code, IntPtr inBuf, uint inLen, IntPtr outBuf, uint outLen, out uint returned, IntPtr overlapped);

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

            // First try WMI Plug-and-Play disable/eject semantics through PowerShell.
            var ps = PowerShellBridge.TryEject(device);
            if (ps.Success)
            {
                message = ps.Message;
                return true;
            }

            // Fallback: send eject to each mounted volume.
            foreach (var raw in (device.Letters ?? "").Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries))
            {
                var drive = raw.Trim();
                if (drive.Length < 2) continue;
                var handle = CreateFile(@"\." + drive.TrimEnd('\') + "\", 0x80000000 | 0x40000000, 3, IntPtr.Zero, 3, 0, IntPtr.Zero);
                if (handle == new IntPtr(-1)) continue;
                try
                {
                    uint returned;
                    if (DeviceIoControl(handle, IOCTL_STORAGE_EJECT_MEDIA, IntPtr.Zero, 0, IntPtr.Zero, 0, out returned, IntPtr.Zero))
                    {
                        message = "设备已发送安全弹出请求。";
                        return true;
                    }
                }
                finally { CloseHandle(handle); }
            }

            message = ps.Message.Length > 0 ? ps.Message : "Windows 拒绝弹出。可能仍有程序持有设备句柄，请关闭相关程序后重试。";
            return false;
        }
    }
}
