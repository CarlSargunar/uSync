﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Services;

namespace uSync8.Core.Dependency
{
    public class TemplateChecker : ISyncDependencyChecker<ITemplate>
    {
        private IFileService fileService;

        public TemplateChecker(IFileService fileService)
        {
            this.fileService = fileService;
        }

        public UmbracoObjectTypes ObjectType => UmbracoObjectTypes.Template;

        public IEnumerable<uSyncDependency> GetDependencies(ITemplate item)
        {
            var dependencies = new List<uSyncDependency>();

            dependencies.Add(new uSyncDependency()
            {
                Order = DependencyOrders.Templates,
                Udi = item.GetUdi()
            });

            GetParents(item, DependencyOrders.Templates - 1);

            return dependencies;
        }

        private IEnumerable<uSyncDependency> GetParents(ITemplate item, int order)
        {
            var templates = new List<uSyncDependency>();


            if (!string.IsNullOrWhiteSpace(item.MasterTemplateAlias))
            {
                var master = fileService.GetTemplate(item.MasterTemplateAlias);
                if (master != null)
                {
                    templates.Add(new uSyncDependency()
                    {
                        Order = order,
                        Udi = master.GetUdi()
                    });

                    templates.AddRange(GetParents(master, order - 1)); 
                }
            }


            return templates;
        }
    }
}
