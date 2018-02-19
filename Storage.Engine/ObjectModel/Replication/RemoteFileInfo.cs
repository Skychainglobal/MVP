using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Storage.Lib;

namespace Storage.Engine
{
    internal class RemoteFileInfo
    {
        public RemoteFileInfo(Tuple<Guid, Guid> fileVersionInfo, IFolder folder)
        {
            if (fileVersionInfo == null)
                throw new ArgumentNullException("fileVersionInfo");

            if (folder == null)
                throw new ArgumentNullException("folder");

            this.UniqueID = fileVersionInfo.Item1;
            this.VersionID = fileVersionInfo.Item2;
            this.Folder = folder;
        }

        /// <summary>
        /// Идентификатор файла.
        /// </summary>
        public Guid UniqueID { get; private set; }

        /// <summary>
        /// Идентификатор версии файла.
        /// </summary>
        public Guid VersionID { get; private set; }

        /// <summary>
        /// Папка файла.
        /// </summary>
        public IFolder Folder { get; set; }
    }
}
