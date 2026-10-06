using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;

namespace SafeEject
{
    internal static class PowerShellBridge
    {
        internal sealed class Result
        {
            public bool Success;
            public string Message;
        }

        public static Result TryEject(DeviceInfo device)
        {
            try
            {
                var script = ReadEmbeddedScript();
                var temp = Path.Combine(Path.GetTempPath(), "SafeEject-" + Guid.NewGuid().ToString("N") + ".ps1");
                File.WriteAllText(temp, script, new UTF8Encoding(false));

                var escapedTemp = temp.Replace("\"", "\\\"");
                var escapedDeviceId = (device.DeviceId ?? "").Replace("\"", "\\\"");
                var args = "-NoProfile -NonInteractive -ExecutionPolicy Bypass -File \"" +
                           escapedTemp + "\" -DeviceId \"" + escapedDeviceId + "\"";

                var psi = new ProcessStartInfo
                {
                    FileName = Environment.ExpandEnvironmentVariables("%SystemRoot%\\System32\\WindowsPowerShell\\v1.0\\powershell.exe"),
                    Arguments = args,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                using (var p = Process.Start(psi))
                {
                    var output = p.StandardOutput.ReadToEnd();
                    var error = p.StandardError.ReadToEnd();

                    if (!p.WaitForExit(15000))
                    {
                        try { p.Kill(); } catch { }
                        try { File.Delete(temp); } catch { }
                        return new Result { Success = false, Message = "PowerShell 弹出请求超时。" };
                    }

                    try { File.Delete(temp); } catch { }

                    if (p.ExitCode == 0)
                        return new Result
                        {
                            Success = true,
                            Message = string.IsNullOrWhiteSpace(output) ? "设备已安全移除。" : output.Trim()
                        };

                    return new Result
                    {
                        Success = false,
                        Message = string.IsNullOrWhiteSpace(error) ? output.Trim() : error.Trim()
                    };
                }
            }
            catch (Exception ex)
            {
                return new Result { Success = false, Message = ex.Message };
            }
        }

        private static string ReadEmbeddedScript()
        {
            using (var s = Assembly.GetExecutingAssembly().GetManifestResourceStream("SafeEject.SafeEject.ps1"))
            {
                if (s == null)
                    throw new InvalidOperationException("找不到内置 SafeEject.ps1。");

                using (var r = new StreamReader(s))
                    return r.ReadToEnd();
            }
        }
    }
}
