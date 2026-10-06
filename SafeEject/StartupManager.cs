using System;
using System.Diagnostics;
using System.IO;

namespace SafeEject
{
    internal static class StartupManager
    {
        private const string TaskName = "SafeEject Portable";

        public static bool IsInstalled()
        {
            var result = Run("/Query /TN \"" + TaskName + "\"");
            return result.ExitCode == 0;
        }

        public static void Install()
        {
            var exe = Process.GetCurrentProcess().MainModule.FileName;
            var taskCommand = "\"" + exe.Replace("\"", "\\\"") + "\"";
            Run("/Create /SC ONLOGON /TN \"" + TaskName + "\" /TR " + taskCommand + " /RL HIGHEST /F");
        }

        public static void Uninstall()
        {
            Run("/Delete /TN \"" + TaskName + "\" /F");
        }

        private static Result Run(string args)
        {
            using (var p = new Process())
            {
                p.StartInfo = new ProcessStartInfo
                {
                    FileName = Path.Combine(Environment.SystemDirectory, "schtasks.exe"),
                    Arguments = args,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };
                p.Start();
                var output = p.StandardOutput.ReadToEnd();
                var error = p.StandardError.ReadToEnd();
                p.WaitForExit(5000);
                return new Result { ExitCode = p.ExitCode, Output = output, Error = error };
            }
        }

        private sealed class Result
        {
            public int ExitCode;
            public string Output;
            public string Error;
        }
    }
}