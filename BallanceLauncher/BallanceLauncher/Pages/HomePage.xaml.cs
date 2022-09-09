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
        private MainWindow _mainWindow;

        public HomePage()
        {
            this.InitializeComponent();
            InstanceName.Text = App.Instances[0].Name;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            _mainWindow = e.Parameter as MainWindow;
            UpdateInstanceList();
            base.OnNavigatedTo(e);
        }

        private async void StartButton_Click(SplitButton sender, SplitButtonClickEventArgs e)
        {
            var instance = App.Instances.FirstOrDefault(i => i.Name == InstanceName.Text);
            await App.RunBallanceAsync(instance);
        }

        private void OnItemClick(object sender, RoutedEventArgs e)
        {
            InstanceName.Text = (sender as MenuFlyoutItem).Text;
        }

        private void UpdateInstanceList()
        {
            InstanceList.Items.Clear();

            //var item = new MenuFlyoutItem();
            //item.Text = "Ballance";
            //item.Click += OnItemClick;
            //InstanceList.Items.Add(item);
            //InstanceList.Items.Add(new MenuFlyoutSeparator());

            foreach (var i in App.Instances)
            {
                var item = new MenuFlyoutItem { Text = i.Name };
                item.Click += OnItemClick;
                InstanceList.Items.Add(item);
            }

            //InstanceName.Text = instanceText;
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            _mainWindow.NavigateTo("Instances");
        }

        private void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            //ProcessHelper.RunProcess()
        }
    }
}
