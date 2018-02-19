using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Storage.Lib;

namespace Storage.Engine
{
    internal class StorageNode : IStorageNode
    {
        public StorageNode(StorageEngine storage, IStorageMetadata metadata)
        {
            if (storage == null)
                throw new ArgumentNullException("storage");

            if (metadata == null)
                throw new ArgumentNullException("metadata");

            this.TypedStorage = storage;
            this.Metadata = metadata;
        }

        /// <summary>
        /// Типизированное файловое хранилище.
        /// </summary>
        internal StorageEngine TypedStorage { get; private set; }

        /// <summary>
        /// Метаданные узла хранилища.
        /// </summary>
        internal IStorageMetadata Metadata { get; private set; }

        public Guid UniqueID { get { return this.Metadata.UniqueID; } }

        public bool IsCurrent { get { return this.Metadata.IsCurrent; } }

        public DateTime LastAccessTime
        {
            get { return this.Metadata.LastAccessTime; }
            set { this.Metadata.LastAccessTime = value; }
        }

        public string Host
        {
            get { return this.Metadata.Host; }
            set { this.Metadata.Host = value; }
        }

        internal void Update()
        {
            this.TypedStorage.MetadataAdapter.SaveStorage(this.Metadata);
        }
    }
}