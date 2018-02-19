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
    /// Представляет объект метаданных контейнера blob.
    /// </summary>
    [MetadataClass("BlobContainers")]
    public class BlobContainerMetadata : IBlobContainerMetadata, IMetadataObject
    {
        private BlobContainerMetadata() { }

        /// <summary>
        /// к-тор.
        /// </summary>
        /// <param name="row">Данные контейнера.</param>
        internal BlobContainerMetadata(DataRow row)
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

        private bool __init_FolderID = false;
        private int _FolderID;
        [MetadataProperty("FolderID", Indexed = true)]
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


        private bool __init_Closed;
        private bool _Closed;
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

        private bool __init_Name;
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

        private bool __init_Path = false;
        private string _Path;
        [MetadataProperty("Path", Size = 400)]
        public string Path
        {
            get
            {
                if (!__init_Path)
                {
                    _Path = this.MetadataReader.GetStringValue("Path");
                    __init_Path = true;
                }
                return _Path;
            }
            set
            {
                _Path = value;
                __init_Path = true;
            }
        }

        private bool __init_SettingsXml = false;
        private string _SettingsXml;
        [MetadataProperty("SettingsXml", IsNullable = true)]
        public string SettingsXml
        {
            get
            {
                if (!__init_SettingsXml)
                {
                    _SettingsXml = this.MetadataReader.GetStringValue("SettingsXml");
                    __init_SettingsXml = true;
                }
                return _SettingsXml;
            }
            set
            {
                _SettingsXml = value;
                __init_SettingsXml = true;
            }
        }

        internal static BlobContainerMetadata Create(string name, string path, int folderID)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException("name");

            if (String.IsNullOrEmpty(path))
                throw new ArgumentNullException("path");

            BlobContainerMetadata container = new BlobContainerMetadata();
            container.Name = name;
            container.Path = path;
            container.FolderID = folderID;

            return container;
        }
    }
}
