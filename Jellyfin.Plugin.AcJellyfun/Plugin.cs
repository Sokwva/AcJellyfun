using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using Jellyfin.Plugin.AcJellyfun.Configuration;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Serialization;

namespace Jellyfin.Plugin.AcJellyfun
{
    /// <summary>
    /// The main plugin.
    /// </summary>
    public class Plugin : BasePlugin<PluginConfiguration>, IHasWebPages
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Plugin"/> class.
        /// </summary>
        /// <param name="applicationPaths">Instance of the <see cref="IApplicationPaths"/> interface.</param>
        /// <param name="xmlSerializer">Instance of the <see cref="IXmlSerializer"/> interface.</param>
        public Plugin(IApplicationPaths applicationPaths, IXmlSerializer xmlSerializer)
            : base(applicationPaths, xmlSerializer)
        {
            Instance = this;
        }

        public const string PluginName = "AcJellyfun";

        /// <inheritdoc />
        public override string Name => PluginName;

        /// <inheritdoc />
        public override Guid Id => Guid.Parse("e471526c-fc10-4c54-a769-3b8e85abf708");

        /// <summary>
        /// Gets the current plugin instance.
        /// </summary>
        public static Plugin? Instance { get; private set; }

        /// <inheritdoc />
        public IEnumerable<PluginPageInfo> GetPages()
        {
            // return
            // [
            //     new PluginPageInfo
            //     {
            //         Name = Name,
            //         EmbeddedResourcePath = GetType().Namespace + ".Settings.settings.html",
            //     },
            //     new PluginPageInfo
            //     {
            //         Name = Name + "js",
            //         EmbeddedResourcePath = GetType().Namespace + ".Settings.settings.js"
            //     }
            // ];
            return
            [
                new PluginPageInfo
                {
                    Name = Name,
                    EmbeddedResourcePath = string.Format(CultureInfo.InvariantCulture, "{0}.Configuration.configPage.html", GetType().Namespace)
                }
            ];
        }
    }
}