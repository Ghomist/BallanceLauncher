using BallanceLauncher.Utils;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.ApplicationSettings;
using Windows.UI.Core.Preview;
using Windows.UI.WindowManagement;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace BallanceLauncher
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        private readonly object _lock = new();
        private string _currentPageTag;
        private int _downloadTaskCounter = 0;

        public MainWindow()
        {
            //Type type = MethodBase.GetCurrentMethod().DeclaringType;
            //string _namespace = type.Namespace;
            //Assembly _assembly = Assembly.GetExecutingAssembly();
            //string resourceName = _namespace + ".rcs.background.png";
            //Stream stream = _assembly.GetManifestResourceStream(resourceName);
            //Image myImage = new Image();

            this.InitializeComponent();

            Title = "Ballance Launcher X";
            ExtendsContentIntoTitleBar = true;
            if (!App.Config.ShowSystemTitleBar)
            {
                SetTitleBar(AppTitleBar);
            }
            else
            {
                AppTitleBar.Visibility = Visibility.Collapsed;
                AppTitleBarDefinition.Height = GridLength.Auto;
            }
        }

        public void NavigateTo(string pageName)
        {
            switch (pageName)
            {
                case "Home":
                    ContentFrame.Navigate(typeof(Pages.HomePage), this); NavView.SelectedItem = NavView.MenuItems[0]; break;
                case "Instances":
                    ContentFrame.Navigate(typeof(Pages.InstancesPage)); NavView.SelectedItem = NavView.MenuItems[1]; break;
                default:
                    return;
            }

            _currentPageTag = pageName;
        }

        public void SetAcrylicOpacity(double opacity)
        {
            BackgroundAcrylic.Opacity = opacity;
        }

        public void SetBackground(int index)
        {
            if (index < 1 || index > 5) return;
            BackgroundImage.UriSource = new Uri("ms-appx:///rcs/background-" + index + ".jpg");
            //BackgroundImage = new ImageBrush
            //{
            //    ImageSource = new BitmapImage(new Uri("rcs/background-" + index + ".jpg"))
            //};
        }

        public async Task TellDownloadBeginAsync()
        {
            //Interlocked.Increment(ref _downloadTaskCounter);
            await Task.Run(async () =>
            {
                lock (_lock)
                {
                    _downloadTaskCounter++;
                }
                await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
                    () => Downloading.Visibility = Visibility.Visible);
            });
        }

        public async Task TellDownloadFinishAsync()
        {
            //Interlocked.Decrement(ref _downloadTaskCounter);
            await Task.Run(async () =>
            {
                lock (_lock)
                {
                    _downloadTaskCounter--;
                }
                if (_downloadTaskCounter == 0)
                    await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
                        () => Downloading.Visibility = Visibility.Collapsed);
            });
        }

        private void navView_Loaded(object sender, RoutedEventArgs e)
        {
            ContentFrame.Navigate(typeof(Pages.HomePage), this);
            NavView.SelectedItem = HomeNavItem;
            _currentPageTag = "Home";
        }

        private void navView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            string tag = args.InvokedItemContainer.Tag.ToString();

            if (tag == _currentPageTag) return;

            switch (tag)
            {
                case "Test":
                    ContentFrame.Navigate(typeof(Pages.TestPage)); break;
                case "Home":
                    ContentFrame.Navigate(typeof(Pages.HomePage), this); break;
                case "Instances":
                    ContentFrame.Navigate(typeof(Pages.InstancesPage)); break;
                case "Download":
                    return;
                case "DownloadMaps":
                    ContentFrame.Navigate(typeof(Pages.MapDownloadPage)); break;
                case "About":
                    ContentFrame.Navigate(typeof(Pages.AboutPage)); break;
                case "设置":
                    ContentFrame.Navigate(typeof(Pages.SettingsPage)); break;
                default:
                    ContentFrame.Navigate(typeof(Pages.TestPage)); break;
            }

            _currentPageTag = tag;

            //navView.AlwaysShowHeader = tag != "Home";
            //navView.Header = args.InvokedItemContainer.Content;
        }

        private void Window_Closed(object sender, WindowEventArgs args)
        {
            App.SaveConfigAsync().GetAwaiter().GetResult();
        }

    }
}
