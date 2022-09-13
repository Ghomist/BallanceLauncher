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
    public sealed partial class ConfigMapsPage : Page
    {
        private BallanceInstance _instance;
        private List<BallanceMap> _selectedItems;
        private List<BallanceMap> _maps;

        public ConfigMapsPage()
        {
            this.InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            _selectedItems = new();

            _instance = e.Parameter as BallanceInstance;

            _maps = await _instance.GetMapsAsync();
            MapList.ItemsSource = _maps;
            base.OnNavigatedTo(e);
        }

        private async void Delete_Click(object sender, RoutedEventArgs e)
        {
            var result = await DialogHelper.ShowConfirmAsync(XamlRoot, "确认一下", "它们马上就要永远消失了哦 T^T\n（ 无法还原！）", close: true);
            if (result == ContentDialogResult.Primary)
            {
                foreach (var map in _selectedItems)
                {
                    map.Delete();
                }
                FreshMapList();
            }
        }

        private void ModList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (BallanceMap map in e.AddedItems)
                if (!_selectedItems.Contains(map)) _selectedItems.Add(map);
            foreach (BallanceMap map in e.RemovedItems)
                if (_selectedItems.Contains(map))
                    _selectedItems.Remove(_selectedItems.FirstOrDefault(o => o.Equals(map)));
            Delete.IsEnabled = _selectedItems.Count > 0;
        }

        private async void FreshMapList()
        {
            _maps = await _instance.GetMapsAsync();
            MapList.ItemsSource = _maps;
        }

        private void Fresh_Click(object sender, RoutedEventArgs e)
        {
            FreshMapList();
        }
    }
}
