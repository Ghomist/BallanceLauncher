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
        public static MainWindow Window { get; private set; }
        public static IntPtr Hwnd { get; private set; }
        public static Config Config { get; private set; }

        public static string BaseDir => AppDomain.CurrentDomain.BaseDirectory;
        public static StorageFolder LocalFolder => ApplicationData.Current.LocalFolder;
        public static ApplicationDataContainer LocalSettings => ApplicationData.Current.LocalSettings;

        private static App _appInstance;
        private static Process _runningInstance;

        private static readonly string s_configSavePath = "config.json";
        private static readonly string s_instancesSavePath = "instances.json";
        private static readonly string s_exceptionLogPath = "err.log";

        public App()
        {
            // avoid 're-open'
            //if (ProcessHelper.HasFormerProcess()) Environment.Exit(1);

            //AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;


            // extract resources
            //if (!File.Exists(BaseDir + "BallanceModInfoReader.exe"))
            //{
            _ = FileHelper.ExtractResourceAsync("BallanceModInfoReader", "BallanceModInfoReader.exe");
            _ = FileHelper.ExtractResourceAsync("BallanceModInfoReader", "BML.dll");
            //}

            // read configuration
            try
            {
                var jsonText = FileHelper.ReadLocalFileAsync(s_configSavePath).GetAwaiter().GetResult();
                if (jsonText == null || jsonText == "")
                {
                    Config = new();
                }
                else
                {
                    Config = JsonConvert.DeserializeObject<Config>(jsonText);
                }
            }
            catch (Exception) { Config = new(); }

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
            catch (Exception) { Instances = new() { new BallanceInstance("真正的游戏", @"D:\Ballance\") }; }

            // app initialize
            this.InitializeComponent();

            // set some variable
            _appInstance = this;
        }

        public static void RestartWindow()
        {
            if (Window != null)
            {
                (var window, var hwnd) = StartWindow();
                Window.Close();
                Window = window;
                Hwnd = hwnd;
            }
        }

        public static async Task RunBallanceAsync(BallanceInstance instance)
        {
            try
            {
                _ = instance ?? throw new ArgumentNullException(message: "未找到 Ballance 实例", paramName: nameof(instance));
                if (_runningInstance != null && !_runningInstance.HasExited)
                {
                    await DialogHelper.ShowErrorMessageAsync(Window.Content.XamlRoot, "已经有一个 Ballance 在运行啦！").ConfigureAwait(false);
                    return;
                }
                _runningInstance = ProcessHelper.RunProcess(instance.Executable, instance.WorkingPath, showindow: true);
            }
            catch (Exception ex)
            {
                await DialogHelper.ShowErrorMessageAsync(Window.Content.XamlRoot, ex.Message, true);
            }
        }

        public static Task SaveConfigAsync() =>
            Task.Run(async () =>
            {
                await FileHelper.WriteLocalFileAsync(s_configSavePath, JsonConvert.SerializeObject(Config)).ConfigureAwait(false);
                await FileHelper.WriteLocalFileAsync(s_instancesSavePath, JsonConvert.SerializeObject(Instances));
            });

        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args) =>
            (Window, Hwnd) = StartWindow();

        private static (MainWindow, IntPtr) StartWindow()
        {
            var window = new MainWindow();
            //window.Closed += async (sender, e) => { await SaveAll(); };
            window.Activate();

            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
            return (window, hwnd);
        }

    }
}


