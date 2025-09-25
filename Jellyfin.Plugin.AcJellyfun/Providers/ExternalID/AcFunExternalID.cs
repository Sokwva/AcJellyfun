using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Providers;

namespace Jellyfin.Plugin.AcJellyfun.Providers.ExternalId
{
    public class AcFunExternalIdProvider : IExternalId
    {
        public string ProviderName => BaseProvider.BaseProviderName;
        public string Key => BaseProvider.BaseProviderId;
        public ExternalIdMediaType? Type => null;
        public string? UrlFormatString => "https://www.acfun.cn/v/{0}";

        public bool Supports(IHasProviderIds item)
        {
            return item is Movie;
        }
    }
}