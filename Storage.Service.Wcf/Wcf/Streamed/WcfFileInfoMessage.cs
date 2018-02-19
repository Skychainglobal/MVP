using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using Storage.Lib;

namespace Storage.Service.Wcf
{
    [MessageContract]
    public class WcfFileInfoMessage
    {
        /// <summary>
        /// Уникальный идентификатор файла.
        /// </summary>
        [MessageHeader(MustUnderstand = true)]
        public Guid UniqueID { get; set; }

        /// <summary>
        /// Уникальный идентификатор папки, в которой находится файл.
        /// </summary>
        [MessageHeader(MustUnderstand = true)]
        public Guid FolderUniqueID { get; set; }

        /// <summary>
        /// Уникальный идентификатор версии файла.
        /// </summary>
        [MessageHeader(MustUnderstand = true)]
        public Guid VersionUniqueID { get; set; }

        /// <summary>
        /// Имя файла.
        /// </summary>
        [MessageHeader(MustUnderstand = true)]
        public string Name { get; set; }

        /// <summary>
        /// Дата создания.
        /// </summary>
        [MessageHeader(MustUnderstand = true)]
        public DateTime TimeCreated { get; set; }

        /// <summary>
        /// Дата изменения.
        /// </summary>
        [MessageHeader(MustUnderstand = true)]
        public DateTime TimeModified { get; set; }

        /// <summary>
        /// Адрес папки файла.
        /// </summary>
        [MessageHeader(MustUnderstand = true)]
        public string FolderUrl { get; set; }

        /// <summary>
        /// Адрес файла.
        /// </summary>
        [MessageHeader(MustUnderstand = true)]
        public string Url { get; set; }

        /// <summary>
        /// Размер файла.
        /// </summary>
        [MessageHeader(MustUnderstand = true)]
        public long Size { get; set; }

        /// <summary>
        /// Возвращает транспортный объект файла для передачи клиенту.
        /// </summary>
        /// <param name="file">Файл.</param>
        /// <param name="loadOptions">Опции загрузки.</param>
        /// <returns></returns>
        public static WcfFileInfoMessage FromFile(IFile file)
        {
            if (file == null)
                throw new ArgumentNullException("file");

            WcfFileInfoMessage wcfFile = new WcfFileInfoMessage()
            {
                UniqueID = file.UniqueID,
                FolderUniqueID = file.Folder.UniqueID,
                VersionUniqueID = file.VersionUniqueID,
                Name = file.Name,
                TimeCreated = file.TimeCreated,
                TimeModified = file.TimeModified,
                FolderUrl = file.FolderUrl,
                Url = file.Url,
                Size = file.Size
            };

            return wcfFile;
        }
    }
}