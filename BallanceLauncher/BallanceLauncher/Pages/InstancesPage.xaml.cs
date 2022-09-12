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
using System.Collections.ObjectModel;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml.Media.Animation;
using BallanceLauncher.Model;
using BallanceLauncher.Utils;
using System.Threading.Tasks;
//using BallanceLauncher.Pages.InstanceSubpages;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace BallanceLauncher.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class InstancesPage : Page
    {
        private const int OVERVIEW_PG = -1;
        private const int DOWNLOAD_PG = int.MaxValue;

        private static readonly SlideNavigationTransitionInfo s_slideFromLeft = new() { Effect = SlideNavigationTransitionEffect.FromLeft };
        private static readonly SlideNavigationTransitionInfo s_slideFromRight = new() { Effect = SlideNavigationTransitionEffect.FromRight };

        private int _currentPage;

        public InstancesPage()
        {
            this.InitializeComponent();
            //this.NavigationCacheMode = Microsoft.UI.Xaml.Navigation.NavigationCacheMode.Disabled;
        }

        public void NavigateTo(BallanceInstance instance) => NavigateTo(instance.Name);
        public void NavigateTo(string name)
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                UpdateInstances();
                var target = App.Instances.FirstOrDefault(i => i.Name == name);
                int page = App.Instances.IndexOf(target);
                ContentFrame.Navigate(typeof(InstanceConfigPage), (target, this), s_slideFromRight);
                NavView.SelectedItem = NavView.MenuItems[page + 2];
                _currentPage = page;
            });
        }

        public void NavigateToOverview() =>
            DispatcherQueue.TryEnqueue(() =>
            {
                UpdateInstances();
                ContentFrame.Navigate(typeof(InstanceOverviewPage), this, s_slideFromLeft);
                NavView.SelectedItem = NavView.MenuItems[1];
                _currentPage = OVERVIEW_PG;
            });

        public void NavigateToLast() =>
            DispatcherQueue.TryEnqueue(() =>
            {
                UpdateInstances();
                int last = App.Instances.Count - 1;
                ContentFrame.Navigate(typeof(InstanceConfigPage), (App.Instances[last], this), s_slideFromRight);
                NavView.SelectedItem = NavView.MenuItems[last + 2];
                _currentPage = last;
            });

        public Task<BallanceInstance> AddBallanceAsync(string instancePath, string instanceName) =>
            Task.Run(() =>
            {
                string path = instancePath;
                string name = instanceName == "" ? "Ballance" : instanceName;
                if (!BallanceInstance.EnsureBallancePath(path))
                {
                    DispatcherQueue.TryEnqueue(async () =>
                        await DialogHelper.ShowErrorMessageAsync(XamlRoot, path == null ? "一定要填路径啊喂！" : "Ballance 路径有问题呀？").ConfigureAwait(false));
                    return null;
                }
                foreach (var instance in App.Instances)
                {
                    if (instance.Path == path)
                    {
                        DispatcherQueue.TryEnqueue(async () =>
                            await DialogHelper.ShowErrorMessageAsync(XamlRoot, "这份 Ballance 已经添加过了哈").ConfigureAwait(false));
                        return null;
                    }
                }
                bool hasSameName;
                int nameIndex = -1;
                string testName;
                do
                {
                    hasSameName = false;
                    nameIndex++;
                    testName = name + (nameIndex == 0 ? "" : "~" + nameIndex.ToString());
                    foreach (var instance in App.Instances)
                        if (instance.Name == testName)
                            hasSameName = true;
                }
                while (hasSameName);

                var newInstance = new BallanceInstance(testName, path);
                DispatcherQueue.TryEnqueue(() => App.Instances.Add(newInstance));
                NavigateToLast();

                return newInstance;
            });

        private void NavigationView_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateInstances();
            ContentFrame.Navigate(typeof(InstanceOverviewPage), this);
            NavView.SelectedItem = NavView.MenuItems[0];
            _currentPage = OVERVIEW_PG;
        }

        private async void NavigationView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            if (args.InvokedItemContainer == null) return;
            string tag = args.InvokedItemContainer.Tag.ToString();
            switch (tag)
            {
                case "overview":
                    if (_currentPage == OVERVIEW_PG) break;
                    ContentFrame.Navigate(typeof(InstanceOverviewPage), this, s_slideFromLeft);
                    _currentPage = OVERVIEW_PG; break;
                case "install":
                    if (_currentPage == DOWNLOAD_PG) break;
                    ContentFrame.Navigate(typeof(InstanceInstallPage), this, s_slideFromRight);
                    _currentPage = DOWNLOAD_PG; break;
                case "add":
                    await ShowAddPageAsync().ConfigureAwait(false);
                    break;
                case "find":
                    FindHelper.IsOpen = true;
                    break;
                default:
                    int targetPage = int.Parse(tag);
                    if (targetPage == _currentPage) break;
                    ContentFrame.Navigate(typeof(InstanceConfigPage), (App.Instances[targetPage], this),
                        targetPage < _currentPage ? s_slideFromLeft : s_slideFromRight);
                    _currentPage = targetPage; break;
            }
        }

        private void UpdateInstances()
        {
            NavView.MenuItems.Clear();

            NavView.MenuItems.Add(new NavigationViewItem
            {
                Content = "全部实例",
                Tag = "overview"
            });
            NavView.MenuItems.Add(new NavigationViewItemSeparator());

            int index = 0;
            foreach (var instance in App.Instances)
            {
                var item = new NavigationViewItem
                {
                    Content = instance.Name,
                    Tag = index.ToString()
                };
                NavView.MenuItems.Add(item);
                index++;
            }
        }

        private async Task ShowAddPageAsync()
        {
            var page = new InstanceAddPage();
            var result = await DialogHelper.ShowDialogAsync(XamlRoot, "添加本地 Ballance 实例", page,
                primary: "好啦", close: "算了", defaultButton: ContentDialogButton.Primary).ConfigureAwait(false);

            if (result == ContentDialogResult.Primary)
            {
                await AddBallanceAsync(page.InstancePath, page.InstanceName);
            }
        }

        private void FindHelper_ActionButtonClick(TeachingTip sender, object args)
        {
            sender.IsOpen = false;
        }
    }
}
