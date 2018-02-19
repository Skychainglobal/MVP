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
    /// Транспортный объект папки. Передается на клиент через Wcf службу.
    /// </summary>
    [DataContract]
    internal class WcfFolderInfo
    {
        /// <summary>
        /// Уникальный идентификатор папки.
        /// </summary>
        [DataMember]
        public Guid UniqueID { get; set; }

        /// <summary>
        /// Адрес папки.
        /// </summary>
        [DataMember]
        public string Url { get; set; }

        /// <summary>
        /// Имя папки.
        /// </summary>
        [DataMember]
        public string Name { get; set; }

        /// <summary>
        /// Адрес родительской папки.
        /// </summary>
        [DataMember]
        public string ParentFolderUrl { get; set; }

        /// <summary>
        /// Возвращает транспортный объект папки для передачи клиенту.
        /// </summary>
        /// <param name="folder">Папка.</param>
        /// <returns></returns>
        public static WcfFolderInfo FromFolder(IFolder folder)
        {
            if (folder == null)
                throw new ArgumentNullException("folder");

            string parentUrl = null;
            if (folder.ParentFolder != null)
                parentUrl = folder.ParentFolder.Url;

            WcfFolderInfo wcfFolder = new WcfFolderInfo()
            {
                UniqueID = folder.UniqueID,
                Url = folder.Url,
                Name = folder.Name,
                ParentFolderUrl = parentUrl
            };

            return wcfFolder;
        }
    }
}