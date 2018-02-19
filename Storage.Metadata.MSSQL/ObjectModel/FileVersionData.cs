using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Storage.Metadata.MSSQL
{
    internal class FileVersionData
    {
        public FileVersionData(Guid fileID, Guid versionID, DateTime versionTime)
        {
            this.FileID = fileID;
            this.VersionID = versionID;
            this.VersionTime = versionTime;
        }

        internal Guid FileID { get; private set; }
        internal Guid VersionID { get; private set; }
        internal DateTime VersionTime { get; private set; }
    }
}