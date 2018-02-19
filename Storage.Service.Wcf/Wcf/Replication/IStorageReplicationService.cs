using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Storage.Service.Wcf
{
    /// <summary>
    /// Сервис для репликации данных файлового хранилища.
    /// </summary>
    [ServiceContract(Namespace = "Storage.Service.IStorageReplicationService")]
    interface IStorageReplicationService
    {
        /// <summary>
        /// Обмен схемой репликации текущего узла.
        /// </summary>
        /// <param name="currentSchema"></param>
        /// <returns></returns>
        [OperationContract]
        WcfReplicationSchemaMessage ExchangeSchema(WcfReplicationSchemaMessage currentSchema);

        /// <summary>
        /// Возвращает информацию по узлу файлового хранилища.
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        WcfStorageInfo GetStorageInfo();

        /// <summary>
        /// Возвращает идентификаторы версий файлов для репликации.
        /// </summary>
        /// <param name="requestStorageID">Идентификатор узла, запрашивающего файлы для репликации.</param>
        /// <param name="folderUrl">Адрес папки.</param>
        /// <param name="from">Дата, с которой необходимо забрать файлы с узла источника.</param>
        /// <returns></returns>
        [OperationContract]
        Tuple<Guid, Guid>[] GetReplicationFiles(Guid requestStorageID, string folderUrl, DateTime from);

        /// <summary>
        /// Возвращает файл для репликации.
        /// </summary>
        /// <param name="fileInfo">Информация по запрашиваемому файлу.</param>
        /// <returns>Объект с реплицируемым файлом.</returns>
        [OperationContract]
        WcfRemoteFile GetReplicationFile(WcfRemoteFileInfo fileInfo);
    }
}