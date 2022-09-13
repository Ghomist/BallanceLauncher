using Newtonsoft.Json;
using BallanceLauncher.Utils;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Runtime.Serialization;
using BallanceLauncher.Pages;

namespace BallanceLauncher.Model
{
    public class BallanceMod
    {
        public string DisplayName { get; private set; }
        public string FullName { get; private set; }
        public BallanceModType Type { get; private set; }
        public bool Enable { get; private set; }
        public bool Exists { get; private set; }
        public bool IsSelected { get; set; } // selected in mod list

        public string Hash { get; private set; }

        public Symbol DisplaySymbol =>
            Type switch
            {
                BallanceModType.Zip or BallanceModType.BMod => Symbol.Setting,
                BallanceModType.Folder => Symbol.Repair,
                _ => Symbol.Help,
            };

        public ToolTip TypeTip =>
            new()
            {
                Content = Type switch
                {
                    BallanceModType.Zip => "这是一个打包的 Mod 文件\n包含 Mod 文件与其它资源",
                    BallanceModType.BMod => "这是一个 Ballance Mod 独立文件",
                    BallanceModType.Folder => "这是一个 Ballance Mod 文件夹",
                    _ => "我也不知道这是什么类型"
                }
            };

        public BallanceModInfo Details { get; private set; }

        public BallanceMod() { }
        public BallanceMod(string fullName, string displayName, BallanceModType type, bool enable)
        {
            DisplayName = displayName;
            Enable = enable;
            Type = type;
            FullName = fullName;
            Exists = true;
            IsSelected = false;
            Hash = null;

            Task.Run(TryUpdateInfoAsync);
        }

        public async Task TryUpdateInfoAsync()
        {
            var realHash = await FileHelper.GetRealHashAsync(FullName).ConfigureAwait(false);
            if (realHash == Hash) return;
            Hash = realHash;
            string modPath = FullName;

            if (Type == BallanceModType.Zip)
                // extract zip file
                modPath = await FileHelper.ExtractModAsync(FullName, DisplayName).ConfigureAwait(false);

            try
            {
                // read mod info
                var readerProcess = ProcessHelper.RunProcess(App.InfoReaderPath, args: modPath);

                var stdout = await readerProcess.StandardOutput.ReadToEndAsync().ConfigureAwait(false);

                var keyValuePairs = JObject.Parse(stdout);
                if (keyValuePairs["Status"].Value<int>() == 0)
                    // read successfully
                    Details = JsonConvert.DeserializeObject<BallanceModInfo>(stdout);
                else
                    Details = new BallanceModInfo()
                    {
                        Message = keyValuePairs["Message"].Value<string>(),
                        Status = keyValuePairs["Status"].Value<int>(),
                        Mod = null
                    };
            }
            catch (Exception ex)
            {
                Details = new BallanceModInfo()
                {
                    Message = ex.Message,
                    Status = -1,
                    Mod = null
                };
            }
        }

        public string GetTypeString() =>
            Type switch
            {
                BallanceModType.Zip => "Zipped Mod",
                BallanceModType.BMod => "Ballance Mod",
                BallanceModType.Folder => "Mod Folder",
                _ => null,
            };


        public void SetEnable(bool enable)
        {
            if (enable == Enable) return;
            var file = new FileInfo(FullName);
            if (enable)
            {
                FullName = FullName[..(FullName.Length - 8)];
            }
            else
            {
                FullName += ".disable";
            }

            // Rename
            file.MoveTo(FullName);

            Enable = enable;
        }

        public void Delete()
        {
            File.Delete(FullName);
            Exists = false;
        }

        public override bool Equals(object obj) => obj is BallanceMod mod
            && (mod.DisplayName, mod.Type, mod.Hash) == (DisplayName, Type, Hash);

        public override string ToString() => DisplayName;

        public override int GetHashCode() => Hash.GetHashCode();

        public static bool ShouldSerializeDetails()
        {
            // don't serialize the Details property!
            return false;
        }
    }

    public class BallanceModInfo
    {
        public string Message { get; set; }
        public int Status { get; set; }
        public ModDetails Mod { get; set; }
        public class ModDetails
        {
            public string ID { get; set; }
            public string Name { get; set; }
            public string Author { get; set; }
            public string Version { get; set; }
            public string Description { get; set; }
            public string BuildVersion { get => $"{BML.Major}.{BML.Minor}.{BML.Build}"; }
            public BMLVersion BML { get; set; }
            public class BMLVersion
            {
                public int Major { get; set; }
                public int Minor { get; set; }
                public int Build { get; set; }
            }
        }

        public override string ToString() => JsonConvert.SerializeObject(this, Formatting.Indented);
    }

    public enum BallanceModType
    {
        BMod, Zip, Folder
    }
}
