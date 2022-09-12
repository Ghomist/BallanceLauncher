using BallanceLauncher.Model;
using Microsoft.UI.Xaml.Media;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using Windows.Storage;
using Windows.UI;

namespace BallanceLauncher.Utils
{
    public class MapDownloader
    {
        public static bool MapListFreshing { get; private set; } = false;

        private static readonly string s_mapsSavePath = "custom_maps.json";
        private static List<BMap> s_maps = null;
        private static DateTime s_updateTime = new(0);

        private static readonly HttpClient s_client = new(new HttpClientHandler()
        {
            //ServerCertificateCustomValidationCallback = delegate { return false; }
        });
        private static readonly string s_referer = "http://cg.ys168.com/f_ht/ajcx/000ht.html?bbh=1134";
        private static readonly List<string> s_excludeString = new() { "Ballance", "自制" };

        private static readonly string s_patternUpdateURL = "http://t.bcrc.site/resources-match.json";

        private static string s_ysIndexPage;
        private static string s_ysFileListPage;
        private static string s_categoryRegex;
        private static string s_fileRegex;
        // default values
        // s_ysIndexPage = $"http://cc.ysepan.com/f_ht/ajcx/ml.aspx?cz=ml_dq&_dlmc={s_username}&_dlmm="
        // s_ysFileListPage = $"http://cc.ysepan.com/f_ht/ajcx/wj.aspx?cz=dq&jsq=0&mlbh={{CategoryID}}&wjpx=1&_dlmc={s_username}&_dlmm="
        // s_categoryRegexPattern = "<li[^<>]+id=\"ml_([0-9]+)\"[^<>]*>.*?<a [^<>]*>([^<>]+)</a><label>([^<>]+)?</label>.*?</li>"
        // s_fileRegexPatter = "<li(?:[^<>]+)>.*?<a[^<>]+?href=\"([^\">]+)\"(?:[^<>]+)?>([^<>]+)<\\/a><i>([^<]+)<\\/i><b>\\s*" +
        //  "([^\\s|<>]+(?:\\s+[^\\s|<>]+)*)?\\s*\\|?\\s*([^\\s|<>]+(?:\\s+[^\\s|<>]+)*)?\\s*\\|?\\s*([^\\" +
        //  "s|<>]+(?:\\s+[^\\s|<>]+)*)?\\s*<\\/b><span(?:[^<>]+)>([^<>]+)<\\/span>.*?<\\/a><\\/li>"

        private static bool s_init = false;

        public static async Task FreshPatternAsync()
        {
            try
            {
                var patternStr = await s_client.GetStringAsync(s_patternUpdateURL).ConfigureAwait(false);
                var keyValuePairs = JObject.Parse(patternStr);
                s_ysIndexPage = keyValuePairs["categoryIndex"]["link"].Value<string>();
                s_categoryRegex = keyValuePairs["categoryIndex"]["regex"].Value<string>();
                s_ysFileListPage = keyValuePairs["fileIndex"]["link"].Value<string>();
                s_fileRegex = keyValuePairs["fileIndex"]["regex"].Value<string>();
            }
            catch (HttpRequestException)
            {
                s_ysIndexPage = $"http://cc.ysepan.com/f_ht/ajcx/ml.aspx?cz=ml_dq&_dlmc=ballancemaps&_dlmm=";
                s_ysFileListPage = $"http://cc.ysepan.com/f_ht/ajcx/wj.aspx?cz=dq&jsq=0&mlbh={{category}}&wjpx=1&_dlmc=ballancemaps&_dlmm=";
                s_categoryRegex = "<li[^<>]+id=\"ml_([0-9]+)\"[^<>]*>.*?<a [^<>]*>([^<>]+)</a><label>([^<>]+)?</label>.*?</li>";
                s_fileRegex = "<li(?:[^<>]+)>.*?<a[^<>]+?href=\"([^\">]+)\"(?:[^<>]+)?>([^<>]+)<\\/a><i>([^<]+)<\\/i><b>\\s*" +
                 "([^\\s|<>]+(?:\\s+[^\\s|<>]+)*)?\\s*\\|?\\s*([^\\s|<>]+(?:\\s+[^\\s|<>]+)*)?\\s*\\|?\\s*([^\\" +
                 "s|<>]+(?:\\s+[^\\s|<>]+)*)?\\s*<\\/b><span(?:[^<>]+)>([^<>]+)<\\/span>.*?<\\/a><\\/li>";
            }
        }

        public static async ValueTask<List<BMap>> GetMapsAsync(bool force = false)
        {
            MapListFreshing = true;

            if (!s_init)
            {
                await FreshPatternAsync().ConfigureAwait(false);
                s_client.DefaultRequestHeaders.Add("Referer", s_referer);
                s_init = true;
            }

            if (force)
            {
                await TryFetchMapsOnlineAsync().ConfigureAwait(false);
                return s_maps;
            }

            if (s_maps == null || (DateTime.UtcNow - s_updateTime).TotalHours > 2)
            {
                var success = await TryFetchMapsLocalAsync().ConfigureAwait(false);
                if (!success)
                    await TryFetchMapsOnlineAsync().ConfigureAwait(false);
            }
            MapListFreshing = false;
            return s_maps;
        }

