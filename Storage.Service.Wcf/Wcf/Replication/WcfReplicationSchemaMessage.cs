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
    public class WcfReplicationSchemaMessage
    {
        public WcfReplicationSchemaMessage(WcfReplicationSchema schema)
        {
            if (schema == null)
                throw new ArgumentNullException("schema");

            this.Schema = schema;
        }

        [IgnoreDataMember]
        public WcfReplicationSchema Schema { get; private set; }

        private bool __init_StrongItems;
        private WcfReplicationSchemaItemMessage[] _StrongItems;
        [DataMember]
        public WcfReplicationSchemaItemMessage[] StrongItems
        {
            get
            {
                if (!__init_StrongItems)
                {
                    if (this.Schema != null && this.Schema.StrongItems != null)
                        _StrongItems = this.Schema.StrongItems.Select(si => new WcfReplicationSchemaItemMessage(new WcfReplicationSchemaItem(si))).ToArray();

                    __init_StrongItems = true;
                }
                return _StrongItems;
            }
            set
            {
                _StrongItems = value;
                __init_StrongItems = true;
            }
        }

        private bool __init_WeakItems;
        private WcfReplicationSchemaItemMessage[] _WeakItems;
        [DataMember]
        public WcfReplicationSchemaItemMessage[] WeakItems
        {
            get
            {
                if (!__init_WeakItems)
                {
                    if (this.Schema != null && this.Schema.WeakItems != null)
                        _WeakItems = this.Schema.WeakItems.Select(si => new WcfReplicationSchemaItemMessage(new WcfReplicationSchemaItem(si))).ToArray();

                    __init_WeakItems = true;
                }
                return _WeakItems;
            }
            set
            {
                _WeakItems = value;
                __init_WeakItems = true;
            }
        }

        private bool __init_Host;
        private string _Host;
        [DataMember]
        public string Host
        {
            get
            {
                if (!__init_Host)
                {
                    if (this.Schema != null)
                        _Host = this.Schema.Host;
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

        private bool __init_StorageID;
        private Guid _StorageID;
        [DataMember]
        public Guid StorageID
        {
            get
            {
                if (!__init_StorageID)
                {
                    if (this.Schema != null)
                        _StorageID = this.Schema.StorageID;
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
    }
}