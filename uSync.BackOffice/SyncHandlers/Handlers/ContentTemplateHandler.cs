﻿using System;

using Microsoft.Extensions.Logging;

using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;

using uSync.BackOffice.Configuration;
using uSync.BackOffice.Services;
using uSync.Core;

using static Umbraco.Cms.Core.Constants;

namespace uSync.BackOffice.SyncHandlers.Handlers
{
    /// <summary>
    ///  Handler to manage content templates in uSync
    /// </summary>
    [SyncHandler(uSyncConstants.Handlers.ContentTemplateHandler, "Blueprints", "Blueprints", uSyncConstants.Priorites.ContentTemplate
        , Icon = "icon-document-dashed-line usync-addon-icon", IsTwoPass = true, EntityType = UdiEntityType.DocumentBlueprint)]
    public class ContentTemplateHandler : ContentHandlerBase<IContent, IContentService>, ISyncHandler
    {
        /// <summary>
        /// ContentTypeHandler belongs to the Content group by default
        /// </summary>
        public override string Group => uSyncConstants.Groups.Content;

        private readonly IContentService contentService;

        /// <summary>
        ///  Handler constrcutor Loaded via DI
        /// </summary>
        public ContentTemplateHandler(
            ILogger<ContentTemplateHandler> logger,
            IEntityService entityService,
            IContentService contentService,
            AppCaches appCaches,
            IShortStringHelper shortStringHelper,
            SyncFileService syncFileService,
            uSyncEventService mutexService,
            uSyncConfigService uSyncConfigService,
            ISyncItemFactory syncItemFactory)
            : base(logger, entityService, appCaches, shortStringHelper, syncFileService, mutexService, uSyncConfigService, syncItemFactory)
        {

            this.contentService = contentService;

            // make sure we load up the template serializer - because we need that one, not the normal content one.
            this.serializer = syncItemFactory.GetSerializer<IContent>("contentTemplateSerializer");
        }

        /// <summary>
        ///  Delete a content template via the ContentService
        /// </summary>
        protected override void DeleteViaService(IContent item)
            => contentService.DeleteBlueprint(item);

        /// <summary>
        ///  Fetch a content template via the ContentService
        /// </summary>
        protected override IContent GetFromService(int id)
            => contentService.GetBlueprintById(id);

        /// <summary>
        ///  Fetch a content template via the ContentService
        /// </summary>
        protected override IContent GetFromService(Guid key)
            => contentService.GetBlueprintById(key);

        /// <summary>
        ///  Fetch a content template via the ContentService
        /// </summary>
        protected override IContent GetFromService(string alias)
            => null;
    }
}
