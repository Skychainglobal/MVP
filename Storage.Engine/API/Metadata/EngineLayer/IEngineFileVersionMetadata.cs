using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Storage.Engine
{
    public interface IEngineFileVersionMetadata : IEngineObjectMetadata
    {
        /// <summary>
        /// Локальный идентификатор хранилища, на котором создана данная версия.
        /// </summary>
        int CreatedStorageID { get; }

        /// <summary>
        /// Локальный идентификатор папки файла.
        /// </summary>
        int FolderID { get; }

        /// <summary>
        /// Локальный идентификатор файла.
        /// </summary>
        int FileID { get; }

        /// <summary>
        /// Локальный идентификатор пользователя, который создал данную версию.
        /// </summary>
        int ModifiedUserID { get; }
    }
}