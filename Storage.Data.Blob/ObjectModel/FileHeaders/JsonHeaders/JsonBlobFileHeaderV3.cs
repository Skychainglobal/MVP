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
    internal class JsonBlobFileHeaderV1 : IFileHeader, IBlobFileHeader
    {
        /// <summary>
        /// Начальная позиция системного заголовка файла в блобе.
        /// </summary>
        [DataMember]
        public long ContentAbsoluteStartPosition { get; set; }

        /// <summary>
        /// Длина содержимого (без заголовков).
        /// </summary>
        [DataMember]
        public long ContentLength { get; set; }

        /// <summary>
        /// Имя файла.
        /// </summary>
        [DataMember]
        public string FileName { get; set; }

        /// <summary>
        /// Адрес папки файла.
        /// </summary>
        [DataMember]
        public string FolderUrl { get; set; }

        /// <summary>
        /// Уникальный идентификатор файла.
        /// </summary>
        [DataMember]
        public Guid UniqueID { get; set; }

        /// <summary>
        /// Длина заголовка. Не хранится в самом заголовке, потому что заголовок не может сам знать о своей длине в байтах.
        /// Заполняется после поднятия заголовка из потока.
        /// </summary>
        [IgnoreDataMember]
        public int HeaderLength { get; set; }

        /// <summary>
        /// Дата создания файла.
        /// </summary>
        [DataMember]
        public DateTime TimeCreated { get; set; }

        /// <summary>
        /// Идентификатор версии данного файла.
        /// </summary>
        [DataMember]
        public Guid VersionUniqueID { get; set; }

        /// <summary>
        /// Идентификатор пользователя, изменившего файл.
        /// </summary>
        [DataMember]
        public int ModifiedUserID { get; set; }
    }
}