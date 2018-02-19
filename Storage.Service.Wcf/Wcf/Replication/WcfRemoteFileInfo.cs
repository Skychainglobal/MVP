using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using Storage.Engine;

namespace Storage.Service.Wcf
{
    /// <summary>
    /// Информация о реплицируемом файле.
    /// </summary>
    [MessageContract]
    public class WcfRemoteFileInfo
    {
        /// <summary>
        /// ID файла.
        /// </summary>
        [MessageHeader(MustUnderstand = true)]
        public Guid FileID { get; set; }

        /// <summary>
        /// ID версии.
        /// </summary>
        [MessageHeader(MustUnderstand = true)]
        public Guid VersionID { get; set; }

        /// <summary>
        /// Url папки.
        /// </summary>
        [MessageHeader(MustUnderstand = true)]
        public string FolderUrl { get; set; }

        public static WcfRemoteFileInfo FromRemoteFileInfo(string folderUrl, Guid fileUniqueID, Guid fileVersionUniqueID)
        {
            return new WcfRemoteFileInfo
            {
                FileID = fileUniqueID,
                VersionID = fileVersionUniqueID,
                FolderUrl = folderUrl
            };
        }
    }
}
