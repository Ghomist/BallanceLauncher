using BallanceLauncher.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace BallanceLauncher.Utils
{
    public class ResourceDownloader
    {
        private const string _baseUrl = "https://bcrc-1307489590.cos.ap-nanjing.myqcloud.com/";

        private static readonly HttpClient s_client = new() { BaseAddress = new(_baseUrl) };

        #region File List
        private static readonly ResourceInfo s_ballanceEn = new("BallanceCore", "Ballance-en.zip");
        private static readonly ResourceInfo s_bml = new("BallanceCore", "BML-0.3.40.zip");
        private static readonly ResourceInfo s_infoReader = new("BallanceModInfoReader", "BallanceModInfoReader.exe");
        private static readonly ResourceInfo s_infoReaderBML = new("BallanceModInfoReader", "BML.dll");
        #endregion

        public static Task DownloadBallanceAsync(string targetPath)
        {
            return Task.Run(async () =>
            {
                // create folder
                var targetDir = new DirectoryInfo(targetPath);
                if (!targetDir.Exists) targetDir.Create();
                // download
                var tempFile = await FileHelper.OpenTempFileAsync(s_ballanceEn.FileName);
                await UpdateFromUrlAsync(s_ballanceEn.Url, tempFile.Path).ConfigureAwait(false);
                // extract
                ZipFile.ExtractToDirectory(tempFile.Path, targetDir.FullName, Encoding.UTF8, true);
            });
        }

        public static Task DownloadBMLAsync(BallanceInstance instance)
        {
            return Task.Run(async () =>
            {
                // create folder
                var targetDir = new DirectoryInfo(instance.Path);
                if (!targetDir.Exists) targetDir.Create();
                // download
                var tempFile = await FileHelper.OpenTempFileAsync(s_bml.FileName);
                await UpdateFromUrlAsync(s_bml.Url, tempFile.Path).ConfigureAwait(false);
                // extract
                ZipFile.ExtractToDirectory(tempFile.Path, targetDir.FullName, Encoding.UTF8, true);
            });
        }

        public static Task DownloadInfoReaderAsync()
        {
            return Task.Run(async () =>
            {
                var infoReader = await FileHelper.OpenLocalFileAsync(s_infoReader.FileName);
                await UpdateFromUrlAsync(s_infoReader.Url, infoReader.Path).ConfigureAwait(false);
                var fakeBml = await FileHelper.OpenLocalFileAsync(s_infoReaderBML.FileName);
                await UpdateFromUrlAsync(s_infoReaderBML.Url, fakeBml.Path).ConfigureAwait(false);
            });
        }

        private static Task UpdateFromUrlAsync(string url, string fullName)
        {
            return Task.Run(async () =>
            {
                // crc64
                ulong crc64 = 0;
                if (File.Exists(fullName))
                {
                    crc64 = await FileHelper.GetCRC64Async(fullName).ConfigureAwait(false);
                }
                // head
                var msg = new HttpRequestMessage(HttpMethod.Head, url);
                var rsp = await s_client.SendAsync(msg).ConfigureAwait(false);
                var newCrc64 = rsp.Headers.GetValues("x-cos-hash-crc64ecma").FirstOrDefault();
                // verify crc64
                if (newCrc64 != "" && crc64.ToString() == newCrc64)
                {
                    return; // cancel
                }
                // download
                using var download = await s_client.GetStreamAsync(url).ConfigureAwait(false);
                using var target = File.OpenWrite(fullName);
                await download.CopyToAsync(target).ConfigureAwait(false);
            });
        }
    }

    public readonly struct ResourceInfo
    {
        public readonly string ResourceCategory;
        public readonly string FileName;
        public readonly string Url;
        public ResourceInfo(string category, string name) =>
            (ResourceCategory, FileName, Url) = (category, name, category + '/' + name);
    }

}
