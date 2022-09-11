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
using System.Text.RegularExpressions;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace BallanceLauncher.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class InstanceInstallPage : Page
    {
        private InstancesPage _instancesPage;

        public InstanceInstallPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            _instancesPage = e.Parameter as InstancesPage;
            base.OnNavigatedTo(e);
        }

        private void Browser_Click(object sender, RoutedEventArgs e)
        {
            var picker = new WpfCore.FolderPicker.FolderBrowserDialog();

            var result = picker.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                string folder = picker.Folder;

                if (folder != null)
                {
                    DispatcherQueue.TryEnqueue(() =>
                    {
                        SelectedFolder.Text = folder;
                        if (NameText.Text == "") NameText.Text = folder.Split('\\')[^1];
                    });
                }
            }

            /* FolderPicker cannot work when running as admin
             * 
            var picker = new Windows.Storage.Pickers.FolderPicker
            {
                //picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
                SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Desktop
            };
            picker.FileTypeFilter.Add("*");
            WinRT.Interop.InitializeWithWindow.Initialize(picker, App.Hwnd);

            var folder = await picker.PickSingleFolderAsync();
            if (folder != null)
            {
                SelectedFolder.Text = folder.Path;
                if (NameText.Text == "") NameText.Text = folder.Name;
            }
            */
        }

        private async void Submit_Click(object sender, RoutedEventArgs e)
        {
            var name = NameText.Text;
            var path = SelectedFolder.Text;

            if (path == "")
            {
                DispatcherQueue.TryEnqueue(async () =>
                    await DialogHelper.ShowErrorMessageAsync(XamlRoot, "一定要填路径啊喂！").ConfigureAwait(false));
                return;
            }

            if (!Regex.IsMatch(path, "[a-zA-Z]:[/\\\\]{1,2}.*"))
            {
                DispatcherQueue.TryEnqueue(async () =>
                    await DialogHelper.ShowErrorMessageAsync(XamlRoot, "路径有问题呀？要使用绝对路径哦").ConfigureAwait(false));
                return;
            }

            await FileHelper.ExtractBallance(path).ConfigureAwait(false);
            var newInstance = await _instancesPage.AddBallanceAsync(path, name).ConfigureAwait(false);
            if (newInstance != null)
                await newInstance.InstallBMLAsync().ConfigureAwait(false);
        }
    }
}
