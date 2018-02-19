using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Storage.Engine
{
    /// <summary>
    /// Адаптер файловых токенов.
    /// </summary>
    public interface IFileTokenAdapter
    {
        /// <summary>
        /// Адаптер метаданных хранилища.
        /// </summary>
        IMetadataAdapter MetadataAdapter { get; }

        /// <summary>        
        /// Создает токен для конкретной версии файла.
        /// </summary>
        /// <param name="fileVersionMetadata">Метаданные версии файла.</param>        
        /// <param name="securityIdentifier">Идентификатор безопасности токена.</param>
        /// <returns></returns>
        IFileToken GenerateFileToken(IFileVersionMetadata fileVersionMetadata, string securityIdentifier);

        /// <summary>
        /// Возвращает действующий версии файла.
        /// </summary>
        /// <param name="fileVersionMetadata">Метаданные версии файла.</param>
        /// <param name="tokenUniqueID">Уникальный идентификатор токена.</param>
        /// <returns>Действующий версии файла, либо null, если токен отсутствует или его срок действия истек.</returns>
        IFileToken GetToken(IFileVersionMetadata fileVersionMetadata, Guid tokenUniqueID);
    }
}