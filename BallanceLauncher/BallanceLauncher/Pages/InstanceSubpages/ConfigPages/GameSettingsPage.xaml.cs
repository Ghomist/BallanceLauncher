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

        public GameSettingsPage()
        {
            this.InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            _instance = e.Parameter as BallanceInstance;
            _database = await TdbHelper.ReadDatabaseAsync(_instance.Database);

            Volume.Value = _database.Volume;
            CloudLayer.IsOn = _database.CloudLayer;
            SynchToScreen.IsOn = _database.SynchToScreen;
            InvertCamRotation.IsOn = _database.InvertCamRotation;

            for (int i = 1; i <= 12; ++i)
            {
                var text = new TextBlock
                {
                    Style = Resources["PropKey"] as Style,
                    Text = $"Level_{i:d02}"
                };
                LevelStatus.Children.Add(text);
                Grid.SetRow(text, i);

                var toggle = new ToggleSwitch
                {
                    IsOn = _database.GetLockedOf(i),
                    Tag = $"Level_{i:d02}",
                };
                toggle.Toggled += ToggleSwitch_Toggled;
                //Binding binding = new Binding() { Path = new("_database.") };
                LevelStatus.Children.Add(toggle);
                Grid.SetRow(toggle, i);
                Grid.SetColumn(toggle, 1);
            }

            SaveIngameSettings.IsEnabled = false;
            SaveIngameSettings2.IsEnabled = false;

            base.OnNavigatedTo(e);
        }

        private void ToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            var toggle = sender as ToggleSwitch;
            if (toggle == null) return;
            var tag = toggle.Tag;
            if (tag != null)
            {
                var tagString = tag.ToString();
                if (tagString.StartsWith("Level_"))
                {
                    int level = int.Parse(tagString[^2..]);
                    _database.SetLockedOf(level, toggle.IsOn);
                }
            }
            SaveIngameSettings.IsEnabled = true;
            SaveIngameSettings2.IsEnabled = true;
            //Bindings.Update(); // TODOs
        }

        private void Slider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            SaveIngameSettings.IsEnabled = true;
            SaveIngameSettings2.IsEnabled = true;
        }

        private async void SaveIngameSettings_Click(object sender, RoutedEventArgs e)
        {
            var dlg = DialogHelper.ShowProcessingDialog(XamlRoot, "写入设置");
            SaveIngameSettings.IsEnabled = false;
            SaveIngameSettings2.IsEnabled = false;
            _database.Volume = (int)Volume.Value;
            _database.CloudLayer = CloudLayer.IsOn;
            _database.SynchToScreen = SynchToScreen.IsOn;
            _database.InvertCamRotation = InvertCamRotation.IsOn;
            await TdbHelper.WriteDatabaseAsync(_database, _instance.Database);
            DialogHelper.FinishProcessingDialog(dlg, "搞定！");
        }
    }
}
