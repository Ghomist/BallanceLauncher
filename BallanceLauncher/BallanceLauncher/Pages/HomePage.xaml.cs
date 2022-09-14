using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using BallanceLauncher.Model;
using Windows.UI;
using BallanceLauncher.Utils;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace BallanceLauncher.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class HomePage : Page
    {
        public HomePage()
        {
            this.InitializeComponent();
            string defaultInstanceName = ConfigHelper.DefaultInstance;
            var instance = App.Instances.FirstOrDefault(i => i.Name == defaultInstanceName);
            InstanceName.Text = instance != null ? instance.Name : "";
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            UpdateInstanceList();
            base.OnNavigatedTo(e);
        }

        private async void StartButton_Click(SplitButton sender, SplitButtonClickEventArgs e)
        {
            var instance = App.Instances.FirstOrDefault(i => i.Name == InstanceName.Text);
            if (instance != null)
            {
                await App.RunBallanceAsync(instance);
            }
        }

        private void OnItemClick(object sender, RoutedEventArgs e)
        {
            var text = (sender as MenuFlyoutItem).Text;
            InstanceName.Text = text;
            ConfigHelper.DefaultInstance = text;
        }

        private void UpdateInstanceList()
        {
            InstanceList.Items.Clear();

            foreach (var i in App.Instances)
            {
                var item = new MenuFlyoutItem { Text = i.Name };
                item.Click += OnItemClick;
                InstanceList.Items.Add(item);
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            App.MainWindow.NavigateTo("Instances");
        }

        private void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            //ProcessHelper.RunProcess()
        }
    }
}
