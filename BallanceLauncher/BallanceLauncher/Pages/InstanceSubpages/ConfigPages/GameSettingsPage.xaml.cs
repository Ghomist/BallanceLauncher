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
    public sealed partial class GameSettingsPage : Page
    {
        private BallanceInstance _instance;
        private BallanceDatabase _database;

        #region Props
        private int Volume { get => _database != null ? _database.Volume : 0; set => _database.Volume = value; }
        private bool CloudLayer { get => _database != null && _database.CloudLayer; set => _database.CloudLayer = value; }
        private bool SynchToScreen { get => _database != null && _database.SynchToScreen; set => _database.SynchToScreen = value; }
        private bool InvertCamRotation { get => _database != null && _database.InvertCamRotation; set => _database.InvertCamRotation = value; }
        private bool Lv1Locked { get => _database != null && _database.GetLevelLocked(1); set => _database.SetLevelLocked(1, value); }
        private bool Lv2Locked { get => _database != null && _database.GetLevelLocked(2); set => _database.SetLevelLocked(2, value); }
        private bool Lv3Locked { get => _database != null && _database.GetLevelLocked(3); set => _database.SetLevelLocked(3, value); }
        private bool Lv4Locked { get => _database != null && _database.GetLevelLocked(4); set => _database.SetLevelLocked(4, value); }
        private bool Lv5Locked { get => _database != null && _database.GetLevelLocked(5); set => _database.SetLevelLocked(5, value); }
        private bool Lv6Locked { get => _database != null && _database.GetLevelLocked(6); set => _database.SetLevelLocked(6, value); }
        private bool Lv7Locked { get => _database != null && _database.GetLevelLocked(7); set => _database.SetLevelLocked(7, value); }
        private bool Lv8Locked { get => _database != null && _database.GetLevelLocked(8); set => _database.SetLevelLocked(8, value); }
        private bool Lv9Locked { get => _database != null && _database.GetLevelLocked(9); set => _database.SetLevelLocked(9, value); }
        private bool Lv10Locked { get => _database != null && _database.GetLevelLocked(10); set => _database.SetLevelLocked(10, value); }
        private bool Lv11Locked { get => _database != null && _database.GetLevelLocked(11); set => _database.SetLevelLocked(11, value); }
        private bool Lv12Locked { get => _database != null && _database.GetLevelLocked(12); set => _database.SetLevelLocked(12, value); }
        private string KeyForward { get => _database != null ? _database.KeyForward : ""; set { } }
        private string KeyBackward { get => _database != null ? _database.KeyBackward : ""; set { } }
        private string KeyLeft { get => _database != null ? _database.KeyLeft : ""; set { } }
        private string KeyRight { get => _database != null ? _database.KeyRight : ""; set { } }
        private string KeyLiftCam { get => _database != null ? _database.KeyLiftCam : ""; set { } }
        private string KeyRotateCam { get => _database != null ? _database.KeyRotateCam : ""; set { } }
        #endregion

        public GameSettingsPage()
        {
            this.InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            _instance = e.Parameter as BallanceInstance;
            _database = await TdbHelper.ReadDatabaseAsync(_instance.Database);
            SetSaveButtonEnable(false);
            Bindings.Update();
        }

        private void SetSaveButtonEnable(bool enable)
        {
            SaveIngameSettings.IsEnabled = enable;
            SaveIngameSettings2.IsEnabled = enable;
        }

        private void ToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            SetSaveButtonEnable(true);
        }

        private void Slider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            SetSaveButtonEnable(true);
        }

        private async void SaveIngameSettings_Click(object sender, RoutedEventArgs e)
        {
            var dlg = DialogHelper.ShowProcessingDialog(XamlRoot, "写入设置");
            SetSaveButtonEnable(false);
            await TdbHelper.WriteDatabaseAsync(_database, _instance.Database);
            DialogHelper.FinishProcessingDialog(dlg, "搞定！");
        }
    }
}
