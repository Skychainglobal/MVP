using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Storage.Engine
{
    /// <summary>
    /// Файловый токен.
    /// </summary>
    internal class SessionLinkToken : IFileToken
    {
        public SessionLinkToken(Guid uniqueID, Guid fileUniqueID, Guid versionUniqueID, string folderUrl)
        {
            if (uniqueID == Guid.Empty)
                throw new ArgumentNullException("uniqueID");

            if (fileUniqueID == Guid.Empty)
                throw new ArgumentNullException("fileUniqueID");

            if (versionUniqueID == Guid.Empty)
                throw new ArgumentNullException("versionUniqueID");

            if (string.IsNullOrEmpty(folderUrl))
                throw new ArgumentNullException("folderUrl");

            this.UniqueID = uniqueID;
            this.FileUniqueID = fileUniqueID;
            this.VersionUniqueID = versionUniqueID;
            this.FolderUrl = folderUrl;
        }

        /// <summary>
        /// Уникальный идентификатор токена.
        /// </summary>
        public Guid UniqueID { get; private set; }

        /// <summary>
        /// Уникальный идентификатор файла.
        /// </summary>
        public Guid FileUniqueID { get; private set; }

        /// <summary>
        /// Уникальный идентификатор версии файла.
        /// </summary>
        public Guid VersionUniqueID { get; private set; }

        /// <summary>
        /// Адрес папки.
        /// </summary>
        public string FolderUrl { get; set; }

        /// <summary>
        /// Дата експирации токена.
        /// </summary>
        public DateTime Expired
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Идентификатор безопасности токена.
        /// </summary>
        public string SecurityIdentifier
        {
            get { throw new NotImplementedException(); }
        }
    }
}