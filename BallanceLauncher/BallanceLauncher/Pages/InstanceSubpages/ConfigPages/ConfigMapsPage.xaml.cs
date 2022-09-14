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
using System.Threading.Tasks;
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

        private async void Add_Click(object sender, RoutedEventArgs e)
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker()
            {
                //picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
                SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Desktop
            };
            picker.FileTypeFilter.Add(".nmo");
            picker.FileTypeFilter.Add(".cmo");
            WinRT.Interop.InitializeWithWindow.Initialize(picker, App.MainWindow.Hwnd);

            var files = await picker.PickMultipleFilesAsync();
            if (files != null && files.Count != 0)
            {
                int existCount = 0;
                var dlg = DialogHelper.ShowProcessingDialog(XamlRoot, "添加地图");
                await Task.Run(() =>
                {
                    foreach (var f in files)
                    {
                        if (File.Exists(_instance.MapDir + f.Name))
                        {
                            existCount++;
                            continue;
                        }
                        File.Copy(f.Path, _instance.MapDir + f.Name, false);
                    }
                });
                DialogHelper.FinishProcessingDialog(dlg, existCount == 0 ? "搞定！" : $"完成！有{existCount}个地图已经存在了");

                FreshMapList();
            }
        }

        private void Browser_Click(object sender, RoutedEventArgs e)
        {
            ProcessHelper.RunProcess("explorer.exe", args: _instance.MapDir);
        }
    }
}
