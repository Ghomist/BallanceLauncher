using BallanceLauncher.Utils;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Threading.Tasks;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace BallanceLauncher.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingsPage : Page
    {
        private Config _config;
        private bool _modified;

        public SettingsPage()
        {
            _config = new Config(App.Config);
            _modified = false;
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
        }

        protected override async void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
            if (_modified)
            {
                var result = await DialogHelper.ShowConfirmAsync(XamlRoot, "别急！", "还没保存呢！要不要保存呢",
                    secondary: true).ConfigureAwait(false);
                if (result == ContentDialogResult.Primary) App.Config.UpdateFrom(_config);
            }
        }

        private void SaveNow(TeachingTip sender, object args)
        {
            //App.Config = new Config(_config);
            App.Config.UpdateFrom(_config);
            sender.IsOpen = false;
            _modified = false;
        }

        private void TextBoxChanged(object sender, TextChangedEventArgs e)
        {
            PropChange();
        }

        private void SliderChange(object sender, RangeBaseValueChangedEventArgs e)
        {
            PropChange();
        }

        private void BoolChange(object sender, RoutedEventArgs e)
        {
            PropChange();
        }

        private void PropChange()
        {
            if (!IsLoaded) return;
            _modified = true;
            SaveTip.IsOpen = true;
        }

        private void RestartButton_Click(object sender, RoutedEventArgs e)
        {
            App.RestartWindow();
        }

        private void AcrylicOpacity_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            App.Window.SetAcrylicOpacity(_config.AcrylicOpacity);
        }

        private void BackgoundIndex_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            App.Window.SetBackground(_config.BackgroundIndex);
        }
    }
}
