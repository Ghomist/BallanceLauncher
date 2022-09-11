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
using Windows.Storage;
using Windows.Storage.Pickers;

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
        }

        private void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            InstanceName = NameText.Text;
            InstancePath = SelectedFolder.Text;
        }
    }
}
