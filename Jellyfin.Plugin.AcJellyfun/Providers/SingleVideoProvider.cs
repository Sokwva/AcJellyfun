using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.AcJellyfun.Model;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Providers;
using Microsoft.Extensions.Logging;
using MediaBrowser.Controller.Entities.Movies;

namespace Jellyfin.Plugin.AcJellyfun.Providers
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SingleVideoProvider"/> class.
    /// </summary>
    /// <param name="httpClient"></param>
    /// <param name="loggerFactory"></param>
    public class SingleVideoProvider(HttpClient httpClient, ILoggerFactory loggerFactory) : BaseProvider(httpClient, loggerFactory.CreateLogger<SingleVideoProvider>()), IRemoteMetadataProvider<Movie, MovieInfo>
    {
        /// <inheritdoc/>
        public string Name => Plugin.PluginName;

        public const string SingleVideoProviderId = "AcFunSingleVideoProvider";
        protected HttpClient httpClient = new()
        {
            Timeout = System.TimeSpan.FromSeconds(60)
        };

        public async Task<IEnumerable<RemoteSearchResult>> GetSearchResults(MovieInfo searchInfo, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(searchInfo?.Name))
            {
                return [];
            }

            Log($"GetRemoteSearchResults of {searchInfo.Name}");
            string[] pathsplit = searchInfo.Path.Split("/");
            string folderName = pathsplit.Last();
            if (string.IsNullOrEmpty(folderName))
            {
                return [];
            }

            if (!RegAcid.IsMatch(folderName))
            {
                return [];
            }

            string acid = folderName;

            DougaInfoApiResp? resp = await FetchDougaInfo(folderName, cancellationToken).ConfigureAwait(false);
            if (resp == null)
            {
                return [];
            }

            if (resp.Code != 0)
            {
                return [];
            }

            List<RemoteSearchResult> result =
            [
                new()
                {
                    ProviderIds = new Dictionary<string, string> { { BaseProviderId, acid }, { AcJellyfunSpId, SingleVideoProviderId + "_ac" + acid } },
                    ImageUrl = resp.Data.CoverURL,
                    Overview = resp.Data.Description,
                    ProductionYear = GetYearFromCreateTime(resp.Data.CreateTime),
                }
            ];
            return result;
        }

        public async Task<MetadataResult<Movie>> GetMetadata(MovieInfo info, CancellationToken cancellationToken)
        {
            if (info == null)
            {
                return new MetadataResult<Movie>{};
            }

            string? dirName = Path.GetDirectoryName(info.Path);
            if (string.IsNullOrEmpty(dirName) || !RegAcid.IsMatch(dirName))
            {
                return new MetadataResult<Movie>{};
            }

            string acid = dirName;
            DougaInfoApiResp? resp = await FetchDougaInfo(acid, cancellationToken).ConfigureAwait(false);
            if (resp == null || resp.Code != 0)
            {
                return new MetadataResult<Movie>{};
            }

            string? sid = info.GetProviderId(SingleVideoProviderId);
            MetadataResult<Movie> result = new() { };
            Movie movie = new()
            {
                ProviderIds = new Dictionary<string, string> { { BaseProviderId, acid }, { AcJellyfunSpId, SingleVideoProviderId + "_ac" + acid } },
                Name = resp.Data.Title,
                OriginalTitle = resp.Data.Title,
                Overview = BuildOverview(resp),
                ProductionYear = GetYearFromCreateTime(resp.Data.CreateTime),
                HomePageUrl = resp.Data.ShareURL,
            };

            result.Item = movie;
            result.QueriedById = true;
            result.HasMetadata = true;

            result.AddPerson(new MediaBrowser.Controller.Entities.PersonInfo
            {
                Name = resp.Data.User.Name,
                Type = Data.Enums.PersonKind.Producer,
                Role = "Up",
                ImageUrl = resp.Data.User.HeadUrl,
                ProviderIds = new Dictionary<string, string> { { AcJellyfunSpId, SingleVideoProviderId + string.Empty } }
            });
            return result;
        }

        public async Task<HttpResponseMessage> GetImageResponse(string url, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(url))
            {
                return new HttpResponseMessage() { };
            }

            using HttpRequestMessage req = new(HttpMethod.Get, url);
            req.Headers.Add("User-Agent", UserAgentStr);
            req.Headers.Add("Refer", ReferStr);
            return await httpClient.GetAsync(url, cancellationToken).ConfigureAwait(false);
        }

        protected static int GetYearFromCreateTime(string createTime)
        {
            if (string.IsNullOrEmpty(createTime))
            {
                return 0;
            }

            string year = createTime[..3];
            int result = 0;
            return int.TryParse(year, out result) ? result : 0;
        }

        /// <summary>
        /// BuildOverview.
        /// </summary>
        /// <returns>strings.</returns>
        protected static string BuildOverview(DougaInfoApiResp dougaInfoApiResp)
        {
            if (string.IsNullOrEmpty(dougaInfoApiResp?.Data?.Description))
            {
                return string.Empty;
            }

            string unicodeRemovedDesc = UnicodeIncludedStrToNormalStr(dougaInfoApiResp.Data.Description);
            string htmlTagRemovedDesc = RemoveHTMLTagInStr(unicodeRemovedDesc);

            string desc = $@"{htmlTagRemovedDesc}
            =================================
播放：{dougaInfoApiResp?.Data?.ViewCount ?? 0}
投蕉：{dougaInfoApiResp?.Data?.BananaCount ?? 0}
点赞：{dougaInfoApiResp?.Data?.LikeCount ?? 0}
收藏：{dougaInfoApiResp?.Data?.StowCount ?? 0}

源地址：{dougaInfoApiResp?.Data?.ShareURL ?? "待发现"}
            ";
            return desc;
        }

        protected static string UnicodeIncludedStrToNormalStr(string rawText)
        {
            if (string.IsNullOrEmpty(rawText))
            {
                return string.Empty;
            }

            string srcText = rawText;

            Regex regex = new("""\\u[0-z]{4}""");

            MatchCollection unicodeInSrcText = regex.Matches(srcText);
            foreach (Match m in unicodeInSrcText)
            {
                string after = Regex.Unescape(m.Value);
                srcText = srcText.Replace(m.Value, after, System.StringComparison.Ordinal);
            }

            return srcText;
        }

        protected static string RemoveHTMLTagInStr(string rawText)
        {
            string str1 = Regex.Replace(rawText, "<[^>]+>", string.Empty);
            string str2 = Regex.Replace(str1, "&[^;]+;", string.Empty);
            return str2;
        }
    }
}