using Newtonsoft.Json;
using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace BallanceLauncher.Utils
{
    class ProcessHelper
    {
        public static Process RunProcess(string exePath, string baseDir = null, string args = null, bool showindow = false)
        {
            baseDir ??= App.BaseDir;
            args ??= "";

            Process process = new()
            {
                StartInfo = new ProcessStartInfo()
                {
                    RedirectStandardOutput = true,
                    WorkingDirectory = baseDir,
                    CreateNoWindow = !showindow,
                    WindowStyle = showindow ? ProcessWindowStyle.Normal : ProcessWindowStyle.Hidden,
                    FileName = exePath,
                    Arguments = args,
                }
            };

            process.Start();

            return process;
        }

        public static Process RunProcessCmd(string exePath, string baseDir = null, string args = null)
        {
            baseDir ??= App.BaseDir;
            args ??= "";

            Process process = new()
            {
                StartInfo = new ProcessStartInfo()
                {
                    RedirectStandardOutput = true,
                    WorkingDirectory = baseDir,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    FileName = "cmd.exe",
                    Arguments = string.Format("/C \"{0}: & cd \"{1}\" & \"{2}\" {3}\"", baseDir[0], baseDir, exePath, args),
                }
            };

            process.Start();

            return process;
        }

        public static bool HasFormerProcess()
        {
            Process process = GetRunningInstance();
            if (process != null)
            {
                var handle = process.MainWindowHandle;
                var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(handle);
                var appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowId);
                appWindow.Show(activateWindow: true);
                return true;
            }
            return false;
        }

        public static bool HasFormerProcessMutex()
        {
            // avoid 're-open' with Mutex
            _ = new System.Threading.Mutex(true, "__Mutex__", out bool exist);
            return exist;
        }

        private static Process GetRunningInstance()
        {
            Process currentProcess = Process.GetCurrentProcess();

            Process[] processcollection = Process.GetProcessesByName(currentProcess.ProcessName.Replace(".vshost", ""));
            foreach (Process process in processcollection)
            {

                if (process.Id != currentProcess.Id)
                {
                    //string _name = Assembly.GetExecutingAssembly().FullName.Split(',')[0];
                    //string _name_former = process.MainModule.ModuleName;
                    //if (_name.Replace("/", "\\") == _name_former)
                    //{
                    //    return process;
                    //}
                    return process;
                }
            }
            return null;
        }

    }
}
