using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Storage.Data.Blob;
using Storage.Metadata.MSSQL;
using Storage.Engine;

namespace Storage.Metadata.Blob.MSSQL
{
    /// <summary>
    /// Представляет DB-коллекцию метаданных версий файла.
    /// </summary>
    internal class FileVersionsCollection : DBCollection<IBlobFileVersionMetadata>
    {
        /// <summary>
        /// К-тор.
        /// </summary>
        /// <param name="file">Метаданные файла.</param>
        internal FileVersionsCollection(FileMetadata file)
            : base()
        {
            if (file == null)
                throw new ArgumentNullException("file");

            this.File = file;
        }

        /// <summary>
        /// К-тор.
        /// </summary>
        /// <param name="file">Метаданные файла.</param>
        /// <param name="versions">Метаданные версий файла.</param>
        internal FileVersionsCollection(FileMetadata file, IEnumerable<FileVersionMetadata> versions)
            : base(versions)
        {
            if (file == null)
                throw new ArgumentNullException("file");

            if (versions == null)
                throw new ArgumentNullException("versions");

            this.File = file;
            this.Versions = versions.ToDictionary(x => x.UniqueID);
        }

        private FileMetadata _File;
        /// <summary>
        /// Метаданные файла.
        /// </summary>
        internal FileMetadata File
        {
            get { return _File; }
            private set { _File = value; }
        }

        private bool __init_Versions = false;
        private Dictionary<Guid, FileVersionMetadata> _Versions;
        /// <summary>
        /// Словарь версий файла.
        /// Ключ - уникальный идентификатор версии. Значение - метаданные версии.
        /// </summary>
        internal Dictionary<Guid, FileVersionMetadata> Versions
        {
            get
            {
                if (!__init_Versions)
                {
                    _Versions = new Dictionary<Guid, FileVersionMetadata>();
                    __init_Versions = true;
                }
                return _Versions;
            }
            private set
            {
                _Versions = value;
                __init_Versions = true;
            }
        }

        private bool __init_VersionsStorages = false;
        private Dictionary<int, IStorageMetadata> _VersionsStorages;
        /// <summary>
        /// Словарь объектов хранилища в которых были созданы версии.
        /// Ключ - идентификатор версии, значение - метаданные хранилища.
        /// </summary>
        internal Dictionary<int, IStorageMetadata> VersionsStorages
        {
            get
            {
                if (!__init_VersionsStorages)
                {
                    List<int> itemsIDs = this.Versions.Values.Select(x => x.CreatedStorageID).ToList();
                    ICollection<IStorageMetadata> stotages = this.File.Adapter.MetadataAdapter.GetStorages(itemsIDs);

                    _VersionsStorages = stotages.ToDictionary(x => x.ID);
                    __init_VersionsStorages = true;
                }
                return _VersionsStorages;
            }
        }

        /// <summary>
        /// Добавление новой версии в коллекцию.
        /// </summary>
        /// <param name="version">Метаданные версии.</param>
        internal void AddVersion(FileVersionMetadata version)
        {
            if (version == null)
                throw new ArgumentNullException("version");

            if (!this.Versions.ContainsKey(version.UniqueID))
            {
                this.Add(version);
                this.Versions.Add(version.UniqueID, version);

                this.__init_VersionsStorages = false;
            }
        }
    }
}
