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
using System.Runtime.InteropServices;
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
    public sealed partial class MapDownloadPage : Page
    {
        private readonly string _defaultCategory = "全部地图";
        private BMap _selectedMap;

        public MapDownloadPage()
        {
            this.InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            await MapDownloader.FreshInfoAsync();

            ContentList.ItemsSource = MapDownloader.Maps;
            FreshCategoryButtonList();

            base.OnNavigatedTo(e);
        }

        private void FreshCategoryButtonList()
        {
            CategoryList.Items.Clear();
            var defaultItem = new MenuFlyoutItem() { Text = _defaultCategory };
            defaultItem.Click += ChangeCategory;
            CategoryList.Items.Add(defaultItem);
            CategoryList.Items.Add(new MenuFlyoutSeparator());
            foreach (var category in MapDownloader.MapCollection)
            {
                var item = new MenuFlyoutItem()
                {
                    Text = category.Category,
                };
                item.Click += ChangeCategory;
                CategoryList.Items.Add(item);
            }
            CategoryButton.Content = _defaultCategory;
        }

        private void ReshowMapList()
        {
            var maps = MapDownloader.Maps;
            var currentCategory = CategoryButton.Content.ToString();
            if (currentCategory != _defaultCategory)
                maps = maps.Where(i => i.Category.Equals(currentCategory)).ToList();

            maps = maps.Where(i =>
                   i.Name.Contains(NameFilter.Text, StringComparison.OrdinalIgnoreCase) &&
                   i.Author.Contains(AuthorFilter.Text, StringComparison.OrdinalIgnoreCase)
               ).Select((i, j) => { i.Index = j; return i; }).ToList();

            ContentList.ItemsSource = maps;
        }

        private void ChangeCategory(object sender, RoutedEventArgs args)
        {
            var text = (sender as MenuFlyoutItem).Text;
            CategoryButton.Content = text;

            ReshowMapList();
        }

        private void Filter_TextChanged(object sender, TextChangedEventArgs e)
        {
            ReshowMapList();
        }

        private async void ContentList_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            var page = new DownloadMapDetailsPage(_selectedMap) { MaxWidth = 350, MaxHeight = 300 };
            var result = await DialogHelper.ShowDialogAsync(XamlRoot, "地图详情", page,
                "下载到所有", "下载到游戏…", "好的", ContentDialogButton.Primary);
            if (result == ContentDialogResult.Primary)
            {
                await MapDownloader.DownloadMap(_selectedMap.Url, _selectedMap.Name, App.Instances.ToList()).ConfigureAwait(false);
            }
            else if (result == ContentDialogResult.Secondary)
            {
                var page2 = new InstanceSelectPage(App.Instances.ToList()) { MaxWidth = 350, MaxHeight = 300 };
                var result2 = await DialogHelper.ShowDialogAsync(XamlRoot, $"下载\"{_selectedMap.Name}\"到…", page2,
                    primary: "好啦", close: "算了", defaultButton: ContentDialogButton.Primary);
                if (result2 == ContentDialogResult.Primary)
                {
                    if (page2.SelectedItems.Count == 0)
                        await DialogHelper.ShowErrorMessageAsync(XamlRoot, "至少要选一个呢！");
                    else
                        await MapDownloader.DownloadMap(_selectedMap.Url, _selectedMap.Name, page2.SelectedItems);
                }
            }
        }

        private void ContentList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                _selectedMap = e.AddedItems[0] as BMap;
            }
            catch (Exception)
            {
                _selectedMap = null;
            }
        }
    }
}
