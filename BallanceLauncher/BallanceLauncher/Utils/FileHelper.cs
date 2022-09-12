using System;
using System.IO;
using System.IO.Compression;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Reflection;
using BallanceLauncher.Model;
using Windows.Storage;
using Newtonsoft.Json;

namespace BallanceLauncher.Utils
{
    public class FileHelper
    {
        public static readonly string TempDir = App.BaseDir + @"temp\";

        private static readonly SHA256 s_sha256Encrypter = SHA256.Create();
        private static bool s_init = false;

        public static Task ExtractBallance(string pathName) =>
            Task.Run(async () =>
            {
                // create folder
                //if (!pathName.EndsWith('\\') && !pathName.EndsWith('/')) pathName += '\\';
                var targetDir = new DirectoryInfo(pathName);
                if (!targetDir.Exists) targetDir.Create();
                // release Ballance.zip
                var zipFile = new FileInfo(targetDir.FullName + "\\Ballance.zip");
                await ExtractResourceAsync("Ballance", "Ballance.zip", zipFile.FullName).ConfigureAwait(false);
                // extract
                ZipFile.ExtractToDirectory(zipFile.FullName, pathName, Encoding.UTF8, true);
                // delete zip
                zipFile.Delete();
            });

        public static Task ExtractBMLAsync(BallanceInstance instance) =>
            Task.Run(async () =>
            {
                var targetDir = new DirectoryInfo(instance.Path);
                // release BML-0.3.40.zip
                var zipFile = new FileInfo(targetDir.FullName + "\\BML.zip");
                await ExtractResourceAsync("Ballance", "BML-0.3.40.zip", zipFile.FullName).ConfigureAwait(false);
                // extract
                ZipFile.ExtractToDirectory(zipFile.FullName, targetDir.FullName, Encoding.UTF8, true);
                // delete zip
                zipFile.Delete();
            });

        public static Task<(string, string)> ExtractModAsync(string fullName, string displayName) =>
            Task.Run(() =>
            {
                if (!s_init)
                {
                    DeleteTemp();
                    CreateTemp();
                    s_init = true;
                }

                var tempPath = TempDir + displayName;
                // ensure target dir exist
                var tempDir = new DirectoryInfo(tempPath);
                if (tempDir.Exists) tempDir.Delete(true);
                tempDir.Create();
                // extract
                ZipFile.ExtractToDirectory(fullName, tempPath, true);
                // find bmod
                var mod = tempDir.GetFiles().FirstOrDefault(file => file.Extension.ToLower() == ".bmod");
                return (tempPath, mod.FullName);
            });

        public static Task ExtractResourceAsync(string resourcePath, string resourceName, string fullName = null) =>
            Task.Run(async () =>
            {
                fullName ??= App.BaseDir + resourceName;
                var resource = "BallanceLauncher." + resourcePath + "." + resourceName;
                var assembly = Assembly.GetExecutingAssembly();

                using var input = new BufferedStream(assembly.GetManifestResourceStream(resource));
                using var output = new FileStream(fullName, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
                await input.CopyToAsync(output).ConfigureAwait(false);
            });

        public static Task<string> ReadLocalFileAsync(string fileName)
        {
            return Task.Run(async () =>
            {
                var f = await App.LocalFolder.GetFileAsync(fileName);
                return await FileIO.ReadTextAsync(f);
            });
        }

        public static Task<DateTime> GetConfigModifiedTimeAsync(string fileName)
        {
            return Task.Run(async () =>
            {
                try
                {
                    var f = await App.LocalFolder.GetFileAsync(fileName);
                    return File.GetLastWriteTimeUtc(f.Path);
                }
                catch (FileNotFoundException)
                {
                    return DateTime.MinValue;
                }
            });
        }

        public static Task WriteLocalFileAsync(string fileName, string content)
        {
            return Task.Run(async () =>
            {
                var configFile = await App.LocalFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
                await FileIO.WriteTextAsync(configFile, content);
            });
        }

        public static void DeleteModTemp(string extractedPath)
        {
            DirectoryInfo dir = new(extractedPath);
            if (dir.Exists) dir.Delete(true);
        }

        public static void DeleteTemp()
        {
            DirectoryInfo dir = new(TempDir);
            try
            {
                if (dir.Exists) dir.Delete(true);
            }
            catch (Exception) { }
        }

        public static void CreateTemp()
        {
            DirectoryInfo dir = new(TempDir);
            if (!dir.Exists) dir.Create();
        }

        public static string GetRealHash(string fullName)
        {
            using var fs = new FileStream(fullName, FileMode.Open, FileAccess.Read);
            var hashBytes = s_sha256Encrypter.ComputeHash(fs);
            return BitConverter.ToString(hashBytes).Replace("-", "");
        }

        public static async Task<string> GetRealHashAsync(string fullName)
        {
            using var fs = new FileStream(fullName, FileMode.Open, FileAccess.Read);
            var hashBytes = await s_sha256Encrypter.ComputeHashAsync(fs).ConfigureAwait(false);
            return BitConverter.ToString(hashBytes).Replace("-", "");
        }
    }
}
