using BallanceLauncher.Model;
using BallanceLauncher.Utils;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Swung0x48.Ballance.TdbReader;
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
    public sealed partial class RecordPage : Page
    {
        private BallanceInstance _instance;
        private BallanceDatabase _database;

        public RecordPage()
        {
            this.InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            _instance = e.Parameter as BallanceInstance;
            base.OnNavigatedTo(e);

            _database = await TdbHelper.ReadDatabaseAsync(_instance.Database).ConfigureAwait(false);
            await FreshRecords();
        }

        private Task FreshRecords()
        {
            return Task.Run(() =>
            {
                int levelCount = _database.Lv13Enable ? 13 : 12;
                var recordLists = new List<RecordList>();
                for (int i = 1; i <= levelCount; ++i)
                {
                    var records = _database.GetRecordsOf(i);
                    recordLists.Add(new(i, records));
                }
                // update UI
                DispatcherQueue.TryEnqueue(() =>
                {
                    Records.ItemsSource = recordLists;
                });
            });
        }

        private async void ClearSingle_Click(object sender, RoutedEventArgs e)
        {
            var item = (RecordList)Records.SelectedItem;
            var level = int.Parse(item.Level);
            var result = await DialogHelper.ShowConfirmAsync(XamlRoot, "确认清除纪录？",
                 $"这会将 Level_{level:d02} 的纪录还原至默认值！", close: true);
            if (result == ContentDialogResult.Primary)
            {
                var dlg = DialogHelper.ShowProcessingDialog(XamlRoot, "清除纪录");
                _database.ClearRecordsOf(level);
                await TdbHelper.WriteDatabaseAsync(_database, _instance.Database);
                _database = await TdbHelper.ReadDatabaseAsync(_instance.Database);
                await FreshRecords();
                DialogHelper.FinishProcessingDialog(dlg, "搞定！");
            }
        }

        private async void DeleteAll_Click(object sender, RoutedEventArgs e)
        {
            var result = await DialogHelper.ShowConfirmAsync(XamlRoot, "确认清除纪录？",
                 $"这会将所有关卡的纪录还原至默认值！", close: true);
            if (result == ContentDialogResult.Primary)
            {
                var dlg = DialogHelper.ShowProcessingDialog(XamlRoot, "清除纪录");
                _database.ClearAllRecords();
                await TdbHelper.WriteDatabaseAsync(_database, _instance.Database);
                _database = await TdbHelper.ReadDatabaseAsync(_instance.Database);
                await FreshRecords();
                DialogHelper.FinishProcessingDialog(dlg, "搞定！");
            }
        }
    }

    readonly struct RecordList
    {
        public readonly string Level;
        public readonly RecordProp[] Props;
        public RecordList(int level, (string, int)[] list)
        {
            //Level = $"Level_{level:d02}";
            Level = level.ToString();
            Props = new RecordProp[list.Length];
            for (int i = 0; i < Props.Length; ++i)
            {
                Props[i] = new RecordProp(i + 1, list[i].Item1, list[i].Item2);
            }
        }
        public override string ToString() => Level;
    }

    readonly struct RecordProp
    {
        public readonly int Rank;
        public readonly string Player;
        public readonly int Score;
        public RecordProp(int rank, string player, int score) => (Rank, Player, Score) = (rank, player, score);
    }
}
