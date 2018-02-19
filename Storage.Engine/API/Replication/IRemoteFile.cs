using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Storage.Lib;

namespace Storage.Engine
{
    public interface IRemoteFile
    {
        Stream Stream { get; }
        string Name { get; }
        Guid UniqueID { get; }
        Guid VersionID { get; }
        DateTime TimeCreated { get; }

        /// <summary>
        /// Узел, на котором был создана версия файла.
        /// </summary>
        IStorageMetadata CreatedStorageNode { get; set; }
    }
}