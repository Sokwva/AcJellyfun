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
        public SingleVideoImgProvider(HttpClient httpClient, ILoggerFactory loggerFactory):base(httpClient,loggerFactory.CreateLogger<SingleVideoImgProvider>())
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
            Log($"GetImages: {item?.Name} {item?.OriginalTitle} {item?.Path}");
            if (item == null)
            {
                return new List<RemoteImageInfo> { };
            }
            string acid = item.GetProviderId(BaseProviderId);
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

            Log($"Return GetImages of {acid}");
            RemoteImageInfo resou = new()
            {
                ProviderName = Name,
                Url = resp.Data.CoverURL,
                Language = "zh"
            };

            List<RemoteImageInfo> result = [];
            resou.Type = ImageType.Primary;
            result.Add(resou);

            RemoteImageInfo resou2 = resou;
            resou2.Type = ImageType.Art;
            result.Add(resou2);

            RemoteImageInfo resou3 = resou;
            resou3.Type = ImageType.Backdrop;
            result.Add(resou3);

            return result;
        }

        public async Task<HttpResponseMessage> GetImageResponse(string url, CancellationToken cancellationToken)
        {
            Log($"GetImageResponse: url:{url}");
            return await httpClient.GetAsync(new Uri(url), cancellationToken).ConfigureAwait(false);
        }

    }
}