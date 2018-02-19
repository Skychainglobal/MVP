using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using Storage.Engine;

namespace Storage.Service.Wcf
{
    public class WcfReplicationSchema : IReplicationSchema
    {
        public WcfReplicationSchema() { }

        public WcfReplicationSchema(IReplicationSchema replicationSchema)
        {
            if (replicationSchema == null)
                throw new ArgumentNullException("replicationSchema");

            this.ReplicationSchema = replicationSchema;
        }

        public WcfReplicationSchema(WcfReplicationSchemaMessage replicationSchemaMessage)
        {
            if (replicationSchemaMessage == null)
                throw new ArgumentNullException("replicationSchemaMessage");

            this.ReplicationSchemaMessage = replicationSchemaMessage;
        }

        public IReplicationSchema ReplicationSchema { get; private set; }

        public WcfReplicationSchemaMessage ReplicationSchemaMessage { get; private set; }

        private bool __init_StrongItems;
        private IReplicationSchemaItem[] _StrongItems;
        public IReplicationSchemaItem[] StrongItems
        {
            get
            {
                if (!__init_StrongItems)
                {
                    if (this.ReplicationSchemaMessage != null)
                    {
                        if (this.ReplicationSchemaMessage.StrongItems != null)
                            _StrongItems = this.ReplicationSchemaMessage.StrongItems.Select(smi => new WcfReplicationSchemaItem(smi)).ToArray();
                    }
                    else
                    {
                        if (this.ReplicationSchema.StrongItems != null)
                            _StrongItems = this.ReplicationSchema.StrongItems.Select(si => new WcfReplicationSchemaItem(si)).ToArray();
                    }

                    __init_StrongItems = true;
                }
                return _StrongItems;
            }
        }

        private bool __init_WeakItems;
        private IReplicationSchemaItem[] _WeakItems;
        public IReplicationSchemaItem[] WeakItems
        {
            get
            {
                if (!__init_WeakItems)
                {
                    if (this.ReplicationSchemaMessage != null)
                    {
                        if (this.ReplicationSchemaMessage.WeakItems != null)
                            _WeakItems = this.ReplicationSchemaMessage.WeakItems.Select(smi => new WcfReplicationSchemaItem(smi)).ToArray();
                    }
                    else
                    {
                        if (this.ReplicationSchema.WeakItems != null)
                            _WeakItems = this.ReplicationSchema.WeakItems.Select(si => new WcfReplicationSchemaItem(si)).ToArray();
                    }
                    __init_WeakItems = true;
                }
                return _WeakItems;
            }
        }

        private bool __init_Host;
        private string _Host;
        public string Host
        {
            get
            {
                if (!__init_Host)
                {
                    if (this.ReplicationSchemaMessage != null)
                        _Host = this.ReplicationSchemaMessage.Host;
                    else
                        _Host = this.ReplicationSchema.Host;

                    __init_Host = true;
                }
                return _Host;
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
                    if (this.ReplicationSchemaMessage != null)
                        _StorageID = this.ReplicationSchemaMessage.StorageID;
                    else
                        _StorageID = this.ReplicationSchema.StorageID;

                    __init_StorageID = true;
                }
                return _StorageID;
            }
        }
    }
}