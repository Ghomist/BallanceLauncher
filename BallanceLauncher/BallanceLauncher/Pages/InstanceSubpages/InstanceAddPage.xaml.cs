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
    public sealed partial class InstanceAddPage : Page
    {
        public string InstanceName { set; get; }
        public string InstancePath { set; get; }

        public InstanceAddPage()
        {
            this.InitializeComponent();
        }

        private async void Browser_Click(object sender, RoutedEventArgs e)
        {
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
        }

        private void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            InstanceName = NameText.Text;
            InstancePath = SelectedFolder.Text;
        }
    }
}
