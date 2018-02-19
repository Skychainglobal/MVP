using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Storage.Engine;
using Storage.Data.Blob;
using Storage.Metadata.MSSQL;
using Storage.Lib;

namespace Storage.Metadata.Blob.MSSQL
{
    /// <summary>
    /// Представляет объект метаданных файла.
    /// </summary>
    [MetadataClass("Files")]
    public class FileMetadata : IBlobFileMetadata, IMetadataObject
    {
        /// <summary>
        /// К-тор. Используется при создании файла.
        /// </summary>
        /// <param name="adapter">Адаптер метаданных файла.</param>
        internal FileMetadata(FileAdapter adapter)
        {
            if (adapter == null)
                throw new ArgumentNullException("adapter");

            this.Adapter = adapter;
        }

        /// <summary>
        /// К-тор.
        /// </summary>
        /// <param name="row">Данные.</param>
        /// <param name="adapter">Адаптер метаданных файла.</param>
        internal FileMetadata(DataRow row, FileAdapter adapter)
        {
            if (row == null)
                throw new ArgumentNullException("row");

            if (adapter == null)
                throw new ArgumentNullException("adapter");

            this.MetadataRow = row;
            this.Adapter = adapter;
        }

        /// <summary>
        /// К-тор.
        /// </summary>
        /// <param name="row">Данные.</param>
        /// <param name="adapter">Адаптер метаданных файла.</param>
        /// <param name="folderMetadata">Метаданные папки.</param>
        internal FileMetadata(DataRow row, FileAdapter adapter, IFolderMetadata folderMetadata)
        {
            if (row == null)
                throw new ArgumentNullException("row");

            if (adapter == null)
                throw new ArgumentNullException("adapter");

            if (folderMetadata == null)
                throw new ArgumentNullException("folderMetadata");

            this.FolderMetadata = folderMetadata;
            this.MetadataRow = row;
            this.Adapter = adapter;
        }

        private DataRow _MetadataRow;
        private DataRow MetadataRow
        {
            get { return _MetadataRow; }
            set { _MetadataRow = value; }
        }

        private bool __init_MetadataReader = false;
        private DataRowReader _MetadataReader;
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

        private FileAdapter _Adapter;
        /// <summary>
        /// Адаптер метаданных файлов.
        /// </summary>
        public FileAdapter Adapter
        {
            get { return _Adapter; }
            set { _Adapter = value; }
        }

        private bool __init_ID = false;
        private int _ID;
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

        private bool __init_UniqueID = false;
        private Guid _UniqueID;
        [MetadataProperty("UniqueID", Indexed = true)]
        public Guid UniqueID
        {
            get
            {
                if (!__init_UniqueID)
                {
                    _UniqueID = this.MetadataReader.GetGuidValue("UniqueID");
                    __init_UniqueID = true;
                }
                return _UniqueID;
            }
            internal set
            {
                _UniqueID = value;
                __init_UniqueID = true;
            }
        }

        private bool __init_VersionUniqueID = false;
        private Guid _VersionUniqueID;
        [MetadataProperty("VersionUniqueID")]
        public Guid VersionUniqueID
        {
            get
            {
                if (!__init_VersionUniqueID)
                {
                    _VersionUniqueID = this.MetadataReader.GetGuidValue("VersionUniqueID");
                    __init_VersionUniqueID = true;
                }
                return _VersionUniqueID;
            }
            set
            {
                _VersionUniqueID = value;
                __init_VersionUniqueID = true;
            }
        }

        private bool __init_BlobID = false;
        private int _BlobID;
        [MetadataProperty("BlobID")]
        public int BlobID
        {
            get
            {
                if (!__init_BlobID)
                {
                    _BlobID = this.MetadataReader.GetIntegerValue("BlobID");
                    __init_BlobID = true;
                }
                return _BlobID;
            }
            set
            {
                _BlobID = value;
                __init_BlobID = true;
            }
        }

        private bool __init_FolderID = false;
        private int _FolderID;
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

        private bool __init_Name = false;
        private string _Name;
        [MetadataProperty("Name", Size = 400)]
        public string Name
        {
            get
            {
                if (!__init_Name)
                {
                    _Name = this.MetadataReader.GetStringValue("Name");
                    __init_Name = true;
                }
                return _Name;
            }
            set
            {
                _Name = value;
                __init_Name = true;
            }
        }

        private bool __init_TimeCreated = false;
        private DateTime _TimeCreated;
        [MetadataProperty("TimeCreated")]
        public DateTime TimeCreated
        {
            get
            {
                if (!__init_TimeCreated)
                {
                    _TimeCreated = this.MetadataReader.GetDateTimeValue("TimeCreated");
                    __init_TimeCreated = true;
                }
                return _TimeCreated;
            }
            internal set
            {
                _TimeCreated = value;
                __init_TimeCreated = true;
            }
        }

        private bool __init_TimeModified = false;
        private DateTime _TimeModified;
        [MetadataProperty("TimeModified")]
        public DateTime TimeModified
        {
            get
            {
                if (!__init_TimeModified)
                {
                    _TimeModified = this.MetadataReader.GetDateTimeValue("TimeModified");
                    __init_TimeModified = true;
                }
                return _TimeModified;
            }
            internal set
            {
                _TimeModified = value;
                __init_TimeModified = true;
            }
        }

