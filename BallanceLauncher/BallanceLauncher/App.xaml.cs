﻿using Microsoft.UI.Xaml;
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

        public static string BaseDir { get => AppDomain.CurrentDomain.BaseDirectory; }

        private static App _appInstance;
        private static Process _runningInstance;

        private static readonly string s_configSavePath = BaseDir + "config.json";
        private static readonly string s_instancesSavePath = BaseDir + "instances.json";
        private static readonly string s_exceptionLogPath = BaseDir + "err.log";

        public App()
        {
            // avoid 're-open'
            //if (ProcessHelper.HasFormerProcess()) Environment.Exit(1);

            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

            // extract resources
            if (!File.Exists(BaseDir + "BallanceModInfoReader.exe"))
            {
                _ = FileHelper.ExtractResourceAsync("BallanceModInfoReader", "BallanceModInfoReader.exe");
                _ = FileHelper.ExtractResourceAsync("BallanceModInfoReader", "BML.dll");
            }

            // read configuration
            try
            {
                using var fs = new FileStream(s_configSavePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                var jsonText = new StreamReader(fs, Encoding.UTF8).ReadToEnd();
                Config = JsonConvert.DeserializeObject<Config>(jsonText);
            }
            catch (Exception)
            {
                Config = new Config();
            }

            // read instances
            try
            {
                using var fs = new FileStream(s_instancesSavePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                var jsonText = new StreamReader(fs, Encoding.UTF8).ReadToEnd();
                Instances = JsonConvert.DeserializeObject<ObservableCollection<BallanceInstance>>(jsonText);
                //foreach (var instance in Instances)
                //{
                //    if (!instance.EnsureExist()) Instances.Remove(instance);
                //}
            }
            catch (FileNotFoundException)
            {
                Instances = new ObservableCollection<BallanceInstance>
                {
                    new BallanceInstance("真正的游戏", @"D:\Ballance")
                };
            }
            catch (JsonException)
            {
                Console.WriteLine("Json 加载失败，重新生成实例列表");
                // add default game
                Instances = new ObservableCollection<BallanceInstance>
                {
                    new BallanceInstance("真正的游戏", @"D:\Ballance")
                };
            }

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

        public static void SaveAll()
        {
            // clear all json files
            foreach (var path in new string[] { s_configSavePath, s_instancesSavePath })
            {
                var file = new FileInfo(path);
                if (file.Exists) file.Delete();
            }
            // save configuration
            using (FileStream fs = new(s_configSavePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite))
            {
                using StreamWriter sw = new(fs, Encoding.UTF8);
                sw.WriteLine(JsonConvert.SerializeObject(Config, Formatting.Indented));
            }
            // save instances
            using (FileStream fs = new(s_instancesSavePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite))
            {
                using StreamWriter sw = new(fs, Encoding.UTF8);
                sw.WriteLine(JsonConvert.SerializeObject(Instances, Formatting.Indented));
            }
        }

        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            (Window, Hwnd) = StartWindow();
        }

        private static (MainWindow, IntPtr) StartWindow()
        {
            var window = new MainWindow();
            window.Activate();

            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
            return (window, hwnd);
        }

        private void OnUnhandledException(object sender, System.UnhandledExceptionEventArgs e)
        {
            var log = new FileInfo(s_exceptionLogPath);
            if (log.Exists) log.Delete();

            using var fs = new FileStream(s_exceptionLogPath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite);
            using StreamWriter sw = new(fs, Encoding.UTF8);
            sw.WriteLine(nameof(e.ExceptionObject) + "\n");
            sw.WriteLine(e.ToString());
        }

    }
}


