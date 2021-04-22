﻿using System.Collections.Generic;

using Umbraco.Cms.Core.Composing;

namespace uSync.Core.Serialization
{
    public class SyncSerializerCollectionBuilder
        : LazyCollectionBuilderBase<SyncSerializerCollectionBuilder, USyncSerializerCollection, ISyncSerializerBase>
    {
        protected override SyncSerializerCollectionBuilder This => this;
    }


    public class USyncSerializerCollection : BuilderCollectionBase<ISyncSerializerBase>
    {
        public USyncSerializerCollection(IEnumerable<ISyncSerializerBase> items)
            : base(items)
        { }
    }
}
