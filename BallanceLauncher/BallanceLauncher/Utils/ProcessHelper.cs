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
        public static Process RunProcess(string exePath, string baseDir = null, string arg = null, bool showindow = false)
        {
            baseDir ??= System.AppDomain.CurrentDomain.BaseDirectory;
            arg ??= "";

            Process process = new()
            {
                StartInfo = new ProcessStartInfo()
                {
                    RedirectStandardOutput = true,
                    WorkingDirectory = baseDir,
                    CreateNoWindow = !showindow,
                    WindowStyle = showindow ? ProcessWindowStyle.Normal : ProcessWindowStyle.Hidden,
                    FileName = exePath,
                    Arguments = arg,
                }
            };

            //startInfo.FileName = "cmd.exe";
            //var disk = baseDir.Split(':')[0];
            //startInfo.Arguments = string.Format("/C {0}: & cd {1} & {2}", disk, baseDir, exePath);

            process.Start();

            return process;
        }

        public static bool HasFormerProcess()
        {
            Process process = RunningInstance();
            if (process != null)
            {
                //var handle = process.MainWindowHandle;
                //var title = process.MainWindowTitle;

                //const int NOSIZE = 1;
                //const int NOMOVE = 2;
                //SetWindowPos(handle, -2, 0, 0, 0, 0, NOSIZE | NOMOVE);

                //ShowWindow(process.MainWindowHandle, WS_SHOWNORMAL);
                //SetForegroundWindow(process.MainWindowHandle);
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

        [DllImport("user32.dll", EntryPoint = "SetWindowPos")]
        public static extern IntPtr SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int x, int Y, int cx, int cy, int wFlags);

        [DllImport("User32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int cmdShow);

        [DllImport("User32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        //private const int WS_SHOWNORMAL = 1;

        private static Process RunningInstance()
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
