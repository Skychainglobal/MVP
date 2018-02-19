using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Storage.Lib;

namespace Storage.Engine
{
    internal class ReplicationFolder : IReplicationFolder
    {
        public ReplicationFolder(StorageEngine storage, Folder folder, IStorageNode node, IReplicationFolderMetadata metadata)
        {
            if (storage == null)
                throw new ArgumentNullException("storage");

            if (folder == null)
                throw new ArgumentNullException("folder");
            if (node == null) throw new ArgumentNullException("node");

            if (metadata == null)
                throw new ArgumentNullException("metadata");

            this.TypedStorage = storage;
            this.TypedFolder = folder;
            this.Metadata = metadata;
            this.SourceStorage = node;
        }

        /// <summary>
        /// Типизированное файловое хранилище.
        /// </summary>
        internal StorageEngine TypedStorage { get; private set; }

        /// <summary>
        /// Папки репликации.
        /// </summary>
        internal Folder TypedFolder { get; private set; }

        internal IReplicationFolderMetadata Metadata { get; set; }

        #region IReplicationFolder Members

        public IFolder Folder
        {
            get { return this.TypedFolder; }
        }

        /// <summary>
        /// Целевой узел хранилища.
        /// </summary>
        public IStorageNode SourceStorage { get; private set; }

        public bool IsRecursive
        {
            get { return this.Metadata.IsRecursive; }
        }

        /// <summary>
        /// Настройка с текущего узла файлового хранилища.
        /// </summary>
        public bool IsCurrentNodeSettings
        {
            get { return this.Metadata.IsCurrentNodeSettings; }
            set { this.Metadata.IsCurrentNodeSettings = value; }
        }

        /// <summary>
        /// Настройка удалена.
        /// </summary>
        internal bool Deleted
        {
            get { return this.Metadata.Deleted; }
            set { this.Metadata.Deleted = value; }
        }

        /// <summary>
        /// Дата синхронизации папки репликации с узлом источником.
        /// </summary>
        internal DateTime LastSyncTime
        {
            get { return this.Metadata.LastSyncTime; }
            set { this.Metadata.LastSyncTime = value; }
        }

        #endregion

        internal void Update()
        {
            this.TypedStorage.MetadataAdapter.SaveReplicationFolder(this.Metadata);
        }
    }
}