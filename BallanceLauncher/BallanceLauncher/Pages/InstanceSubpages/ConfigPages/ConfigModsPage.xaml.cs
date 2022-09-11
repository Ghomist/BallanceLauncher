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
using BallanceLauncher.Utils;
using System.Threading.Tasks;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace BallanceLauncher.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ConfigModsPage : Page
    {
        private BallanceInstance _instance;
        private readonly List<BallanceMod> _selectedItems;
        private List<BallanceMod> _mods;

        public ConfigModsPage()
        {
            _selectedItems = new List<BallanceMod>();
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            _instance = e.Parameter as BallanceInstance;
            FreshModList();

            base.OnNavigatedTo(e);
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (!IsEnabled) return;
            var checkBox = sender as CheckBox;
            if (checkBox.Parent == null) return; // check box's parent
            var listItem = (checkBox.Parent as Grid).Parent as ListViewItem;
            ModEnableChecked(listItem.Name, true);
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!IsEnabled) return;
            var listItem = ((sender as CheckBox).Parent as Grid).Parent as ListViewItem;
            ModEnableChecked(listItem.Name, false);
        }

        private void ModEnableChecked(string modName, bool enable)
        {
            _mods.FirstOrDefault(mod => mod.DisplayName == modName).SetEnable(enable);
        }

        private void Fresh_Click(object sender, RoutedEventArgs e)
        {
            FreshModList();
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            // TODO
        }

        private async void Delete_Click(object sender, RoutedEventArgs e)
        {
            var result = await DialogHelper.ShowConfirmAsync(XamlRoot, "确认一下", "它们马上就要永远消失了哦 T^T\n（ 无法还原！）", close: true);
            if (result != ContentDialogResult.Primary) return;

            foreach (var mod in _selectedItems)
                mod.Delete();

            _selectedItems.Clear();
            FreshModList();
        }

        private async void FreshModList()
        {
            LoadingTip.Visibility = Visibility.Visible;
            ModList.Visibility = Visibility.Collapsed;
            //Commands.Visibility = Visibility.Collapsed;

            _mods = await _instance.GetModsAsync();
            ModList.ItemsSource = _mods;

            LoadingTip.Visibility = Visibility.Collapsed;
            ModList.Visibility = Visibility.Visible;
            Commands.Visibility = Visibility.Visible;
        }

        private async void More_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedItems.Count != 1) return;
            var mod = _selectedItems[0];
            var page = new ModDetailsPage(mod) { MaxHeight = 350 };
            await DialogHelper.ShowDialogAsync(XamlRoot, title: "Mod 详情", content: page, close: "好的");
        }

        private void ModList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (BallanceMod mod in e.AddedItems)
                if (!_selectedItems.Contains(mod)) _selectedItems.Add(mod);
            foreach (BallanceMod mod in e.RemovedItems)
                if (_selectedItems.Contains(mod))
                    _selectedItems.Remove(_selectedItems.FirstOrDefault((o) => o.Hash == mod.Hash));
            More.IsEnabled = _selectedItems.Count == 1;
            Delete.IsEnabled = _selectedItems.Count > 0;
        }
    }
}
