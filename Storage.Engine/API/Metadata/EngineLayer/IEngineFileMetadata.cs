using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Storage.Engine
{
    public interface IEngineFileMetadata : IEngineObjectMetadata
    {
        /// <summary>
        /// Локальный идентификатор.
        /// </summary>
        int FolderID { get; }

        /// <summary>
        /// Локальный идентификатор пользователя, который последним изменил файл.
        /// </summary>
        int ModifiedUserID { get; }
    }
}