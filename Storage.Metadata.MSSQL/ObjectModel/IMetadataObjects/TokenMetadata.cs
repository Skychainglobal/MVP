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
    [MetadataClass("AccessTokens")]
    public class TokenMetadata : IToken, IMetadataObject
    {
        internal TokenMetadata() { }

        internal TokenMetadata(DataRow row)
        {
            this.MetadataRow = row;
        }

        private DataRow MetadataRow { get; set; }

        private bool __init_MetadataReader;
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

        private bool __init_ID;
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

        private bool __init_UniqueID;
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
            set
            {
                _Expired = value;
                __init_Expired = true;
            }
        }

        public bool IsValid()
        {
            bool valid = this.Expired > DateTime.Now;
            return valid;
        }
    }
}