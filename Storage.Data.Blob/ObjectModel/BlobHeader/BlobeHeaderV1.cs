using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Storage.Data.Blob
{
    /// <summary>
    /// Заголовок файла в формате Json.
    /// </summary>
    [DataContract]
    internal class BlobeHeaderV1
    {
        /// <summary>
        /// Локальный идентификатор блоба.
        /// </summary>
        [DataMember]
        public int ID { get; set; }

        /// <summary>
        /// Локальный идентификатор контейнера.
        /// </summary>
        [DataMember]
        public int ContainerID { get; set; }

        /// <summary>
        /// Дата создания блоба.
        /// </summary>
        [DataMember]
        public DateTime TimeCreated { get; set; }

        /// <summary>
        /// Имя файла блоба.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Полное имя файла блоба.
        /// </summary>
        [DataMember]
        public string FullName { get; set; }

        /// <summary>
        /// Имя машины, на которой был создан блоб.
        /// </summary>
        [DataMember]
        public string MachineName { get; set; }
    }
}