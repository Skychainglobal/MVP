using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Storage.Lib;

namespace Storage.Engine
{
    /// <summary>
    /// Интерфейс для транспорта транзакций репликации.
    /// </summary>
    public interface IReplicationTransport : IDisposable
    {
        /// <summary>
        /// Обмен схем репликации текущего узла с узлом storageHost.
        /// </summary>
        /// <param name="remoteNode">Удаленный узел хранилища.</param>
        /// <param name="currentReplicationSchema">Схема репликации текущего узла.</param>
        /// <returns></returns>
        IReplicationSchema ExchangeSchema(IStorageNode remoteNode, IReplicationSchema currentReplicationSchema);

        /// <summary>
        /// Возвращает информацию по узлу файлового хранилища.
        /// </summary>
        /// <param name="remoteHost">Хост удаленного узла.</param>
        /// <returns></returns>
        IStorageNode GetStorageInfo(string remoteHost);

        /// <summary>
        /// Возвращает набор идентификаторов версий файлов с удаленного узла.
        /// </summary>
        /// <param name="remoteNode">Удаленный узел хранилища.</param>
        /// <param name="requestStorageID">Идентификатор узла, запрашиваюшего информацию (текущий узел)</param>
        /// <param name="folderUrl">Адрес папки.</param>
        /// <param name="from">Дата, с которой необходимо забрать изменения.</param>
        /// <returns></returns>
        Tuple<Guid, Guid>[] GetReplicationFiles(IStorageNode remoteNode, Guid requestStorageID, string folderUrl, DateTime from);

        /// <summary>
        /// Возвращает версию файла для репликации.
        /// </summary>
        /// <param name="remoteNode">Удаленный узел хранилища.</param>
        /// <param name="folderUrl">Папка файла.</param>
        /// <param name="fileUniqueID">Идентификатор файла.</param>
        /// <param name="fileVersionUniqueID">Идентификатор версии.</param>
        /// <returns></returns>
        IRemoteFile GetReplicationFile(IStorageNode remoteNode, string folderUrl, Guid fileUniqueID, Guid fileVersionUniqueID);
    }
}