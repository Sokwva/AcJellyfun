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
                    Overview = $"",
                    ProductionYear = 2021,
                }
            ];
            return result;
        }

        /// <summary>
        /// BuildOverview.
        /// </summary>
        /// <returns>strings.</returns>
        protected static string BuildOverview(DougaInfoApiResp dougaInfoApiResp)
        {
            return string.Empty;
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
                srcText = srcText.Replace(m.Value, after);
            }

            return srcText;
        }
    }
}