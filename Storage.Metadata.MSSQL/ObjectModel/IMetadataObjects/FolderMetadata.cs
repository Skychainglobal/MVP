using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Storage.Engine;
using Storage.Lib;

namespace Storage.Metadata.MSSQL
{
    /// <summary>
    /// Метаданные папки.
    /// </summary>
    [MetadataClass("Folders")]
    public class FolderMetadata : IFolderMetadata, IMetadataObject
    {
        internal FolderMetadata() { }

        internal FolderMetadata(DataRow row)
        {
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
        [MetadataProperty("ID", MetadataPropertyUpdateMode.SqlManaged, IsIdentity=true)]
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

        private bool __init_Name = false;
        private string _Name;
        [MetadataProperty("Name", Size=100)]
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

        private bool __init_Url = false;
        private string _Url;
        [MetadataProperty("Url", Size = 1000, Indexed=true)]
        public string Url
        {
            get
            {
                if (!__init_Url)
                {
                    _Url = this.MetadataReader.GetStringValue("Url");
                    __init_Url = true;
                }
                return _Url;
            }
            set
            {
                _Url = value;
                __init_Url = true;
            }
        }

        private bool __init_ParentID = false;
        private int _ParentID;
        [MetadataProperty("ParentID", Indexed = true, IsNullable=true)]
        public int ParentID
        {
            get
            {
                if (!__init_ParentID)
                {
                    _ParentID = this.MetadataReader.GetIntegerValue("ParentID");
                    __init_ParentID = true;
                }
                return _ParentID;
            }
            set
            {
                _ParentID = value;
                __init_ParentID = true;
            }
        }        
    }
}
