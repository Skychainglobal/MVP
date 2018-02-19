using System;
using System.Collections.Generic;
using System.IO;
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
    internal class WcfFileInfo
    {
        /// <summary>
        /// Уникальный идентификатор файла.
        /// </summary>
        [DataMember]
        public Guid UniqueID { get; set; }

        /// <summary>
        /// Уникальный идентификатор папки, в которой находится файл.
        /// </summary>
        [DataMember]
        public Guid FolderUniqueID { get; set; }

        /// <summary>
        /// Уникальный идентификатор версии файла.
        /// </summary>
        [DataMember]
        public Guid VersionUniqueID { get; set; }

        /// <summary>
        /// Имя файла.
        /// </summary>
        [DataMember]
        public string Name { get; set; }

        /// <summary>
        /// Дата создания.
        /// </summary>
        [DataMember]
        public DateTime TimeCreated { get; set; }

        /// <summary>
        /// Дата изменения.
        /// </summary>
        [DataMember]
        public DateTime TimeModified { get; set; }

        /// <summary>
        /// Адрес папки файла.
        /// </summary>
        [DataMember]
        public string FolderUrl { get; set; }

        /// <summary>
        /// Адрес файла.
        /// </summary>
        [DataMember]
        public string Url { get; set; }

        /// <summary>
        /// Содержимое файла.
        /// </summary>
        [DataMember]
        public byte[] Content { get; set; }

        /// <summary>
        /// Размер файла.
        /// </summary>
        [DataMember]
        public long Size { get; set; }

        /// <summary>
        /// Возвращает транспортный объект файла для передачи клиенту.
        /// </summary>
        /// <param name="file">Файл.</param>
        /// <param name="loadOptions">Опции загрузки.</param>
        /// <returns></returns>
        public static WcfFileInfo FromFile(IFile file, GetFileOptions loadOptions = null)
        {
            if (file == null)
                throw new ArgumentNullException("file");

            WcfFileInfo wcfFile = new WcfFileInfo()
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

            if (loadOptions != null && loadOptions.LoadContent)
            {
                if (file.Content != null)
                    wcfFile.Content = file.Content;
                else
                {
                    //в буферном режиме не может быть тастолько большим
                    //проверки на размер есть в вызывающем коде, до физического чтения содержимого
                    int bufferedFileSize = (int)file.Size;

                    wcfFile.Content = new byte[bufferedFileSize];
                    using (Stream st = file.Open())
                    {
                        int read = st.Read(wcfFile.Content, 0, bufferedFileSize);
                        if (read != file.Size)
                            throw new Exception(string.Format("Не удалось полностью считать содержимое файла."));
                    }
                }
            }

            return wcfFile;
        }
    }
}