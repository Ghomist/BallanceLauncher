using BallanceLauncher.Utils;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Threading.Tasks;
using Windows.Globalization.NumberFormatting;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace BallanceLauncher.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingsPage : Page
    {
        public SettingsPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            //DecimalFormatter formatter = new()
            //{
            //    IntegerDigits = 1,
            //    FractionDigits = 1,
            //    NumberRounder = new IncrementNumberRounder()
            //    {
            //        Increment = 0.1,
            //        RoundingAlgorithm = RoundingAlgorithm.RoundHalfToEven
            //    }
            //};
            //ForceFetchInterval.NumberFormatter = formatter;

            base.OnNavigatedTo(e);
        }

        private void AcrylicOpacity_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            App.MainWindow.SetAcrylicOpacity(ConfigHelper.AcrylicOpacity);
        }

        private void ShowSystemTitleBar_Toggled(object sender, RoutedEventArgs e)
        {
            App.MainWindow.UpdateTitleBar((sender as ToggleSwitch).IsOn);
        }

        private async void ClearTemp_Click(object sender, RoutedEventArgs e)
        {
            var dlg = DialogHelper.ShowProcessingDialog(XamlRoot, "删除下载缓存");
            await FileHelper.DeleteTemporaryFilesAsync();
            DialogHelper.FinishProcessingDialog(dlg, "完成！");
        }

        private void RestoreConfig_Click(object sender, RoutedEventArgs e)
        {
            ConfigHelper.ClearConfig();
            Bindings.Update();
        }
    }
}
