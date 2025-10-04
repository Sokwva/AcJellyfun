using System.Collections.Generic;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.AcJellyfun.Model;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
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

        public async Task<IEnumerable<RemoteSearchResult>> GetSearchResults(MovieInfo searchInfo, CancellationToken cancellationToken)
        {
            Log($"GetSearchResults: {searchInfo?.Name} {searchInfo?.OriginalTitle} {searchInfo?.Path}");
            string acid = searchInfo?.GetProviderId(BaseProviderId);
            if (string.IsNullOrEmpty(acid))
            {
                return [];
            }

            DougaInfoApiResp? resp = await FetchDougaInfo(acid, cancellationToken).ConfigureAwait(false);
            if (resp == null)
            {
                return [];
            }

            if (resp.Code != 0)
            {
                return [];
            }

            Log($"Return SearchResult of {acid} Description: {resp.Data.Description}");

            List<RemoteSearchResult> result =
            [
                new()
                {
                    ProviderIds = new Dictionary<string, string> { { BaseProviderId, acid }, { AcJellyfunSpId, SingleVideoProviderId + "_ac" + acid } },
                    Name = resp.Data.Title,
                    ImageUrl = resp.Data.CoverURL,
                    Overview = RemoveHTMLTagInStr(UnicodeIncludedStrToNormalStr(resp.Data.Description)),
                    ProductionYear = GetYearFromCreateTime(resp.Data.CreateTime),
                }
            ];
            return result;
        }

        public async Task<MetadataResult<Movie>> GetMetadata(MovieInfo info, CancellationToken cancellationToken)
        {
            Log($"GetMetadata: {info?.Name} {info?.GetProviderId(BaseProviderId)} {info?.Path}");

            string? acid = info?.GetProviderId(BaseProviderId);
            if (string.IsNullOrEmpty(acid) || !RegAcid.IsMatch(acid))
            {
                Log("acid 不符合要求");
                return new MetadataResult<Movie> { };
            }

            DougaInfoApiResp? resp = await FetchDougaInfo(acid, cancellationToken).ConfigureAwait(false);
            if (resp == null || resp.Code != 0)
            {
                return new MetadataResult<Movie> { };
            }

            Log($"Got DougaInfoApiResp of {acid}");

            // string? sid = info.GetProviderId(SingleVideoProviderId);
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
            Log($"Write Metadata of acid {acid}");

            result.Item = movie;
            result.QueriedById = true;
            result.HasMetadata = true;

            result.AddPerson(new MediaBrowser.Controller.Entities.PersonInfo
            {
                Name = resp.Data.User.Name,
                Type = Data.Enums.PersonKind.Producer,
                Role = "Up",
                ImageUrl = resp.Data.User.HeadUrl,
                ProviderIds = new Dictionary<string, string> { { AcJellyfunSpId, SingleVideoProviderId + "_user_" + resp.Data.User.ID } },
            });

            if (resp.Data.TagList!=null && resp.Data.TagList.Count!=0)
            {
                foreach (DougaInfoTagList item in resp.Data.TagList)
                {
                    result.Item.AddTag(item.Name);
                }
            }

            StaffApi? staffs = await FetchStaffs(acid, cancellationToken).ConfigureAwait(false);
            if (staffs != null && staffs.Result == 0 && staffs.StaffInfo.Count != 0)
            {
                foreach (StaffItem staff in staffs.StaffInfo)
                {
                    result.AddPerson(new MediaBrowser.Controller.Entities.PersonInfo
                    {
                        Name = staff.Name,
                        Type = Data.Enums.PersonKind.Editor,
                        Role = staff.StaffRoleName,
                        ImageUrl = staff.HeadURL,
                        ProviderIds = new Dictionary<string, string> { { AcJellyfunSpId, SingleVideoProviderId + "_user_" + staff.Id } }
                    });
                }
            }
            Log($"Write Person Metadata of user {resp.Data.User.Name}");
            return result;
        }

        /// <inheritdoc/>
        public async Task<HttpResponseMessage?> GetImageResponse(string url, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(url))
            {
                return null;
            }

            using HttpRequestMessage req = new(HttpMethod.Get, url);
            req.Headers.Add("User-Agent", UserAgentStr);
            req.Headers.Add("Refer", ReferStr);
            Log($"Sent Image Request of {url}");
            return await httpClient.GetAsync(url, cancellationToken).ConfigureAwait(false);
        }



        protected static int GetYearFromCreateTime(string createTime)
        {
            if (string.IsNullOrEmpty(createTime))
            {
                return 0;
            }

            string year = createTime[..4];
            int result;
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

-----

播放：{dougaInfoApiResp?.Data?.ViewCount ?? 0}
投蕉：{dougaInfoApiResp?.Data?.BananaCount ?? 0}
点赞：{dougaInfoApiResp?.Data?.LikeCount ?? 0}
收藏：{dougaInfoApiResp?.Data?.StowCount ?? 0}";
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