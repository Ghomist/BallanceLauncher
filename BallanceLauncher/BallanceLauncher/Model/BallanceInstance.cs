using BallanceLauncher.Utils;
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
        public string Name { get; set; }

        public string Path { get; set; }
        public string WorkingPath { get; set; }

        // paths
        private string _bmlPath = "";
        public string BMLDir
        {
            get => _bmlPath == "" ? "没有找到 BML" : _bmlPath;
            set => _bmlPath = value ?? throw new ArgumentNullException(nameof(BMLDir));
        }
        private string _modDir = "";
        public string ModDir
        {
            get => _modDir == "" ? "没有安装 BML" : _modDir;
            set => _modDir = value ?? throw new ArgumentNullException(nameof(ModDir));
        }
        private string _mapDir = "";
        public string MapDir
        {
            get => _mapDir == "" ? "没有安装 BML" : _mapDir;
            set => _mapDir = value ?? throw new ArgumentNullException(nameof(MapDir));
        }
        public string Executable { get; set; }

        //public List<BallanceMod> Mods { get; private set; }
        //public List<BallanceMap> Maps { get; private set; }

        //public bool IsDefault { get; set; }
        public bool HasBMLInstalled { get; set; }

        public BallanceInstance() { }

        public BallanceInstance(string name, string path)
        {
            Name = name;
            Path = path;
            //IsDefault = false;
            WorkingPath = path + @"\bin";
            Executable = path + @"\bin\player.exe";
            BMLDir = path + @"\ModLoader";

            HasBMLInstalled = File.Exists(Path + @"\BuildingBlocks\BML.dll") && Directory.Exists(BMLDir);

            if (HasBMLInstalled)
            {
                ModDir = BMLDir + @"\Mods";
                MapDir = BMLDir + @"\Maps";
            }
        }

        [OnDeserialized]
        internal void OnDeserializedMethod(StreamingContext context) =>
            HasBMLInstalled = File.Exists(Path + @"\BuildingBlocks\BML.dll") && Directory.Exists(BMLDir);


        public bool EnsureExist() => File.Exists(Executable);


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
                            name = file.Name[..(file.Name.Length - 5)];
                            mods.Add(new BallanceMod(file.FullName, name, BallanceModType.BMod, true));
                            break;
                        case ".zip":
                            name = file.Name[..(file.Name.Length - 4)];
                            mods.Add(new BallanceMod(file.FullName, name, BallanceModType.Zip, true));
                            break;
                        case ".disable":
                            name = file.Name[..(file.Name.Length - 8)]; // remove '.disable'
                            if (name.EndsWith(".bmod"))
                            {
                                name = name[..(name.Length - 5)];
                                mods.Add(new BallanceMod(file.FullName, name, BallanceModType.BMod, false));
                            }
                            else if (name.EndsWith(".zip"))
                            {
                                name = name[..(name.Length - 4)];
                                mods.Add(new BallanceMod(file.FullName, name, BallanceModType.Zip, false));
                            }
                            break;
                    }
                }

                foreach (var folder in modDir.GetDirectories())
                {
                    string name = folder.Name;
                    FileInfo modFile = folder.GetFiles().FirstOrDefault(f => f.Name.ToLower().EndsWith(".bmod"));
                    if (modFile != null)
                    {
                        mods.Add(new BallanceMod(modFile.FullName, name, BallanceModType.Folder, true));
                        continue;
                    }
                    // try to find disable mod
                    modFile = folder.GetFiles().FirstOrDefault(f => f.Name.ToLower().EndsWith(".bmod.disable"));
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
                    if (file.Extension.ToLower() == ".nmo")
                    {
                        maps.Add(new BallanceMap(file.FullName, file.Name[..(file.Name.Length - 4)], BallanceMapType.NMO));
                    }
                    else if (file.Extension.ToLower() == ".cmo")
                    {
                        maps.Add(new BallanceMap(file.FullName, file.Name[..(file.Name.Length - 4)], BallanceMapType.CMO));
                    }
                }
                return maps;
            });
        }

        public Task InstallBMLAsync() => FileHelper.ExtractBMLAsync(this);

        public void Delete()
        {
            try
            {
                Directory.Delete(MapDir, true);
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
