using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Storage.Engine;
using Storage.Data.Blob;
using Storage.Metadata.MSSQL;

namespace Storage.Metadata.Blob.MSSQL
{
    /// <summary>
    /// Представляет объект метаданных версии файла.
    /// </summary>
    [MetadataClass("Versions")]
    internal class FileVersionMetadata : IBlobFileVersionMetadata, IMetadataObject, IFileRelativeObject
    {
        /// <summary>
        /// К-тор.
        /// </summary>
        /// <param name="file">Метаданные файла.</param>
        internal FileVersionMetadata(FileMetadata file)
        {
            if (file == null)
                throw new ArgumentNullException("file");

            this.FileMetadata = file;
            this.FileID = file.ID;
            this.FolderID = file.FolderID;
            this.FileID = file.ID;
            this.UniqueID = file.VersionUniqueID;
            this.Name = file.Name;

            this.ModifiedUserID = file.ModifiedUserID;
            if (file.RemoteFile != null)
            {
                this.TimeCreated = file.RemoteFile.TimeCreated;
            }
            else
            {
                this.TimeCreated = file.TimeModified;
            }

            this.BlobStartPosition = file.BlobStartPosition;
            this.BlobEndPosition = file.BlobEndPosition;
            this.BlobID = file.BlobID;
            this.Size = file.Size;
        }

        /// <summary>
        /// К-тор.
        /// </summary>
        /// <param name="adapter">Коллекция версий файла.</param>
        internal FileVersionMetadata(FileVersionsCollection collection)
        {
            if (collection == null)
                throw new ArgumentNullException("collection");

            this.VersionsCollection = collection;
        }

        /// <summary>
        /// К-тор.
        /// </summary>
        /// <param name="row">Данные.</param>
        /// <param name="collection">Коллекция версий файла.</param>
        internal FileVersionMetadata(DataRow row, FileVersionsCollection collection)
        {
            if (row == null)
                throw new ArgumentNullException("row");

            if (collection == null)
                throw new ArgumentNullException("collection");

            this.MetadataRow = row;
            this.VersionsCollection = collection;
        }

        private bool __init_VersionsCollection = false;
        private FileVersionsCollection _VersionsCollection;
        /// <summary>
        /// Коллекция версий файла.
        /// </summary>
        public FileVersionsCollection VersionsCollection
        {
            get
            {
                if (!__init_VersionsCollection)
                {
                    _VersionsCollection = ((FileMetadata)this.FileMetadata).Versions;
                    __init_VersionsCollection = true;
                }
                return _VersionsCollection;
            }
            private set
            {
                _VersionsCollection = value;
                __init_VersionsCollection = true;
            }
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
            set
            {
                _ID = value;
                __init_ID = true;
            }
        }

        private bool __init_FileID = false;
        private int _FileID;
        [MetadataProperty("FileID", Indexed = true)]
        public int FileID
        {
            get
            {
                if (!__init_FileID)
                {
                    _FileID = this.MetadataReader.GetIntegerValue("FileID");
                    __init_FileID = true;
                }
                return _FileID;
            }
            set
            {
                _FileID = value;
                __init_FileID = true;
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

        private bool __init_CreatedStorageID = false;
        private int _CreatedStorageID;
        [MetadataProperty("CreatedStorageID")]
        public int CreatedStorageID
        {
            get
            {
                if (!__init_CreatedStorageID)
                {
                    _CreatedStorageID = this.MetadataReader.GetIntegerValue("CreatedStorageID");
                    __init_CreatedStorageID = true;
                }
                return _CreatedStorageID;
            }
            set
            {
                _CreatedStorageID = value;
                __init_CreatedStorageID = true;
            }
        }

        private bool __init_CreatedStorageUniqueID;
        private Guid _CreatedStorageUniqueID;
        /// <summary>
        /// 
        /// </summary>
        public Guid CreatedStorageUniqueID
        {
            get
            {
                if (!__init_CreatedStorageUniqueID)
                {
                    _CreatedStorageUniqueID = this.VersionsCollection.VersionsStorages[this.CreatedStorageID].UniqueID;
                    __init_CreatedStorageUniqueID = true;
                }
                return _CreatedStorageUniqueID;
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
            set
            {
                _UniqueID = value;
                __init_UniqueID = true;
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
            set
            {
                _TimeCreated = value;
                __init_TimeCreated = true;
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

        private bool __init_FileMetadata = false;
        private IFileMetadata _FileMetadata;
        /// <summary>
        /// Метаданные файла.
        /// </summary>
        public IFileMetadata FileMetadata
        {
            get
            {
                if (!__init_FileMetadata)
                {
                    _FileMetadata = this.VersionsCollection.File;
                    __init_FileMetadata = true;
                }
                return _FileMetadata;
            }
            set
            {
                _FileMetadata = value;
                __init_FileMetadata = true;
            }
        }
    }
}