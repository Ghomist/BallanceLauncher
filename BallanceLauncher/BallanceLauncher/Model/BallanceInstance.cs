using BallanceLauncher.Utils;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Shapes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace BallanceLauncher.Model
{
    public class BallanceInstance
    {
        private string _name;
        public string Name
        {
            get => _name;
            set
            {
                if (value == null || value == "") throw new ArgumentException("Name cannot be empty", nameof(Name));
                _name = value;
            }
        }

        private string _path;
        public string Path
        {
            get => _path;
            set
            {
                if (value == null || value == "") throw new ArgumentException("Path cannot be empty", nameof(Path));
                //if (!Directory.Exists(value)) throw new ArgumentException("Directory dose not exist", nameof(Path));
                if (value[^1] != '\\') value += '\\';
                _path = value;
            }
        }

        // paths
        public string WorkingPath => Path + @"bin\";
        public string Executable => WorkingPath + @"player.exe";
        public string BMLDir => HasBMLInstalled ? Path + @"ModLoader\" : "没有找到 BML";
        public string ModDir => HasBMLInstalled ? BMLDir + @"Mods\" : "没有安装 BML";
        public string MapDir => HasBMLInstalled ? BMLDir + @"Maps\" : "没有安装 BML";

        public bool Exists => Directory.Exists(Path) && File.Exists(Executable);
        public bool HasBMLInstalled => File.Exists(Path + @"BuildingBlocks\BML.dll") && Directory.Exists(Path + @"ModLoader\");

        public Visibility ModPropVisibility => HasBMLInstalled ? Visibility.Visible : Visibility.Collapsed;
        public Visibility MapPropVisibility => HasBMLInstalled ? Visibility.Visible : Visibility.Collapsed;

        //public bool IsDefault { get; set; }

        public BallanceInstance() { }
        public BallanceInstance(string name, string path) => (Name, Path) = (name, path);

        //[OnDeserialized]
        //internal void OnDeserializedMethod(StreamingContext context) { }

        public Task<List<BallanceMod>> GetModsAsync()
        {
            return Task.Run(() =>
            {
                if (!HasBMLInstalled) return new();

                var mods = new List<BallanceMod>();
                var modDir = new DirectoryInfo(ModDir);
                if (!modDir.Exists) return new();

                foreach (var file in modDir.GetFiles())
                {
                    string name;

                    switch (file.Extension.ToLower())
                    {
                        case ".bmod":
                            name = file.Name[..^5];
                            mods.Add(new BallanceMod(file.FullName, name, BallanceModType.BMod, true));
                            break;
                        case ".zip":
                            name = file.Name[..^4];
                            mods.Add(new BallanceMod(file.FullName, name, BallanceModType.Zip, true));
                            break;
                        case ".disable":
                            name = file.Name[..^8]; // remove '.disable'
                            if (name.EndsWith(".bmod"))
                            {
                                name = name[..^5];
                                mods.Add(new BallanceMod(file.FullName, name, BallanceModType.BMod, false));
                            }
                            else if (name.EndsWith(".zip"))
                            {
                                name = name[..^4];
                                mods.Add(new BallanceMod(file.FullName, name, BallanceModType.Zip, false));
                            }
                            break;
                    }
                }

                foreach (var folder in modDir.GetDirectories())
                {
                    string name = folder.Name;
                    FileInfo modFile = folder.GetFiles().FirstOrDefault(f => f.Name.EndsWith(".bmod", StringComparison.OrdinalIgnoreCase));
                    if (modFile != null)
                    {
                        mods.Add(new BallanceMod(modFile.FullName, name, BallanceModType.Folder, true));
                        continue;
                    }
                    // try to find disable mod
                    modFile = folder.GetFiles().FirstOrDefault(f => f.Name.EndsWith(".bmod.disable", StringComparison.OrdinalIgnoreCase));
                    if (modFile != null)
                        mods.Add(new BallanceMod(modFile.FullName, name, BallanceModType.Folder, false));
                }

                return mods;
            });
        }

        public Task<List<BallanceMap>> GetMapsAsync()
        {
            return Task.Run(() =>
            {
                if (!HasBMLInstalled) return new();

                var maps = new List<BallanceMap>();
                var mapDir = new DirectoryInfo(MapDir);
                if (!mapDir.Exists) return new();

                foreach (var file in mapDir.GetFiles())
                {
                    if (!file.Name.Contains('.')) continue;
                    var extensionType = file.Extension.ToLower();

                    if (extensionType == ".nmo")
                    {
                        maps.Add(new BallanceMap(file.FullName, file.Name[..^4], BallanceMapType.NMO));
                    }
                    else if (extensionType == ".cmo")
                    {
                        maps.Add(new BallanceMap(file.FullName, file.Name[..^4], BallanceMapType.CMO));
                    }
                }
                return maps;
            });
        }

        public async Task InstallBMLAsync() => await FileHelper.ExtractBMLAsync(this);

        public void UninstallBML(bool force = false)
        {
            if (!force && !HasBMLInstalled) return;
            var dll = Path + @"BuildingBlocks\BML.dll";
            if (File.Exists(dll)) File.Delete(dll);
            if (Directory.Exists(BMLDir)) Directory.Delete(BMLDir, true);
        }

        public void Delete()
        {
            try
            {
                Directory.Delete(Path, recursive: true);
            }
            catch (DirectoryNotFoundException)
            {
                // has been delete
            }
        }

        public override string ToString() => Name;

        public string ToJson() => JsonConvert.SerializeObject(this, Formatting.Indented);

        public static bool EnsureBallancePath(string path)
        {
            DirectoryInfo dir = new(path + @"\bin");
            if (dir.Exists)
            {
                FileInfo[] files = dir.GetFiles("player.exe");
                if (files.Length == 1)
                    return true;
            }
            return false;
        }
    }
}
