using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace BallanceLauncher.Utils
{
    public class ConfigHelper
    {
        private ConfigHelper() { }

        private static readonly ApplicationDataContainer s_localSettings = ApplicationData.Current.LocalSettings;

        public static double ForceFetchInterval // unit: hour
        {
            get => (double)(s_localSettings.Values[nameof(ForceFetchInterval)] ?? 0.5);
            set => s_localSettings.Values[nameof(ForceFetchInterval)] = double.IsNaN(value) ? 0.5 : value;
        }

        public static bool ShowSystemTitleBar
        {
            get => (bool)(s_localSettings.Values[nameof(ShowSystemTitleBar)] ?? false);
            set => s_localSettings.Values[nameof(ShowSystemTitleBar)] = value;
        }

        public static double AcrylicOpacity
        {
            get => (double)(s_localSettings.Values[nameof(AcrylicOpacity)] ?? 0.7);
            set => s_localSettings.Values[nameof(AcrylicOpacity)] = value;
        }

        // do not shown in the settings page

        public static string DefaultInstance
        {
            get => s_localSettings.Values[nameof(DefaultInstance)] as string;
            set => s_localSettings.Values[nameof(DefaultInstance)] = value;
        }
    }
}
