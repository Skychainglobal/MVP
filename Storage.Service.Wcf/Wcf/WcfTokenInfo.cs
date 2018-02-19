using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Storage.Engine;

namespace Storage.Service.Wcf
{
    /// <summary>
    /// Класс, представляющий информацию о токене доступа для операции с файловым хранилищем.
    /// </summary>
    [DataContract]
    public class WcfTokenInfo
    {
        /// <summary>
        /// Уникальный идентификатор токена доступа.
        /// </summary>
        [DataMember]
        public Guid UniqueID { get; set; }
    }
}