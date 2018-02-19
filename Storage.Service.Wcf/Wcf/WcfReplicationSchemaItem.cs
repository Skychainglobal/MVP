using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using Storage.Engine;

namespace Storage.Service.Wcf
{
    public class WcfReplicationSchemaItem : IReplicationSchemaItem
    {
        public WcfReplicationSchemaItem() { }

        public WcfReplicationSchemaItem(IReplicationSchemaItem sourceSchemaItem)
        {
            if (sourceSchemaItem == null)
                throw new ArgumentNullException("sourceSchemaItem");

            this.Schema = sourceSchemaItem;
        }

        public WcfReplicationSchemaItem(WcfReplicationSchemaItemMessage sourceSchemaItemMessage)
        {
            if (sourceSchemaItemMessage == null)
                throw new ArgumentNullException("sourceSchemaItemMessage");

            this.Message = sourceSchemaItemMessage;
        }

        public IReplicationSchemaItem Schema { get; set; }

        public WcfReplicationSchemaItemMessage Message { get; set; }


        private bool __init_RelationType;
        private ReplicationRelation _RelationType;
        public ReplicationRelation RelationType
        {
            get
            {
                if (!__init_RelationType)
                {
                    if (this.Schema != null)
                        _RelationType = this.Schema.RelationType;
                    else
                        _RelationType = this.Message.RelationType;

                    __init_RelationType = true;
                }
                return _RelationType;
            }
        }

        private bool __init_Folders;
        private IEnumerable<string> _Folders;
        public IEnumerable<string> Folders
        {
            get
            {
                if (!__init_Folders)
                {
                    if (this.Schema != null)
                        _Folders = this.Schema.Folders;
                    else
                        _Folders = this.Message.Folders;

                    __init_Folders = true;
                }
                return _Folders;
            }
        }

        private bool __init_StorageID;
        private Guid _StorageID;
        public Guid StorageID
        {
            get
            {
                if (!__init_StorageID)
                {
                    if (this.Schema != null)
                        _StorageID = this.Schema.StorageID;
                    else
                        _StorageID = this.Message.StorageID;

                    __init_StorageID = true;
                }
                return _StorageID;
            }
        }

        private bool __init_Name;
        private string _Name;
        public string Name
        {
            get
            {
                if (!__init_Name)
                {
                    if (this.Schema != null)
                        _Name = this.Schema.Name;
                    else
                        _Name = this.Message.Name;

                    __init_Name = true;
                }
                return _Name;
            }
        }
    }
}