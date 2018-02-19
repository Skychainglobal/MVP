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
    /// Интерфейс службы файлового хранилища для потоковой передачи.
    /// </summary>
    [ServiceContract(Namespace = "Storage.Service.IStorageServiceStreamed")]
    internal interface IStorageServiceStreamed
    {
        [OperationContract]
        WcfFileInfoMessage UploadFile(UploadMessage uploadMessage);

        [OperationContract]
        WcfFileInfoMessage UpdateFile(UpdateMessage uploadMessage);

        [OperationContract]
        Stream OpenFile(string folderUrl, Guid fileUniqueID, Guid accessToken);

        [OperationContract]
        Stream OpenFileVersion(string folderUrl, Guid fileUniqueID, Guid versionUniqueID, Guid accessToken);
    }
}