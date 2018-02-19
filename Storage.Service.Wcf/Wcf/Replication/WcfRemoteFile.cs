using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using Storage.Engine;
using Storage.Lib;

namespace Storage.Service.Wcf
{
    /// <summary>
    /// Реплицируемый файл.
    /// </summary>
    [MessageContract]
    public class WcfRemoteFile : IRemoteFile
    {
        /// <summary>
        /// Название.
        /// </summary>
        [MessageHeader(MustUnderstand = true)]
        public string Name { get; private set; }

        /// <summary>
        /// ID файла.
        /// </summary>
        [MessageHeader(MustUnderstand = true)]
        public Guid UniqueID { get; private set; }

        /// <summary>
        /// ID версии.
        /// </summary>
        [MessageHeader(MustUnderstand = true)]
        public Guid VersionID { get; private set; }

        /// <summary>
        /// Дата создания версии.
        /// </summary>
        [MessageHeader(MustUnderstand = true)]
        public DateTime TimeCreated { get; private set; }

        /// <summary>
        /// Метаданные узла, на котором была создана версия удаленного файла.
        /// </summary>
        public IStorageMetadata CreatedStorageNode { get; set; }

        /// <summary>
        /// Поток для чтения содержимого файла.
        /// </summary>
        [MessageBodyMember]
        public Stream Stream { get; private set; }

        /// <summary>
        /// Создаёт объект с реплицируемым файлов по версии файла.
        /// </summary>
        /// <param name="version"></param>
        /// <returns></returns>
        public static WcfRemoteFile FromFileVersion(IFileVersion version)
        {
            if (version == null)
                throw new ArgumentNullException("version");

            return new WcfRemoteFile
            {
                Name = version.Name,
                Stream = version.Open(),
                TimeCreated = version.TimeCreated,
                UniqueID = version.File.UniqueID,
                VersionID = version.UniqueID
            };
        }
    }
}