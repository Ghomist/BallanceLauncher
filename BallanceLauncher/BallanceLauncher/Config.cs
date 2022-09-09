using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace BallanceLauncher
{
    public class Config
    {
        public int BackgroundIndex { get; set; }
        public bool ShowSystemTitleBar { get; set; }
        public double AcrylicOpacity { get; set; }

        public Config()
        {
            // here's the default settings
            BackgroundIndex = 3;
            ShowSystemTitleBar = false;
            AcrylicOpacity = 0.7;
        }

        public Config(Config other)
        {
            BackgroundIndex = other.BackgroundIndex;
            ShowSystemTitleBar = other.ShowSystemTitleBar;
            AcrylicOpacity = other.AcrylicOpacity;
        }

        public void UpdateFrom(Config other)
        {
            BackgroundIndex = other.BackgroundIndex;
            ShowSystemTitleBar = other.ShowSystemTitleBar;
            AcrylicOpacity = other.AcrylicOpacity;
        }
    }
}
