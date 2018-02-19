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
    /// Транспортный объект файла. Передается на клиент через Wcf службу.
    /// </summary>
    [DataContract]
    internal class WcfFileVersionInfo
    {
        /// <summary>
        /// Уникальный идентификатор версии файла.
        /// </summary>
        [DataMember]
        public Guid UniqueID { get; set; }

        /// <summary>
        /// Имя версии файла.
        /// </summary>
        [DataMember]
        public string Name { get; set; }

        /// <summary>
        /// Дата создания версии.
        /// </summary>
        [DataMember]
        public DateTime TimeCreated { get; set; }

        /// <summary>
        /// Размер файла версии.
        /// </summary>
        [DataMember]
        public long Size { get; set; }

        /// <summary>
        /// Содержимое версии файла.
        /// </summary>
        [DataMember]
        public byte[] Content { get; set; }

        /// <summary>
        /// Идентификатор хранилища, на котором была создана версия.
        /// </summary>
        [DataMember]
        public Guid CreatedStorageID { get; set; }

        /// <summary>
        /// Возвращает транспортный объект файла для передачи клиенту.
        /// </summary>
        /// <param name="file">Файл.</param>
        /// <param name="loadOptions">Опции загрузки.</param>
        /// <returns></returns>
        public static WcfFileVersionInfo FromFileVersion(IFileVersion fileVersion, GetFileOptions loadOptions = null)
        {
            if (fileVersion == null)
                throw new ArgumentNullException("fileVersion");

            WcfFileVersionInfo wcfFile = new WcfFileVersionInfo()
            {
                UniqueID = fileVersion.UniqueID,
                Name = fileVersion.Name,
                TimeCreated = fileVersion.TimeCreated,
                Size = fileVersion.Size,
                CreatedStorageID = fileVersion.CreatedStorageID
            };

            if (loadOptions != null && loadOptions.LoadContent)
                wcfFile.Content = fileVersion.Content;

            return wcfFile;
        }
    }
}