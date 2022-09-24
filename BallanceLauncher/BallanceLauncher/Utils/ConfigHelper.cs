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

        #region Public Props
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
        #endregion

        #region Private Props
        public static string DefaultInstance
        {
            get => s_localSettings.Values[nameof(DefaultInstance)] as string;
            set => s_localSettings.Values[nameof(DefaultInstance)] = value;
        }
        public static int WindowHeight
        {
            get => (int)(s_localSettings.Values[nameof(WindowHeight)] ?? 720);
            set => s_localSettings.Values[nameof(WindowHeight)] = value;
        }
        public static int WindowWidth
        {
            get => (int)(s_localSettings.Values[nameof(WindowWidth)] ?? 1280);
            set => s_localSettings.Values[nameof(WindowWidth)] = value;
        }
        #endregion

        private static readonly string[] s_publicProps = new string[]
        {
            nameof(ForceFetchInterval),
            nameof(ShowSystemTitleBar),
            nameof(AcrylicOpacity),
        };
        public static void ClearConfig()
        {
            // clear public props only
            foreach (var prop in s_publicProps)
            {
                s_localSettings.Values.Remove(prop);
            }
        }
    }
}
