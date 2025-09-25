using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.AcJellyfun.Configuration;
using Jellyfin.Plugin.AcJellyfun.Model;
using MediaBrowser.Controller.Providers;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.AcJellyfun.Providers
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BaseProvider"/> class.
    /// </summary>
    public abstract class BaseProvider
    {
        /// <summary>
        /// Name.
        /// </summary>
        public const string BaseProviderName = "AcFun";
        public const string BaseProviderId = "AcFunAcID";
        public const string AcJellyfunSpId = "AcJellyfun_Single";
        protected ILogger logger;

        protected HttpClient httpClient;

        protected string UserAgentStr = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/140.0.0.0 Safari/537.36 Edg/140.0.0.0";
        protected string ReferStr = "https://www.acfun.cn/";

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseProvider"/> class.
        /// </summary>
        /// <param name="h">HttpClient.</param>
        /// <param name="l">logget.</param>
        protected BaseProvider(HttpClient h, ILogger l)
        {
            logger = l;
            httpClient = h;
            httpClient.DefaultRequestHeaders.Add("User-Agent", "Jellyfin-Plugin-" + Plugin.PluginName + "-" + PluginConfiguration.Version);
        }

        protected void Log(string? msg, params object?[] args)
        {
            if (msg == null)
            {
                return;
            }
            else
            {
                logger.LogInformation("[Svp]: {Msg} {Args}", msg, args);
            }
        }

        /// <summary>
        /// Gets conf.
        /// </summary>
        protected static PluginConfiguration Conf
        {
            get
            {
                return Plugin.Instance?.Configuration ?? new PluginConfiguration();
            }
        }

        protected static readonly Regex RegAcid = new(@"^[0-9]*[0-9][0-9]*$");

        /// <summary>
        /// 根据配置，去对应的API端点获取稿件信息.
        /// </summary>
        /// <param name="acid">acid.</param>
        /// <param name="cancellationToken">cancellationToken.</param>
        /// <returns>null or api resp.</returns>
        public async Task<DougaInfoApiResp?> FetchDougaInfo(string acid, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(acid))
            {
                return null;
            }

            if (!RegAcid.IsMatch(acid))
            {
                return null;
            }

            if (string.IsNullOrEmpty(Conf.AcFunDougaInfoApi))
            {
                return null;
            }

            string url = Conf.AcFunDougaInfoApi + "/video/?acid=" + acid;
            HttpResponseMessage resp = await httpClient.GetAsync(url, cancellationToken).ConfigureAwait(false);
            if (!resp.IsSuccessStatusCode)
            {
                Log($"从DougaInfo服务拉取 {acid} 数据失败", acid);
                return null;
            }

            string respContent = await resp.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            return string.IsNullOrEmpty(respContent) ? null : JsonSerializer.Deserialize<DougaInfoApiResp>(respContent);
        }


    }
}