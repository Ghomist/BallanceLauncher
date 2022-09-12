using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using BallanceLauncher.Model;
using BallanceLauncher.Utils;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace BallanceLauncher.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class OperationPage : Page
    {
        private BallanceInstance _instance;
        private InstancesPage _parentPage;

        private bool BMLInstalled { get => _instance.HasBMLInstalled; }
        private bool BMLNotInstalled { get => !_instance.HasBMLInstalled; }

        public OperationPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            (_instance, _parentPage) = (ValueTuple<BallanceInstance, InstancesPage>)e.Parameter;
            base.OnNavigatedTo(e);
        }

        private async void InstallBML_Click(object sender, RoutedEventArgs e)
        {
            var dlg = DialogHelper.ShowProcessingDialog(XamlRoot, "安装 BML");
            await _instance.InstallBMLAsync();
            DialogHelper.FinishProcessingDialog(dlg, "安装完毕！");
            _parentPage.NavigateTo(_instance);
        }

        private async void UninstallBML_Click(object sender, RoutedEventArgs e)
        {
            var result = await DialogHelper.ShowConfirmAsync(XamlRoot,
                "确认卸载 BML 吗？", "卸载 BML 会同时将自制地图和 Mod 被删除", close: true);
            if (result == ContentDialogResult.Primary)
            {
                var dlg = DialogHelper.ShowProcessingDialog(XamlRoot, "卸载 BML");
                _instance.UninstallBML();
                DialogHelper.FinishProcessingDialog(dlg, "卸载完毕！");
                _parentPage.NavigateTo(_instance);
            }
        }

        private async void Remove_Click(object sender, RoutedEventArgs e)
        {
            var result = await DialogHelper.ShowConfirmAsync(XamlRoot,
                "确认移除吗？", "该操作不会删除本地游戏文件，仅移除启动器内实例", close: true);
            if (result == ContentDialogResult.Primary)
            {
                App.Instances.Remove(_instance);
                _parentPage.NavigateToOverview();
            }

        }

        private async void Uninstall_Click(object sender, RoutedEventArgs e)
        {
            var result = await DialogHelper.ShowConfirmAsync(XamlRoot,
                "要卸载吗？！", "该操作会永久删除本地游戏文件！", close: true);
            if (result == ContentDialogResult.Primary)
            {
                _instance.Delete();
                App.Instances.Remove(_instance);
                _parentPage.NavigateToOverview();
            }
        }
    }
}
