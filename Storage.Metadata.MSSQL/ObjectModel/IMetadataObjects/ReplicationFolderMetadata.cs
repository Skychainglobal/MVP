using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Storage.Engine;

namespace Storage.Metadata.MSSQL
{
    /// <summary>
    /// Метаданные папки репликации.
    /// </summary>
    [MetadataClass("ReplicationFolders")]
    public class ReplicationFolderMetadata : IReplicationFolderMetadata, IMetadataObject
    {
        internal ReplicationFolderMetadata(IFolderMetadata folder, IStorageMetadata storage)
        {
            if (folder == null)
                throw new ArgumentNullException("folder");

            if (storage == null)
                throw new ArgumentNullException("storage");

            this.SourceStorage = storage;
            this.Folder = folder;
            this.StorageID = storage.ID;
            this.FolderID = folder.ID;
        }

        internal ReplicationFolderMetadata(DataRow row, FolderMetadata folder, StorageMetadata storage)
        {
            if (folder == null)
                throw new ArgumentNullException("folder");

            if (storage == null)
                throw new ArgumentNullException("storage");

            this.SourceStorage = storage;
            this.Folder = folder;
            this.MetadataRow = row;
            this.StorageID = storage.ID;
            this.FolderID = folder.ID;
        }

        /// <summary>
        /// Строка в таблице метаданных.
        /// </summary>
        private DataRow MetadataRow { get; set; }

        /// <summary>
        /// Узел хранилища назначения для репликации.
        /// </summary>
        public IStorageMetadata SourceStorage { get; private set; }

        /// <summary>
        /// Папка для репликации.
        /// </summary>
        public IFolderMetadata Folder { get; private set; }

        private bool __init_MetadataReader = false;
        private DataRowReader _MetadataReader;
        /// <summary>
        /// Класс для чтения строки метаданных.
        /// </summary>
        private DataRowReader MetadataReader
        {
            get
            {
                if (!__init_MetadataReader)
                {
                    _MetadataReader = new DataRowReader(this.MetadataRow);
                    __init_MetadataReader = true;
                }
                return _MetadataReader;
            }
        }

        private bool __init_ID = false;
        private int _ID;
        /// <summary>
        /// Идентификатор объекта метаданных.
        /// </summary>
        [MetadataProperty("ID", MetadataPropertyUpdateMode.SqlManaged, IsIdentity = true)]
        public int ID
        {
            get
            {
                if (!__init_ID)
                {
                    _ID = this.MetadataReader.GetIntegerValue("ID");
                    __init_ID = true;
                }
                return _ID;
            }
            internal set
            {
                _ID = value;
                __init_ID = true;
            }
        }

        private bool __init_IsRecursive;
        private bool _IsRecursive;
        /// <summary>
        /// Признак рекурсивной репликации папки.
        /// При установке данного признака все дочерние папки так же будут реплицироваться.
        /// </summary>
        [MetadataProperty("IsRecursive")]
        public bool IsRecursive
        {
            get
            {
                if (!__init_IsRecursive)
                {
                    _IsRecursive = this.MetadataReader.GetBooleanValue("IsRecursive");
                    __init_IsRecursive = true;
                }
                return _IsRecursive;
            }
            set
            {
                _IsRecursive = value;
                __init_IsRecursive = true;
            }
        }

        private bool __init_Deleted;
        private bool _Deleted;
        /// <summary>
        /// Настройка удалена.
        /// </summary>
        [MetadataProperty("Deleted")]
        public bool Deleted
        {
            get
            {
                if (!__init_Deleted)
                {
                    _Deleted = this.MetadataReader.GetBooleanValue("Deleted");
                    __init_Deleted = true;
                }
                return _Deleted;
            }
            set
            {
                _Deleted = value;
                __init_Deleted = true;

            }
        }

        private bool __init_StorageID;
        private int _StorageID;
        /// <summary>
        /// Идентификатор хранилища назначения для репликации в текущей БД.
        /// </summary>
        [MetadataProperty("StorageID")]
        public int StorageID
        {
            get
            {
                if (!__init_StorageID)
                {
                    _StorageID = this.MetadataReader.GetIntegerValue("StorageID");
                    __init_StorageID = true;
                }
                return _StorageID;
            }
            set
            {
                _StorageID = value;
                __init_StorageID = true;
            }
        }

        private bool __init_FolderID;
        private int _FolderID;
        /// <summary>
        /// Идентификатор реплицируемой папки.
        /// </summary>
        [MetadataProperty("FolderID")]
        public int FolderID
        {
            get
            {
                if (!__init_FolderID)
                {
                    _FolderID = this.MetadataReader.GetIntegerValue("FolderID");
                    __init_FolderID = true;
                }
                return _FolderID;
            }
            set
            {
                _FolderID = value;
                __init_FolderID = true;
            }
        }

        private bool __init_LastSyncTime;
        private DateTime _LastSyncTime;
        [MetadataProperty("LastSyncTime")]
        public DateTime LastSyncTime
        {
            get
            {
                if (!__init_LastSyncTime)
                {
                    _LastSyncTime = this.MetadataReader.GetDateTimeValue("LastSyncTime");
                    __init_LastSyncTime = true;
                }
                return _LastSyncTime;
            }
            set
            {
                _LastSyncTime = value;
                __init_LastSyncTime = true;
            }
        }

        private bool __init_IsCurrentNodeSettings;
        private bool _IsCurrentNodeSettings;
        [MetadataProperty("IsCurrentNodeSettings")]
        public bool IsCurrentNodeSettings
        {
            get
            {
                if (!__init_IsCurrentNodeSettings)
                {
                    _IsCurrentNodeSettings = this.MetadataReader.GetBooleanValue("IsCurrentNodeSettings");
                    __init_IsCurrentNodeSettings = true;
                }
                return _IsCurrentNodeSettings;
            }
            set
            {
                _IsCurrentNodeSettings = value;
                __init_IsCurrentNodeSettings = true;
            }
        }
    }
}