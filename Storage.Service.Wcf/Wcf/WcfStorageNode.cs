using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Storage.Lib;

namespace Storage.Service.Wcf
{
    internal class WcfStorageNode : IStorageNode
    {
        public WcfStorageNode(string host, Guid storageID)
        {
            if (string.IsNullOrEmpty(host))
                throw new ArgumentNullException("host");

            if (storageID == Guid.Empty)
                throw new ArgumentNullException("storageID");

            this.Host = host;
            this.UniqueID = storageID;
        }

        public string Host { get; private set; }

        public Guid UniqueID { get; private set; }

        public bool IsCurrent
        {
            get { return false; }
        }
    }
}