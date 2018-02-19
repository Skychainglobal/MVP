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
    /// Метаданные объекта хранилища.
    /// </summary>
    [MetadataClass("Storages")]
    public class StorageMetadata : IStorageMetadata, IMetadataObject
    {
        private StorageMetadata() { }

        internal StorageMetadata(DataRow row)
        {
            if (row == null)
                throw new ArgumentNullException("row");

            this.MetadataRow = row;
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

        private bool __init_UniqueID = false;
        private Guid _UniqueID;
        [MetadataProperty("UniqueID")]
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

        private bool __init_Host = false;
        private string _Host;
        [MetadataIndex(RelativeName = "Location", ColumnOrder = 1)]
        [MetadataProperty("Host")]
        public string Host
        {
            get
            {
                if (!__init_Host)
                {
                    _Host = this.MetadataReader.GetStringValue("Host");
                    __init_Host = true;
                }
                return _Host;
            }
            set
            {
                _Host = value;
                __init_Host = true;
            }
        }

        private bool __init_IsCurrent;
        private bool _IsCurrent;
        [MetadataProperty("IsCurrent")]
        public bool IsCurrent
        {
            get
            {
                if (!__init_IsCurrent)
                {
                    _IsCurrent = this.MetadataReader.GetBooleanValue("IsCurrent");
                    __init_IsCurrent = true;
                }
                return _IsCurrent;
            }
            set
            {
                _IsCurrent = value;
                __init_IsCurrent = true;
            }
        }

        private bool __init_LastAccessTime;
        private DateTime _LastAccessTime;
        [MetadataProperty("LastAccessTime")]
        public DateTime LastAccessTime
        {
            get
            {
                if (!__init_LastAccessTime)
                {
                    _LastAccessTime = this.MetadataReader.GetDateTimeValue("LastAccessTime");
                    __init_LastAccessTime = true;
                }
                return _LastAccessTime;
            }
            set
            {
                _LastAccessTime = value;
                __init_LastAccessTime = true;

            }
        }

        internal static StorageMetadata Create(string host, bool isCurrent)
        {
            if (string.IsNullOrEmpty(host))
                throw new ArgumentNullException("host");

            StorageMetadata metadata = StorageMetadata.Create(host, isCurrent, Guid.Empty);

            return metadata;
        }

        internal static StorageMetadata Create(string host, bool isCurrent, Guid nodeUniqueID)
        {
            if (string.IsNullOrEmpty(host))
                throw new ArgumentNullException("host");

            if (nodeUniqueID == Guid.Empty)
                nodeUniqueID = Guid.NewGuid();

            StorageMetadata metadata = new StorageMetadata()
            {
                UniqueID = nodeUniqueID,
                Host = host,
                IsCurrent = isCurrent,
            };

            return metadata;
        }
    }
}
