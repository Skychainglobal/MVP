using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Storage.Lib;

namespace Storage.Engine
{
    /// <summary>
    /// Адаптер репликаций.
    /// </summary>
    public interface IReplicationAdapter : IDisposable
    {
        IStorageNode CurrentNode { get; }

        /// <summary>
        /// Реализаця транспорта для транзакций.
        /// </summary>
        IReplicationTransport Transport { get; }

        /// <summary>
        /// Обновляет знания о существующей схеме репликации.
        /// </summary>
        /// <param name="remoteSchema">Схема репликации узла, с которого пришел запрос.</param>
        IReplicationSchema UpdateReplicationSchema(IReplicationSchema remoteSchema);

        /// <summary>
        /// Возвращает идентификаторы версий файлов для репликации.
        /// </summary>
        /// <param name="requestStorageID">Идентификатор узла, запрашивающего файлы для репликации.</param>
        /// <param name="folderUrl">Адрес папки.</param>
        /// <param name="from">Дата, с которой необходимо забрать файлы с узла источника.</param>
        /// <returns></returns>
        Tuple<Guid, Guid>[] GetReplicationFiles(Guid requestStorageID, string folderUrl, DateTime from);
    }
}