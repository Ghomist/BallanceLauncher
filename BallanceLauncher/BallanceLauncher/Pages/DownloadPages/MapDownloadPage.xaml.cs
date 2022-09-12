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
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Contacts;
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
        private List<BMap> _maps;
        private readonly string _defaultCategory = "全部地图";
        private SortType _sortType = SortType.Category;
        private BMap _selectedMap;

        public MapDownloadPage()
        {
            this.InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            LoadingRing.IsActive = true;
            ForceFresh.IsEnabled = false;

            _maps = await MapDownloader.GetMapsAsync();
            FreshCategoryButtonList();
            ReshowMapList();

            LoadingRing.IsActive = false;
            ForceFresh.IsEnabled = true;
        }

        private void FreshCategoryButtonList()
        {
            var categories = new SortedSet<string>();
            foreach (var map in _maps) categories.Add(map.Category);

            CategoryList.Items.Clear();
            var defaultItem = new MenuFlyoutItem() { Text = _defaultCategory };
            defaultItem.Click += ChangeCategory;
            CategoryList.Items.Add(defaultItem);
            CategoryList.Items.Add(new MenuFlyoutSeparator());
            foreach (var category in categories)
            {
                var item = new MenuFlyoutItem()
                {
                    Text = category,
                };
                item.Click += ChangeCategory;
                CategoryList.Items.Add(item);
            }
            CategoryButton.Content = _defaultCategory;
        }

        private void ReshowMapList()
        {
            IEnumerable<BMap> filter = _maps;

            var currentCategory = CategoryButton.Content.ToString();
            if (currentCategory != _defaultCategory)
                filter = filter.Where(i => i.Category.Equals(currentCategory));

            filter = filter
                .Where(i =>
                   i.Name.Contains(NameFilter.Text, StringComparison.OrdinalIgnoreCase) &&
                   i.Author.Contains(AuthorFilter.Text, StringComparison.OrdinalIgnoreCase))
                .OrderBy(i =>
                    _sortType switch
                    {
                        SortType.Category => i.Category,
                        SortType.Name => i.Name,
                        SortType.Author => i.Author,
                        SortType.Date => i.UploadTime,
                        SortType.Difficulty => i.Difficulty.ToString(),
                        _ => i.Category
                    }
                );

            ContentList.ItemsSource = filter;
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
                    {
                        await DialogHelper.ShowErrorMessageAsync(XamlRoot, "至少要选一个呢！").ConfigureAwait(false);
                    }
                    else
                    {
                        var dlg = DialogHelper.ShowProcessingDialog(XamlRoot, "下载地图");
                        await MapDownloader.DownloadMap(_selectedMap.Url, _selectedMap.Name, page2.SelectedItems);
                        DialogHelper.FinishProcessingDialog(dlg, "下载好啦！");
                    }
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

        private async void ForceFresh_Click(object sender, RoutedEventArgs e)
        {
            LoadingRing.IsActive = true;
            ForceFresh.IsEnabled = false;

            _maps = await MapDownloader.GetMapsAsync(force: true);

            // categories
            FreshCategoryButtonList();

            NameFilter.Text = "";
            AuthorFilter.Text = "";

            ReshowMapList();

            LoadingRing.IsActive = false;
            ForceFresh.IsEnabled = true;
        }

        enum SortType { Category, Name, Author, Date, Difficulty }
    }
}
