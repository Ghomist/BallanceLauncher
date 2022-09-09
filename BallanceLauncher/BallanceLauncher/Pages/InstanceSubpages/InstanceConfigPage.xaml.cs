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

        public InstanceConfigPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            _currentTag = "Basic";
            _instance = e.Parameter as BallanceInstance;
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
                //case "BML":
                //    SettingHeader.Text = "BML 设置";
                //    SettingContent.Navigate(typeof(TestPage));
                //    break;
                case "Mods":
                    SettingHeader.Text = "Mods";
                    SettingContent.Navigate(typeof(ConfigModsPage), _instance);
                    break;
                case "Maps":
                    SettingHeader.Text = "自定义地图";
                    SettingContent.Navigate(typeof(ConfigMapsPage), _instance);
                    break;
                case "Other":
                    SettingHeader.Text = "其它设置";
                    SettingContent.Navigate(typeof(TestPage));
                    break;
            }
            _currentTag = tag;
        }
    }
}
