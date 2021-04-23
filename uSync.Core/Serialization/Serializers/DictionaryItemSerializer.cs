﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;
using uSync.Core;
using uSync.Core.Models;
using uSync.Core.Serialization;

namespace uSync.Core.Serialization.Serializers
{
    [SyncSerializer("4D18F4C3-6EBC-4AAD-8D20-6353BDBBD484", "Dicrionary Serializer", uSyncConstants.Serialization.Dictionary)]
    public class DictionaryItemSerializer : SyncSerializerBase<IDictionaryItem>, ISyncSerializer<IDictionaryItem>
    {
        private readonly ILocalizationService localizationService;

        public DictionaryItemSerializer(IEntityService entityService, ILogger<DictionaryItemSerializer> logger,
            ILocalizationService localizationService)
            : base(entityService, logger)
        {
            this.localizationService = localizationService;
        }

        protected override SyncAttempt<IDictionaryItem> DeserializeCore(XElement node, SyncSerializerOptions options)
        {
            var item = FindItem(node);

            var info = node.Element("Info");
            var alias = node.GetAlias();

            var details = new List<uSyncChange>();

            Guid? parentKey = null;
            var parentItemKey = info.Element("Parent").ValueOrDefault(string.Empty);
            if (parentItemKey != string.Empty)
            {
                var parent = localizationService.GetDictionaryItemByKey(parentItemKey);
                if (parent != null)
                {
                    parentKey = parent.Key;
                }
            }

            var key = node.GetKey();

            if (item == null)
            {
                item = new DictionaryItem(parentKey, alias);
                item.Key = key;
            }
            else
            {
                item.ParentId = parentKey;
            }

            if (item.ItemKey != alias)
            {
                details.AddUpdate("ItemKey", item.ItemKey, alias);
                item.ItemKey = alias;
            }

            if (item.Key != key)
            {
                details.AddUpdate("Key", item.Key, key);
                item.Key = key;
            }

            // key only translationm, would not add the translation values. 
            if (!options.GetSetting("KeysOnly", false))
            {
                details.AddRange(DeserializeTranslations(item, node));
            }

            // this.SaveItem(item);
            

            return SyncAttempt<IDictionaryItem>.Succeed(item.ItemKey, item, ChangeType.Import, details);
        }

        private IEnumerable<uSyncChange> DeserializeTranslations(IDictionaryItem item, XElement node)
        {
            var translationNode = node?.Element("Translations");
            if (translationNode == null) return Enumerable.Empty<uSyncChange>();

            var currentTranslations = item.Translations.ToList();

            var changes = new List<uSyncChange>();

            foreach (var translation in translationNode.Elements("Translation"))
            {
                var language = translation.Attribute("Language").ValueOrDefault(string.Empty);
                if (language == string.Empty) continue;

                var itemTranslation = item.Translations.FirstOrDefault(x => x.Language.IsoCode == language);
                if (itemTranslation != null && itemTranslation.Value != translation.Value)
                    {
                        changes.AddUpdate(language, itemTranslation.Value, translation.Value, $"{item.ItemKey}/{language}");
                        itemTranslation.Value = translation.Value;
                    
                }
                else
                {
                    var lang = localizationService.GetLanguageByIsoCode(language);
                    if (lang != null)
                    {
                        changes.AddNew(language, translation.Value, $"{item.ItemKey}/{language}");
                        currentTranslations.Add(new DictionaryTranslation(lang, translation.Value));
                    }
                }
            }

            var translations = currentTranslations.DistinctBy(x => x.Language.IsoCode).ToList();

            // if the count is wrong, we delete the item (shortly before we save it again).
            if (item.Translations.Count() > translations.Count)
            {
                var existing = FindItem(item.Key);
                if (existing != null)
                {
                    DeleteItem(existing);
                    item.Id = 0; // make this a new (so it will be inserted)
                }
            }

            item.Translations = translations; //.DistinctBy(x => x.Language.IsoCode);

            return changes;
        }

        protected override SyncAttempt<XElement> SerializeCore(IDictionaryItem item, SyncSerializerOptions options)
        {
            var node = InitializeBaseNode(item, item.ItemKey, GetLevel(item));

            var info = new XElement("Info");

            if (item.ParentId.HasValue)
            {
                var parent = FindItem(item.ParentId.Value);
                if (parent != null)
                {
                    info.Add(new XElement("Parent", parent.ItemKey));
                }
            }

            var translationsNode = new XElement("Translations");

            foreach (var translation in item.Translations
                .DistinctBy(x => x.Language.IsoCode)
                .OrderBy(x => x.Language.IsoCode))
            {
                translationsNode.Add(new XElement("Translation", translation.Value,
                    new XAttribute("Language", translation.Language.IsoCode)));
            }

            node.Add(info);
            node.Add(translationsNode);

            return SyncAttempt<XElement>.Succeed(
                item.ItemKey, node, typeof(IDictionaryItem), ChangeType.Export);
        }

        public override IDictionaryItem FindItem(int id)
            => localizationService.GetDictionaryItemById(id);

        public override IDictionaryItem FindItem(Guid key)
            => localizationService.GetDictionaryItemById(key);

        public override IDictionaryItem FindItem(string alias)
            => localizationService.GetDictionaryItemByKey(alias);

        private int GetLevel(IDictionaryItem item, int level = 0)
        {
            if (!item.ParentId.HasValue) return level;

            var parent = FindItem(item.ParentId.Value);
            if (parent != null)
                return GetLevel(parent, level + 1);

            return level;
        }

        public override void SaveItem(IDictionaryItem item)
            => localizationService.Save(item);

        public override void DeleteItem(IDictionaryItem item)
            => localizationService.Delete(item);

        public override string ItemAlias(IDictionaryItem item)
            => item.ItemKey;
    }
}
