using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using BallanceLauncher.Model;
using BallanceLauncher.Utils;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using BallanceLauncher.Pages;
using Microsoft.UI;
using Newtonsoft.Json;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Core.Preview;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace BallanceLauncher
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        public static ObservableCollection<BallanceInstance> Instances { get; private set; }

        public static Microsoft.UI.Windowing.AppWindow AppWindow { get; private set; }
        public static MainWindow MainWindow { get; private set; }
        public static Windows.Graphics.SizeInt32 WindowSize
        {
            get => AppWindow.ClientSize;
            set => AppWindow.Resize(value);
        }

        public static string BaseDir => AppDomain.CurrentDomain.BaseDirectory;
        public static string InfoReaderPath => FileHelper.LocalFolder.Path + "\\BallanceModInfoReader.exe";

        private static App _appInstance;
        private static Process _runningInstance;

        private static readonly string s_instancesSavePath = "instances.json";

        public App()
        {
            // avoid 're-open'
            if (ProcessHelper.HasFormerProcess())
            {
                Environment.Exit(1);
            }

            //AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;


            // extract resources
            //if (!File.Exists(BaseDir + "BallanceModInfoReader.exe"))
            //{
            _ = FileHelper.ExtractResourceAsync("BallanceModInfoReader", "BallanceModInfoReader.exe");
            _ = FileHelper.ExtractResourceAsync("BallanceModInfoReader", "BML.dll");
            //}

            // read instances
            try
            {
                var jsonText = FileHelper.ReadLocalFileAsync(s_instancesSavePath).GetAwaiter().GetResult();
                Instances = JsonConvert.DeserializeObject<ObservableCollection<BallanceInstance>>(jsonText);
                for (int i = Instances.Count - 1; i >= 0; --i)
                {
                    if (!Instances[i].Exists)
                    {
                        Instances.RemoveAt(i);
                    }
                }
            }
            catch (Exception) { Instances = new(); }

            // app initialize
            this.InitializeComponent();

            // set some variable
            _appInstance = this;
        }

        public static void RestartWindow()
        {
            if (MainWindow != null)
            {
                (var window, var appWindow) = StartWindow();
                MainWindow.Close();
                MainWindow = window;
                AppWindow = appWindow;
            }
        }

        public static async Task RunBallanceAsync(BallanceInstance instance)
        {
            try
            {
                _ = instance ?? throw new ArgumentNullException(message: "未找到 Ballance 实例", paramName: nameof(instance));
                if (_runningInstance != null && !_runningInstance.HasExited)
                {
                    await DialogHelper.ShowErrorMessageAsync(MainWindow.Content.XamlRoot, "已经有一个 Ballance 在运行啦！").ConfigureAwait(false);
                    return;
                }
                _runningInstance = ProcessHelper.RunProcess(instance.Executable, instance.WorkingDir, showindow: true);
            }
            catch (Exception ex)
            {
                await DialogHelper.ShowErrorMessageAsync(MainWindow.Content.XamlRoot, ex.Message, true);
            }
        }

        public static Task SaveInstancesAsync() =>
            FileHelper.WriteLocalFileAsync(s_instancesSavePath, JsonConvert.SerializeObject(Instances));

        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            (MainWindow, AppWindow) = StartWindow();
            WindowSize = new(ConfigHelper.WindowWidth, ConfigHelper.WindowHeight);
        }

        private static (MainWindow, Microsoft.UI.Windowing.AppWindow) StartWindow()
        {
            var window = new MainWindow();
            window.Activate();
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
            var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hwnd);
            var appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowId);
            return (window, appWindow);
        }

    }
}


