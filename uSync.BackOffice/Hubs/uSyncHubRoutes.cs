﻿
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;

using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.Routing;
using Umbraco.Extensions;

namespace uSync.BackOffice.Hubs
{
    /// <summary>
    /// Handles SignlarR routes for uSync
    /// </summary>
    public class uSyncHubRoutes : IAreaRoutes
    {
        private readonly IRuntimeState _runtimeState;
        private readonly string _umbracoPathSegment;

        /// <summary>
        ///  Constructor (called via DI)
        /// </summary>
        public uSyncHubRoutes(
            IOptions<GlobalSettings> globalSettings,
            IHostingEnvironment hostingEnvironment,
            IRuntimeState runtimeState)
        {
            _runtimeState = runtimeState;
            _umbracoPathSegment = globalSettings.Value.GetUmbracoMvcArea(hostingEnvironment);
        }

        /// <summary>
        /// Create the signalR routes for uSync
        /// </summary>
        public void CreateRoutes(IEndpointRouteBuilder endpoints)
        {
            if (_runtimeState.Level == Umbraco.Cms.Core.RuntimeLevel.Run) 
                endpoints.MapHub<SyncHub>(GetuSyncHubRoute());
        }

        /// <summary>
        /// Get the path to the uSync SignalR route
        /// </summary>
        public string GetuSyncHubRoute()
            => $"/{_umbracoPathSegment}/{nameof(SyncHub)}";
    }
}
