using Newtonsoft.Json;
using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

namespace BallanceLauncher.Utils
{
    class ProcessHelper
    {
        public static Process RunProcess(string exePath, string baseDir = null, string args = null, bool showindow = false)
        {
            baseDir ??= App.BaseDir;
            args ??= "";

            var process = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    RedirectStandardOutput = true,
                    WorkingDirectory = baseDir,
                    CreateNoWindow = !showindow,
                    WindowStyle = showindow ? ProcessWindowStyle.Normal : ProcessWindowStyle.Hidden,
                    FileName = exePath,
                    Arguments = args,
                },
            };

            process.Start();

            return process;
        }

        public static Process RunProcessCmd(string exePath, string baseDir = null, string args = null)
        {
            args ??= "";
            args = string.Format("/C \"{0}: & cd \"{1}\" & \"{2}\" {3}\"", baseDir[0], baseDir, exePath, args);
            return RunProcess("cmd.exe", baseDir, args);
        }

        public static async Task RunAndWaitAsync(string exePath, string baseDir = null, string args = null,
            bool showindow = false, Action callback = null)
        {
            var process = RunProcess(exePath, baseDir, args, showindow);
            await process.WaitForExitAsync();
            callback?.Invoke();
        }

        public static bool IsAppRunning()
        {
            Process process = GetRunningApp();
            if (process != null)
            {
                var handle = process.MainWindowHandle;
                var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(handle);
                var appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowId);
                appWindow.Show(activateWindow: true);
                appWindow.MoveInZOrderAtTop();
                return true;
            }
            return false;
        }

        public static bool IsMutexAppRunning()
        {
            // avoid 're-open' with Mutex
            _ = new System.Threading.Mutex(true, "__Mutex__", out bool exist);
            return exist;
        }

        private static Process GetRunningApp()
        {
            var current = Process.GetCurrentProcess();
            var processName = current.ProcessName.Replace(".vshost", "");
            var processCollection = Process.GetProcessesByName(processName);
            return processCollection.FirstOrDefault(p => p.Id != current.Id);
        }

    }
}