        private bool __init_ModifiedUserID = false;
        private int _ModifiedUserID;
        [MetadataProperty("ModifiedUserID")]
        public int ModifiedUserID
        {
            get
            {
                if (!__init_ModifiedUserID)
                {
                    _ModifiedUserID = this.MetadataReader.GetIntegerValue("ModifiedUserID");
                    __init_ModifiedUserID = true;
                }
                return _ModifiedUserID;
            }
            set
            {
                _ModifiedUserID = value;
                __init_ModifiedUserID = true;
            }
        }

        private bool __init_BlobStartPosition = false;
        private long _BlobStartPosition;
        [MetadataProperty("BlobStartPosition")]
        public long BlobStartPosition
        {
            get
            {
                if (!__init_BlobStartPosition)
                {
                    object startPosition = this.MetadataReader.GetValue("BlobStartPosition");

                    _BlobStartPosition = Convert.ToInt64(startPosition);
                    __init_BlobStartPosition = true;
                }
                return _BlobStartPosition;
            }
            set
            {
                _BlobStartPosition = value;
                __init_BlobStartPosition = true;
            }
        }

        private bool __init_BlobEndPosition = false;
        private long _BlobEndPosition;
        [MetadataProperty("BlobEndPosition")]
        public long BlobEndPosition
        {
            get
            {
                if (!__init_BlobEndPosition)
                {
                    object endPosition = this.MetadataReader.GetValue("BlobEndPosition");

                    _BlobEndPosition = Convert.ToInt64(endPosition);
                    __init_BlobEndPosition = true;
                }
                return _BlobEndPosition;
            }
            set
            {
                _BlobEndPosition = value;
                __init_BlobEndPosition = true;
            }
        }

        private bool __init_Size = false;
        private long _Size;
        [MetadataProperty("Size")]
        public long Size
        {
            get
            {
                if (!__init_Size)
                {
                    object size = this.MetadataReader.GetValue("Size");

                    _Size = Convert.ToInt64(size);
                    __init_Size = true;
                }
                return _Size;
            }
            set
            {
                _Size = value;
                __init_Size = true;
            }
        }

        private bool __init_Deleted = false;
        private bool _Deleted;
        [MetadataProperty("Deleted", IsNullable = true)]
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
            internal set
            {
                _Deleted = value;
                __init_Deleted = true;
            }
        }

        private bool __init_FolderMetadata = false;
        private IFolderMetadata _FolderMetadata;
        /// <summary>
        /// Метаданные папки.
        /// </summary>
        public IFolderMetadata FolderMetadata
        {
            get
            {
                if (!__init_FolderMetadata)
                {
                    _FolderMetadata = this.Adapter.MetadataAdapter.GetFolder(this.FolderID, true);
                    __init_FolderMetadata = true;
                }
                return _FolderMetadata;
            }
            internal set
            {
                _FolderMetadata = value;
                __init_FolderMetadata = true;
            }
        }

        private bool __init_Versions = false;
        private FileVersionsCollection _Versions;
        /// <summary>
        /// Коллекция всех версий файла.
        /// </summary>
        internal FileVersionsCollection Versions
        {
            get
            {
                if (!__init_Versions)
                {
                    _Versions = this.Adapter.VersionAdapter.GetFileVersions(this);
                    __init_Versions = true;
                }
                return _Versions;
            }
        }

        internal Guid OriginalVersionUniqueID { get; private set; }

        internal void ResetVersions()
        {
            this.__init_Versions = false;
        }

        /// <summary>
        /// Обеспечивает св-ва файла для сохранения.
        /// </summary>
        public void EnsureSaveProperties()
        {
            this.OriginalVersionUniqueID = this.VersionUniqueID;
            this.VersionUniqueID = Guid.NewGuid();
        }

        #region IBlobFileMetadata Members

        /// <summary>
        /// Файл с удаленного узла сети.
        /// </summary>
        internal IRemoteFile RemoteFile { get; private set; }

        internal bool PreventSavePreviousVersion { get; private set; }

        public void EnsureRemoteSaveProperties(IRemoteFile remoteFile)
        {
            if (remoteFile == null)
                throw new ArgumentNullException("remoteFile");

            if (remoteFile.CreatedStorageNode == null)
                throw new ArgumentNullException("remoteFile.CreatedStorageNode");

            if (remoteFile.UniqueID == null)
                throw new ArgumentNullException("remoteFile.UniqueID");

            if (remoteFile.VersionID == null)
                throw new ArgumentNullException("fileVersionUniqueID");

            this.UniqueID = remoteFile.UniqueID;
            this.VersionUniqueID = remoteFile.VersionID;

            if (this.ID > 0)
            {
                //файл уже существует, все версии, которые в него загружаются
                //должны иметь тот же идентификатор файла
                //новая версия файла с удаленного узла не соответствует этому условию
                if (this.UniqueID != remoteFile.UniqueID)
                    throw new Exception(string.Format("Идентификатор локального файла не совпадает с идентификатором файла с удаленного узла."));

                //сравниваем даты изменения файлов, если с удаленного сервера
                //пришла более новая версия, то записываем ее и в файл
                //в случае если пришла более ранняя версия ничего не делаем.
                //у файла должен остаться идентификатор последней версии
                //файл вообще не должен пересохраниться
                if (this.TimeModified > remoteFile.TimeCreated)
                    this.PreventSavePreviousVersion = true;
                else
                    this.TimeModified = remoteFile.TimeCreated;
            }

            this.RemoteFile = remoteFile;
        }

        #endregion
    }
}