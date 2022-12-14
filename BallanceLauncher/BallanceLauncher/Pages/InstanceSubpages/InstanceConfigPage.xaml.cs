using BallanceLauncher.Model;
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
    public sealed partial class InstanceConfigPage : Page
    {
        private BallanceInstance _instance;
        private string _currentTag;
        private InstancesPage _parentPage;

        public InstanceConfigPage()
        {
            InitializeComponent();
        }

        public void TryFresh()
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                _currentTag = "Basic";
                SettingHeader.Text = "基本信息";
                SettingContent.Navigate(typeof(BasicConfigPage), (_instance));
            });
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            _currentTag = "Basic";
            (_instance, _parentPage) = (ValueTuple<BallanceInstance, InstancesPage>)e.Parameter;
            SettingHeader.Text = "基本信息";
            SettingContent.Navigate(typeof(BasicConfigPage), _instance);
            //SettingText.Text = (e.Parameter as BallanceInstance).ToJson();
            base.OnNavigatedTo(e);
        }

        private void NavLinksList_ItemClick(object sender, ItemClickEventArgs e)
        {
            var clickedItem = e.ClickedItem as StackPanel;
            var tag = clickedItem.Tag.ToString();
            if (tag == _currentTag) return;
            switch (tag)
            {
                case "Basic":
                    SettingHeader.Text = "基本信息";
                    SettingContent.Navigate(typeof(BasicConfigPage), _instance);
                    break;
                case "Settings":
                    SettingHeader.Text = "游戏设置";
                    SettingContent.Navigate(typeof(GameSettingsPage), _instance);
                    break;
                case "Mods":
                    SettingHeader.Text = "Mods";
                    SettingContent.Navigate(typeof(ConfigModsPage), _instance);
                    break;
                case "Maps":
                    SettingHeader.Text = "自定义地图";
                    SettingContent.Navigate(typeof(ConfigMapsPage), _instance);
                    break;
                case "Records":
                    SettingHeader.Text = "纪录管理";
                    SettingContent.Navigate(typeof(RecordPage), _instance);
                    break;
                case "Other":
                    SettingHeader.Text = "其它设置";
                    SettingContent.Navigate(typeof(OperationPage), (_instance, _parentPage));
                    break;
                default: break;
            }
            _currentTag = tag;
        }
    }
}