        private static Task<bool> TryFetchMapsLocalAsync() =>
            Task.Run(async () =>
            {
                DateTime modifiedTime = await FileHelper.GetConfigModifiedTimeAsync(s_mapsSavePath).ConfigureAwait(false);
                if ((DateTime.UtcNow - modifiedTime).TotalHours <= 2) // read from local
                {
                    var jsonText = await FileHelper.ReadLocalFileAsync(s_mapsSavePath).ConfigureAwait(false);

                    if (jsonText != null && jsonText != "")
                    {
                        s_maps = JsonConvert.DeserializeObject<List<BMap>>(jsonText);
                        s_updateTime = DateTime.UtcNow;
                        return true;
                    }
                }

                return false;
            });

        private static Task TryFetchMapsOnlineAsync() =>
            Task.Run(async () =>
            {
                var indexPage = await GetYsPageContent(s_ysIndexPage).ConfigureAwait(false);
                s_maps ??= new();
                s_maps.Clear();

                foreach (Match match in Regex.Matches(indexPage, s_categoryRegex))
                {
                    var categoryText = match.Groups[2].Value;
                    var categoryID = match.Groups[1].Value;

                    // exclude some text
                    foreach (var text in s_excludeString) categoryText = categoryText.Replace(text, "");

                    // filter
                    if (!categoryText.Contains("图") && !categoryText.Contains("竞速")) continue;

                    // find maps
                    var mapListPage = await GetYsPageContent(s_ysFileListPage.Replace("{category}", categoryID));
                    foreach (Match match1 in Regex.Matches(mapListPage, s_fileRegex))
                    {
                        int difficulty = match1.Groups[5].Value.Count(c => c == '★');
                        var map = new BMap
                        {
                            Index = s_maps.Count,
                            Url = match1.Groups[1].Value,
                            Name = match1.Groups[2].Value,
                            Size = match1.Groups[3].Value,
                            Author = match1.Groups[4].Value,
                            Difficulty = difficulty,
                            Notes = match1.Groups[6].Value,
                            UploadTime = match1.Groups[7].Value,
                            Category = categoryText
                        };
                        if (!map.Name.EndsWith(".nmo", StringComparison.OrdinalIgnoreCase)) continue;
                        map.Name = map.Name.Replace(".level", "", StringComparison.OrdinalIgnoreCase)
                            .Replace(".nmo", "", StringComparison.OrdinalIgnoreCase);

                        s_maps.Add(map);
                    }
                }
                // save to json
                var jsonText = JsonConvert.SerializeObject(s_maps, Formatting.None);
                await FileHelper.WriteLocalFileAsync(s_mapsSavePath, jsonText).ConfigureAwait(false);

                s_updateTime = DateTime.UtcNow;
            });

        public static Task<bool> DownloadMap(string url, string mapName, List<BallanceInstance> instances) =>
            Task.Run(async () =>
            {
                //await App.Window.TellDownloadBeginAsync();

                try
                {
                    var temp = await FileHelper.TemporaryFolder.CreateFileAsync(mapName, Windows.Storage.CreationCollisionOption.ReplaceExisting);
                    var fs = await temp.OpenStreamForWriteAsync().ConfigureAwait(false);
                    // download
                    using var stream = await s_client.GetStreamAsync(url).ConfigureAwait(false);
                    await stream.CopyToAsync(fs).ConfigureAwait(false);

                    if (temp.IsAvailable) // IDK why i put this line here!!!
                    {
                        foreach (var instance in instances)
                        {
                            if (instance.HasBMLInstalled)
                            {
                                var f = new FileInfo(instance.MapDir + mapName + ".nmo");
                                if (!f.Exists) f.Create();
                                var target = await StorageFile.GetFileFromPathAsync(f.FullName);
                                await temp.CopyAndReplaceAsync(target);
                            }
                        }
                    }
                    return true;
                }
                catch (Exception)
                {
                    //DialogHelper.ShowErrorMessageAsync(App.Window.Content.XamlRoot,
                    //    "哇啊我不知道怎么就下载失败了！要不你自己试一试呢？");
                    return false;
                }
                finally
                {
                    //await App.Window.TellDownloadFinishAsync();
                }
            });


        private static async Task<string> GetYsPageContent(string url)
        {
            var result = await s_client.GetAsync(url).ConfigureAwait(false);
            var rawContent = await result.Content.ReadAsStringAsync().ConfigureAwait(false);
            return HttpUtility.HtmlDecode(rawContent);
        }
    }

    public class BMap
    {
        private static readonly Color s_light = Color.FromArgb(10, 200, 200, 200);
        private static readonly Color s_dark = Color.FromArgb(10, 128, 128, 128);

        public int Index { get; set; }
        public string Category { get; set; }
        public string Url { get; set; }
        public string Name { get; set; }
        public string Size { get; set; }
        public string Author { get; set; }
        public int Difficulty { get; set; }
        public string DifficultyString
        {
            get => Difficulty == 0
                ? "未知"
                : new StringBuilder().Append('★', Difficulty).ToString();
        }
        private string _notes = "";
        public string Notes
        {
            get => _notes == "" ? "暂无描述" : _notes;
            set => _notes = value;
        }
        public string UploadTime { get; set; }
    }
}
