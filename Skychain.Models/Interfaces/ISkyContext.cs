using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Skychain.Models
{
    /// <summary>
    /// Представляет контекст работы с системой.
    /// Экземпляр класса не является потокобезопасным, как и все экземпляры, которые могут быть получены через данный экземпляр.
    /// </summary>
    public interface ISkyContext
    {
        /// <summary>
        /// Содержит адаптеры всех типов объектов системы.
        /// </summary>
        ISkyObjectAdapterRepository ObjectAdapters { get; }

        /// <summary>
        /// Возвращает новый или существующий экземпляр пользователя.
        /// Метод выполняет сопоставление между идентификатором и экземпляром, соответствующим данному идентификатору,
        /// какого-либо поиска пользователя по идентификатору не производится.
        /// </summary>
        /// <param name="userID">Идентификатор пользователя, для которого производится сопоставление.</param>
        ISkyUser GetUser(Guid userID);
    }
}