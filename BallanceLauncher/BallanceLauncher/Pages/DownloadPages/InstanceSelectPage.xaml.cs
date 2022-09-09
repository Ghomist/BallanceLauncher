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
using BallanceLauncher.Model;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace BallanceLauncher.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class InstanceSelectPage : Page
    {
        private readonly List<BallanceInstance> _instances;
        public List<BallanceInstance> SelectedItems { get; private set; }

        public InstanceSelectPage(List<BallanceInstance> instances)
        {
            SelectedItems = new();

            this.InitializeComponent();

            _instances = instances;
            List.ItemsSource = instances;
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            var name = ((sender as CheckBox).Parent as Grid).Tag.ToString();
            var instance = _instances.FirstOrDefault(i => i.Name == name);
            SelectedItems.Add(instance);
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            var name = ((sender as CheckBox).Parent as Grid).Tag.ToString();
            var instance = SelectedItems.FirstOrDefault(i => i.Name == name);
            SelectedItems.Remove(instance);
        }
    }
}
