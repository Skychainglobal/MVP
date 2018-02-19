using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Storage.Engine;
using Storage.Metadata.MSSQL;

namespace Storage.Metadata.Blob.MSSQL
{
    /// <summary>
    /// Представляет токен доступа к файлу.
    /// </summary>
    [MetadataClass("Tokens")]
    public class FileToken : IFileToken, IMetadataObject, IFileRelativeObject
    {
        /// <summary>
        /// К-тор.
        /// </summary>
        /// <param name="row">Данные.</param>
        /// <param name="version">Метаданные версии файла.</param>
        internal FileToken(DataRow row, IFileVersionMetadata version)
        {
            if (version == null)
                throw new ArgumentNullException("version");

            this.MetadataRow = row;
            this.FileVersion = version;
            this.FileMetadata = version.FileMetadata;
        }

        private IFileMetadata _FileMetadata;
        /// <summary>
        /// Метаданные файла.
        /// </summary>
        public IFileMetadata FileMetadata
        {
            get { return _FileMetadata; }
            internal set { _FileMetadata = value; }
        }

        private IFileVersionMetadata _FileVersion;
        /// <summary>
        /// Метаданные версии файла.
        /// </summary>
        public IFileVersionMetadata FileVersion
        {
            get { return _FileVersion; }
            internal set { _FileVersion = value; }
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

        private bool __init_FolderUrl = false;
        private string _FolderUrl;
        /// <summary>
        /// Адрес папки.
        /// </summary>
        public string FolderUrl
        {
            get
            {
                if (!__init_FolderUrl)
                {
                    _FolderUrl = this.FileMetadata.FolderMetadata.Url;
                    __init_FolderUrl = true;
                }
                return _FolderUrl;
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
            internal set
            {
                _FileID = value;
                __init_FileID = true;
            }
        }

        private bool __init_FileUniqueID = false;
        private Guid _FileUniqueID;
        /// <summary>
        /// Уникальный идентификатор файла.
        /// </summary>
        public Guid FileUniqueID
        {
            get
            {
                if (!__init_FileUniqueID)
                {
                    _FileUniqueID = this.FileMetadata.UniqueID;
                    __init_FileUniqueID = true;
                }
                return _FileUniqueID;
            }
        }

        private bool __init_VersionID = false;
        private int _VersionID;
        [MetadataProperty("VersionID", Indexed = true)]
        public int VersionID
        {
            get
            {
                if (!__init_VersionID)
                {
                    _VersionID = this.MetadataReader.GetIntegerValue("VersionID");
                    __init_VersionID = true;
                }
                return _VersionID;
            }
            internal set
            {
                _VersionID = value;
                __init_VersionID = true;
            }
        }

        private bool __init_VersionUniqueID = false;
        private Guid _VersionUniqueID;
        /// <summary>
        /// Уникальный идентификатор версии файла.
        /// </summary>
        public Guid VersionUniqueID
        {
            get
            {
                if (!__init_VersionUniqueID)
                {
                    _VersionUniqueID = this.FileVersion.UniqueID;
                    __init_VersionUniqueID = true;
                }
                return _VersionUniqueID;
            }
        }

        private bool __init_Expired = false;
        private DateTime _Expired;
        [MetadataProperty("Expired", Indexed = true)]
        public DateTime Expired
        {
            get
            {
                if (!__init_Expired)
                {
                    _Expired = this.MetadataReader.GetDateTimeValue("Expired");
                    __init_Expired = true;
                }
                return _Expired;
            }
            internal set
            {
                _Expired = value;
                __init_Expired = true;
            }
        }

        private bool __init_SecurityIdentifier = false;
        private string _SecurityIdentifier;
        [MetadataProperty("SecurityIdentifier", Indexed = true, IsNullable = true, Size = 100)]
        public string SecurityIdentifier
        {
            get
            {
                if (!__init_SecurityIdentifier)
                {
                    _SecurityIdentifier = this.MetadataReader.GetStringValue("SecurityIdentifier");
                    __init_SecurityIdentifier = true;
                }
                return _SecurityIdentifier;
            }
            set
            {
                _SecurityIdentifier = value;
                __init_SecurityIdentifier = true;
            }
        }
    }
}
