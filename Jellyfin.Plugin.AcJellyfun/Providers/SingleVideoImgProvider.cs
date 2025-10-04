using System;
using System.Collections.Generic;
using System.Net.Http;
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
    public class SingleVideoImgProvider : BaseProvider, IRemoteImageProvider
    {
        // public string Name => throw new System.NotImplementedException();

        // public Task<HttpResponseMessage> GetImageResponse(string url, CancellationToken cancellationToken)
        // {
        //     throw new System.NotImplementedException();
        // }

        // public Task<IEnumerable<RemoteImageInfo>> GetImages(BaseItem item, CancellationToken cancellationToken)
        // {
        //     throw new System.NotImplementedException();
        // }

        // public IEnumerable<ImageType> GetSupportedImages(BaseItem item)
        // {
        //     throw new System.NotImplementedException();
        // }

        // public bool Supports(BaseItem item)
        // {
        //     throw new System.NotImplementedException();
        // }

        /// <summary>
        /// Initializes a new instance of the <see cref="SingleVideoImgProvider"/> class.
        /// </summary>
        /// <param name="httpClient"></param>
        /// <param name="loggerFactory"></param>
        public SingleVideoImgProvider(HttpClient httpClient, ILoggerFactory loggerFactory) : base(httpClient, loggerFactory.CreateLogger<SingleVideoImgProvider>())
        {
        }
        public string Name => Plugin.PluginName;
        public bool Supports(BaseItem item) => item is Movie;
        public IEnumerable<ImageType> GetSupportedImages(BaseItem item) => new List<ImageType>
            {
                ImageType.Primary,
                ImageType.Backdrop,
                ImageType.Art,
                ImageType.Logo
            };

        public async Task<IEnumerable<RemoteImageInfo>> GetImages(BaseItem item, CancellationToken cancellationToken)
        {
            if (item == null)
            {
                return new List<RemoteImageInfo> { };
            }
            string acid = item.GetProviderId(BaseProviderId);
            Log($"GetImages: {item?.Name} {item?.OriginalTitle} {item?.Path} {acid}");
            if (string.IsNullOrEmpty(acid) || !RegAcid.IsMatch(acid))
            {
                return new List<RemoteImageInfo> { };
            }

            DougaInfoApiResp? resp = await FetchDougaInfo(acid, cancellationToken).ConfigureAwait(false);

            if (resp == null)
            {
                return new List<RemoteImageInfo> { };
            }

            if (resp.Code != 0)
            {
                return new List<RemoteImageInfo> { };
            }

            List<RemoteImageInfo> result = [
                new()
            {
                ProviderName = Name,
                Url = resp.Data.CoverURL,
                Type = ImageType.Primary,
                Language = "zh"
            },
                new()
            {
                ProviderName = Name,
                Url = resp.Data.CoverURL,
                Type = ImageType.Art,
                Language = "zh"
            },
                new()
            {
                ProviderName = Name,
                Url = resp.Data.CoverURL,
                Type = ImageType.Backdrop,
                Language = "zh"
            }
            ];

            return result;
        }

        public async Task<HttpResponseMessage> GetImageResponse(string url, CancellationToken cancellationToken)
        {
            Log($"GetImageResponse: url:{url}");
            return await httpClient.GetAsync(new Uri(url), cancellationToken).ConfigureAwait(false);
        }

    }
}