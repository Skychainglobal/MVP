using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Storage.Lib;

namespace Storage.Service.Wcf
{
    /// <summary>
    /// Транспортный объект файлового хранилища. Передается на клиент через Wcf службу.
    /// </summary>
    [DataContract]
    internal class WcfStorageInfo
    {
        public WcfStorageInfo(IStorageNode node)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            this.Node = node;
        }

        public IStorageNode Node { get; private set; }

        private bool __init_UniqueID;
        private Guid _UniqueID;
        /// <summary>
        /// Уникальный идентификатор папки.
        /// </summary>
        [DataMember]
        public Guid UniqueID
        {
            get
            {
                if (!__init_UniqueID)
                {
                    _UniqueID = this.Node.UniqueID;
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
    }
}