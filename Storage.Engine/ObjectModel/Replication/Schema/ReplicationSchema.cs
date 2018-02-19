using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Storage.Lib;

namespace Storage.Engine
{
    internal class ReplicationSchema : IReplicationSchema
    {
        public ReplicationSchema(IStorageNode storage, List<ReplicationSchemaItem> strongSchemaItems, List<ReplicationSchemaItem> weakSchemaItems)
        {
            if (storage == null)
                throw new ArgumentNullException("storage");

            this.Storage = storage;
            if (strongSchemaItems != null)
                this.StrongItems = strongSchemaItems.ToArray();
            if (weakSchemaItems != null)
                this.WeakItems = weakSchemaItems.ToArray();
        }

        public IStorageNode Storage { get; private set; }

        public IReplicationSchemaItem[] WeakItems { get; private set; }

        public IReplicationSchemaItem[] StrongItems { get; private set; }

        public Guid StorageID
        {
            get { return this.Storage.UniqueID; }
        }

        public string Host
        {
            get { return this.Storage.Host; }
        }
    }
}