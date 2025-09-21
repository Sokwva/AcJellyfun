using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Data.Entities.Libraries;
using Jellyfin.Plugin.AcJellyfun.Model;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Providers;
using Microsoft.Extensions.Logging;

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

        public async Task<IEnumerable<RemoteSearchResult>> GetRemoteSearchResults(MovieInfo movieInfo, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(movieInfo?.Name))
            {
                return [];
            }

            Log($"GetRemoteSearchResults of {movieInfo.Name}");
            string[] pathsplit = movieInfo.Path.Split("/");
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

            List<RemoteSearchResult> result =
            [
                new()
                {
                    ProviderIds = new Dictionary<string, string> { { BaseProviderId, acid }, { AcJellyfunSpId, SingleVideoProviderId + "_" + acid } },
                    ImageUrl = resp.Data.CoverURL,
                    Overview = BuildOverview(resp),
                    ProductionYear = GetYearFromCreateTime(resp.Data.CreateTime),
                }
            ];
            return result;
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

            Regex regex = new Regex("""\\u[0-z]{4}""");

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