using BallanceLauncher.Model;
using BallanceLauncher.Utils;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace BallanceLauncher.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class BasicConfigPage : Page
    {
        private BallanceInstance _instance;

        public BasicConfigPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            _instance = e.Parameter as BallanceInstance;
            base.OnNavigatedTo(e);
        }

        private void BrowseDir(object sender, RoutedEventArgs e)
        {
            ProcessHelper.RunProcess("explorer.exe", args: _instance.Path);
        }

        private void BrowseMapDir(object sender, RoutedEventArgs e)
        {
            ProcessHelper.RunProcess("explorer.exe", args: _instance.MapDir);
        }

        private void BrowseModDir(object sender, RoutedEventArgs e)
        {
            ProcessHelper.RunProcess("explorer.exe", args: _instance.ModDir);
        }

        private void NameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
