using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Storage.Data.Blob;
using Storage.Metadata.MSSQL;

namespace Storage.Metadata.Blob.MSSQL
{
    /// <summary>
    /// Представляет объект метаданных blob.
    /// </summary>
    [MetadataClass("Blobs")]
    public class BlobMetadata : IBlobMetadata, IMetadataObject
    {
        private BlobMetadata() { }

        /// <summary>
        /// К-тор.
        /// </summary>
        /// <param name="row">Данные blob.</param>
        internal BlobMetadata(DataRow row)
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

        private bool __init_ContainerID = false;
        private int _ContainerID;
        [MetadataIndex(RelativeName = "ContainerIndex", ColumnOrder = 1)]
        [MetadataProperty("ContainerID", Indexed = true)]
        public int ContainerID
        {
            get
            {
                if (!__init_ContainerID)
                {
                    _ContainerID = this.MetadataReader.GetIntegerValue("ContainerID");
                    __init_ContainerID = true;
                }
                return _ContainerID;
            }
            set
            {
                _ContainerID = value;
                __init_ContainerID = true;
            }
        }

        private bool __init_Name = false;
        private string _Name;
        [MetadataIndex(RelativeName = "ContainerIndex", ColumnOrder = 3)]
        [MetadataProperty("Name", Size = 50)]
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

        private bool __init_Closed;
        private bool _Closed;
        [MetadataIndex(RelativeName = "ContainerIndex", ColumnOrder = 2)]
        [MetadataProperty("Closed")]
        public bool Closed
        {
            get
            {
                if (!__init_Closed)
                {
                    _Closed = this.MetadataReader.GetBooleanValue("Closed");
                    __init_Closed = true;
                }
                return _Closed;
            }
            set
            {
                _Closed = value;
                __init_Closed = true;
            }
        }

        private bool __init_IntegrityPosition;
        private long _IntegrityPosition;
        /// <summary>
        /// Позиция, файлы до которой имеют целостность метаданных.
        /// </summary>
        [MetadataProperty("IntegrityPosition")]
        public long IntegrityPosition
        {
            get
            {
                if (!__init_IntegrityPosition)
                {
                    object position = this.MetadataReader.GetValue("IntegrityPosition");
                    if (position != null && position != DBNull.Value)
                        _IntegrityPosition = Convert.ToInt64(position);

                    __init_IntegrityPosition = true;
                }
                return _IntegrityPosition;
            }
            set
            {
                _IntegrityPosition = value;
                __init_IntegrityPosition = true;
            }
        }

        internal static BlobMetadata Create(string name, int containerID)
        {
            if (String.IsNullOrEmpty(name))
                throw new ArgumentNullException("name");

            if (containerID < 1)
                throw new ArgumentOutOfRangeException("containerID");

            BlobMetadata metadata = new BlobMetadata()
            {
                ID = 0,
                ContainerID = containerID,
                Name = name,
                IntegrityPosition = 0
            };

            return metadata;
        }

        public void UpdateIntegrityPosition(long integrityPosition)
        {
            if (integrityPosition < 0)
                throw new ArgumentNullException("integrityPosition");

            if (this.IntegrityPosition > integrityPosition)
                throw new ArgumentNullException("integrityPosition должна быть больше существующей позиции.");
            else if (this.IntegrityPosition == integrityPosition)
                return;
            else
            {
                //необходимо обновить позицию целостности блоба
                this.IntegrityPosition = integrityPosition;
            }
        }
    }
}
