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
using Windows.Foundation;
using System.Collections.Immutable;
using System.Data.HashFunction.CRC;

namespace BallanceLauncher.Utils
{
    public class FileHelper
    {
        public static StorageFolder LocalFolder => ApplicationData.Current.LocalFolder;
        public static StorageFolder TemporaryFolder => ApplicationData.Current.TemporaryFolder;

        private static readonly SHA256 s_sha256Encrypter = SHA256.Create();

        public static Task<string> ExtractModAsync(string fullName, string displayName) =>
            Task.Run(async () =>
            {
                var folder = await TemporaryFolder.CreateFolderAsync(displayName, CreationCollisionOption.ReplaceExisting);
                // extract
                ZipFile.ExtractToDirectory(fullName, folder.Path, true);
                // find bmod
                var files = await folder.GetFilesAsync();
                var mod = files.FirstOrDefault(file => file.FileType.ToLower() == ".bmod");
                return mod.Path;
            });

        public static Task ExtractResourceAsync(string resourcePath, string resourceName, string fullName = null) =>
            Task.Run(async () =>
            {
                var resource = "BallanceLauncher." + resourcePath + "." + resourceName;
                var assembly = Assembly.GetExecutingAssembly();
                using var input = new BufferedStream(assembly.GetManifestResourceStream(resource));

                if (fullName == null)
                {
                    var f = await LocalFolder.CreateFileAsync(resourceName);
                    using var target = await f.OpenStreamForWriteAsync();
                    await input.CopyToAsync(target);
                }
                else
                {
                    using var output = new FileStream(fullName, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
                    await input.CopyToAsync(output).ConfigureAwait(false);
                }
            });

        #region Local File
        public static Task<StorageFile> GetLocalFileAsync(string fileName)
        {
            return Task.Run(async () =>
            {
                StorageFile f = null;
                try
                {
                    f = await LocalFolder.GetFileAsync(fileName);
                }
                catch (FileNotFoundException) { }
                return f;
            });
        }

        public static IAsyncOperation<StorageFile> CreateLocalFileAsync(string fileName) =>
            LocalFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);

        public static IAsyncOperation<StorageFile> OpenLocalFileAsync(string fileName) =>
            LocalFolder.CreateFileAsync(fileName, CreationCollisionOption.OpenIfExists);

        public static Task<string> ReadLocalFileAsync(string fileName)
        {
            return Task.Run(async () =>
            {
                var d = await GetLocalFileAsync(fileName);
                return d == null ? "" : await FileIO.ReadTextAsync(d);
            });
        }

        public static Task WriteLocalFileAsync(string fileName, string content)
        {
            return Task.Run(async () =>
            {
                var configFile = await LocalFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
                await FileIO.WriteTextAsync(configFile, content);
            });
        }
        #endregion

        #region Temporary File
        public static Task<StorageFile> GetTempFileAsync(string fileName)
        {
            return Task.Run(async () =>
            {
                StorageFile f = null;
                try
                {
                    f = await TemporaryFolder.GetFileAsync(fileName);
                }
                catch (FileNotFoundException) { }
                return f;
            });
        }

        public static IAsyncOperation<StorageFile> CreateTempFileAsync(string fileName) =>
            TemporaryFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);

        public static IAsyncOperation<StorageFile> OpenTempFileAsync(string fileName) =>
            TemporaryFolder.CreateFileAsync(fileName, CreationCollisionOption.OpenIfExists);

        public static Task<string> ReadTempFileAsync(string fileName)
        {
            return Task.Run(async () =>
            {
                var d = await GetTempFileAsync(fileName);
                return d == null ? "" : await FileIO.ReadTextAsync(d);
            });
        }

        public static Task WriteTempFileAsync(string fileName, string content)
        {
            return Task.Run(async () =>
            {
                var configFile = await TemporaryFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
                await FileIO.WriteTextAsync(configFile, content);
            });
        }

        public static Task DeleteTemporaryFilesAsync() =>
            Task.Run(async () => await TemporaryFolder.DeleteAsync(StorageDeleteOption.PermanentDelete));
        #endregion

        #region Hash
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

        public static Task<ulong> GetCRC64Async(string fullName)
        {
            return Task.Run(async () =>
            {
                using var fs = new FileStream(fullName, FileMode.Open, FileAccess.Read);

                var crcConfig = CRCConfig.CRC64_XZ; // fit to Tencent COS
                var factory = CRCFactory.Instance.Create(crcConfig);
                var value = await factory.ComputeHashAsync(fs);
                return BitConverter.ToUInt64(value.Hash);
            });
        }
        #endregion

        public static Task<DateTime> GetConfigModifiedTimeAsync(string fileName)
        {
            return Task.Run(async () =>
            {
                var f = await GetLocalFileAsync(fileName);
                return f == null ? DateTime.MinValue : File.GetLastWriteTimeUtc(f.Path);
            });
        }
    }
}
