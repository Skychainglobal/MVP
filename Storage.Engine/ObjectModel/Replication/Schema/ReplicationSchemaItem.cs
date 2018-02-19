using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Storage.Lib;

namespace Storage.Engine
{
    /// <summary>
    /// Схема репликации для хранилища.
    /// </summary>
    internal class ReplicationSchemaItem : IReplicationSchemaItem
    {
        public ReplicationSchemaItem(ReplicationFolder replicationFolder)
        {
            if (replicationFolder == null)
                throw new ArgumentNullException("replicationFolder");

            this.ReplicationFolder = replicationFolder;
        }

        public ReplicationFolder ReplicationFolder { get; private set; }

        private bool __init_RelationType;
        private ReplicationRelation _RelationType;
        public ReplicationRelation RelationType
        {
            get
            {
                if (!__init_RelationType)
                {
                    if (this.ReplicationFolder.IsCurrentNodeSettings)
                        _RelationType = ReplicationRelation.Strong;
                    else
                        _RelationType = ReplicationRelation.Weak;

                    __init_RelationType = true;
                }
                return _RelationType;
            }
        }

        #region IReplicationSchemaItem Members

        public string Name
        {
            get { return this.ReplicationFolder.SourceStorage.Host; }
        }

        public Guid StorageID
        {
            get { return this.ReplicationFolder.SourceStorage.UniqueID; }
        }

        private bool __init_TypedFolders;
        private List<string> _TypedFolders;
        private List<string> TypedFolders
        {
            get
            {
                if (!__init_TypedFolders)
                {
                    _TypedFolders = new List<string>();
                    _TypedFolders.Add(this.ReplicationFolder.Folder.Url);
                    __init_TypedFolders = true;
                }
                return _TypedFolders;
            }
        }


        public IEnumerable<string> Folders
        {
            get { return this.TypedFolders; }
        }
        #endregion

        internal void Merge(ReplicationFolder replicationFolder)
        {
            if (replicationFolder == null)
                throw new ArgumentNullException("replicationFolder");

            this.TypedFolders.Add(replicationFolder.Folder.Url);
        }
    }
}