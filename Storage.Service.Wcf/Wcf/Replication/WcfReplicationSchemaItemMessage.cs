using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using Storage.Engine;

namespace Storage.Service.Wcf
{
    [DataContract]
    public class WcfReplicationSchemaItemMessage
    {
        public WcfReplicationSchemaItemMessage(WcfReplicationSchemaItem schemaItem)
        {
            if (schemaItem == null)
                throw new ArgumentNullException("schemaItem");

            this.SchemaItem = schemaItem;
        }

        [IgnoreDataMember]
        public WcfReplicationSchemaItem SchemaItem { get; private set; }

        private bool __init_RelationType;
        private ReplicationRelation _RelationType;
        [DataMember]
        public ReplicationRelation RelationType
        {
            get
            {
                if (!__init_RelationType)
                {
                    _RelationType = this.SchemaItem.RelationType;
                    __init_RelationType = true;
                }
                return _RelationType;
            }
            set
            {
                _RelationType = value;
                __init_RelationType = true;
            }
        }

        private bool __init_Folders;
        private List<string> _Folders;
        [DataMember]
        public List<string> Folders
        {
            get
            {
                if (!__init_Folders)
                {
                    if (this.SchemaItem.Folders != null)
                        _Folders = this.SchemaItem.Folders.ToList();
                    else
                        _Folders = new List<string>();

                    __init_Folders = true;
                }
                return _Folders;
            }
            set
            {
                _Folders = value;
                __init_Folders = true;
            }
        }

        private bool __init_StorageID;
        private Guid _StorageID;
        [DataMember]
        public Guid StorageID
        {
            get
            {
                if (!__init_StorageID)
                {
                    _StorageID = this.SchemaItem.StorageID;
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

        private bool __init_Name;
        private string _Name;
        [DataMember]
        public string Name
        {
            get
            {
                if (!__init_Name)
                {
                    _Name = this.SchemaItem.Name;
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
    }
}