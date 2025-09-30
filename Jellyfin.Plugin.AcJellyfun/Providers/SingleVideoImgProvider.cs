using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Providers;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.AcJellyfun.Providers
{
    public class SingleVideoImgProvider : BaseProvider, IRemoteImageProvider
    {
        public class SingleVideoImgProvider(HttpClient httpClient, ILoggerFactory loggerFactory) : BaseProvider(httpClient, loggerFactory.CreateLogger<SingleVideoImgProvider>()), IRemoteImageProvider
        {
            public string Name => Plugin.PluginName;
            public bool Supports(BaseItem item) => item is Movie;
            public IEnumerable<ImageType> GetSupportedImages(BaseItem baseItem) => new List<ImageType>
            {
                ImageType.Primary,
                ImageType.Backdrop,
                ImageType.Art,
                ImageType.Logo
            };

            public async Task<IEnumerable<RemoteImageInfo>> GetImages(BaseItem item, CancellationToken cancellationToken)
            {
                var itemSid = item.GetProviderId(BaseProviderId)
            }

        }
    }
}