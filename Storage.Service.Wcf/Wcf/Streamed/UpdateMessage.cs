using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Storage.Service.Wcf
{
    /// <summary>
    /// Сообщение для обновления файла.
    /// </summary>
    [MessageContract]
    public class UpdateMessage
    {
        /// <summary>
        /// Адрес папки файла.
        /// </summary>
        [MessageHeader(MustUnderstand = true)]
        public string FolderUrl;

        /// <summary>
        /// Уникальный идентификатор файла.
        /// </summary>
        [MessageHeader(MustUnderstand = true)]
        public Guid FileUniqueID;

        /// <summary>
        /// Новое имя файла.
        /// </summary>
        [MessageHeader(MustUnderstand = true)]
        public string FileName;

        /// <summary>
        /// Токен доступа для потоковой загрузки.
        /// </summary>
        [MessageHeader(MustUnderstand = true)]
        public Guid AccessToken;

        /// <summary>
        /// Содержимое файла.
        /// </summary>
        [MessageBodyMember]
        public Stream FileStream;
    }
}
