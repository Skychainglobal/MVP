using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Storage.Lib;

namespace Storage.Data.Blob
{
    /// <summary>
    /// Блоб файл. Представляет собой единичный контейнер для хранения данных.
    /// </summary>
    internal class Blob
    {
        private static object _commonLocker = new object();

        public Blob(BlobContainer container, IBlobMetadata metadata)
        {
            if (container == null)
                throw new ArgumentNullException("container");

            if (metadata == null)
                throw new ArgumentNullException("metadata");

            this.Container = container;
            this.Metadata = metadata;
        }

        /// <summary>
        /// Контейнер блобов.
        /// </summary>
        internal BlobContainer Container { get; set; }

        /// <summary>
        /// Метаданные блоба
        /// </summary>
        internal IBlobMetadata Metadata { get; private set; }

        private bool __init_Name;
        private string _Name;
        /// <summary>
        /// Имя блоба.
        /// </summary>
        public string Name
        {
            get
            {
                if (!__init_Name)
                {
                    _Name = this.Metadata.Name;
                    if (string.IsNullOrEmpty(_Name))
                        throw new Exception(string.Format("Для блоба с идентификатором {0} не задано имя.",
                            this.ID));

                    __init_Name = true;
                }
                return _Name;
            }
        }

        private bool __init_ID;
        private int _ID;
        /// <summary>
        /// Идентификатор блоба.
        /// </summary>
        public int ID
        {
            get
            {
                if (!__init_ID)
                {
                    _ID = this.Metadata.ID;
                    if (_ID < 1)
                        throw new Exception(string.Format("Для блоба должен быть задан идентификатор."));

                    __init_ID = true;
                }
                return _ID;
            }
        }

        private bool __init_Path;
        private string _Path;
        /// <summary>
        /// Путь до блоба.
        /// </summary>
        internal string Path
        {
            get
            {
                if (!__init_Path)
                {
                    _Path = string.Format("{0}\\{1}.{2}",
                        this.Container.Directory.FullName,
                        this.Name,
                        BlobConsts.Blobs.Extension);

                    __init_Path = true;
                }
                return _Path;
            }
        }

        private bool __init_File;
        private FileInfo _File;
        /// <summary>
        /// Физический файл на диске.
        /// </summary>
        internal FileInfo File
        {
            get
            {
                if (!__init_File)
                {
                    _File = new FileInfo(this.Path);
                    __init_File = true;
                }
                return _File;
            }
        }

        /// <summary>
        /// Считывает поток данных версии файла из блоба.
        /// </summary>
        /// <param name="versionMetadata">Метаданные версии файла.</param>
        /// <returns></returns>
        internal Stream ReadStream(IBlobFileVersionMetadata versionMetadata)
        {
            if (versionMetadata == null)
                throw new ArgumentNullException("versionMetadata");

            //поток закрывает объект, предоставляющий средства потоковой передачи
            FileStream fs = this.File.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            BlobStreamAdapter streamAdapter = new BlobStreamAdapter(fs);
            PartitionStream stream = streamAdapter.ReadStream(versionMetadata);

            return stream;
        }

        /// <summary>
        /// Считывает поток данных файла из блоба.
        /// </summary>
        /// <param name="fileMetadata">Метаданные файла.</param>
        /// <returns></returns>
        internal Stream ReadStream(IBlobFileMetadata fileMetadata)
        {
            if (fileMetadata == null)
                throw new ArgumentNullException("fileMetadata");

            //поток закрывает объект, предоставляющий средства потоковой передачи
            FileStream fs = this.File.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            BlobStreamAdapter streamAdapter = new BlobStreamAdapter(fs);
            PartitionStream stream = streamAdapter.ReadStream(fileMetadata);

            return stream;
        }

        /// <summary>
        /// Возвращает содержимое файла.
        /// </summary>
        /// <param name="versionMetadata">Метаданные версии файла.</param>
        /// <returns></returns>
        internal byte[] Read(IBlobFileVersionMetadata versionMetadata)
        {
            if (versionMetadata == null)
                throw new ArgumentNullException("versionMetadata");

            if (versionMetadata.BlobStartPosition < 0)
                throw new ArgumentNullException("fileVersionMetadata.BlobStartPosition");

            if (versionMetadata.BlobEndPosition < 1)
                throw new ArgumentNullException("fileVersionMetadata.BlobEndPosition");

            this.Container.DataAdapter.Logger.WriteFormatMessage("Blob.Read:Начало чтения содержимого из блоба, blobStartPosition: {0}, blobEndPosition: {1}", versionMetadata.BlobStartPosition, versionMetadata.BlobEndPosition);

            //для операции чтения блоб должен всегда существовать
            if (!this.File.Exists)
                throw new Exception(string.Format("Не удалось найти блоб по пути {0}",
                    this.File.FullName));

            byte[] content = null;
            using (FileStream fs = this.File.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                BlobStreamAdapter streamAdapter = new BlobStreamAdapter(fs);
                content = streamAdapter.Read(versionMetadata);
            }
            this.Container.DataAdapter.Logger.WriteFormatMessage("Blob.Read:Окончание чтения содержимого из блоба, blobStartPosition: {0}, blobEndPosition: {1}", versionMetadata.BlobStartPosition, versionMetadata.BlobEndPosition);

            return content;
        }

        /// <summary>
        /// Возвращает содержимое файла.
        /// </summary>
        /// <param name="fileMetadata">Метаданные файла.</param>
        /// <returns></returns>
        internal byte[] Read(IBlobFileMetadata fileMetadata)
        {
            if (fileMetadata == null)
                throw new ArgumentNullException("fileMetadata");

            if (fileMetadata.BlobStartPosition < 0)
                throw new ArgumentNullException("fileMetadata.BlobStartPosition");

            if (fileMetadata.BlobEndPosition < 1)
                throw new ArgumentNullException("fileMetadata.BlobEndPosition");

            this.Container.DataAdapter.Logger.WriteFormatMessage("Blob.Read:Начало чтения содержимого из блоба, blobStartPosition: {0}, blobEndPosition: {1}", fileMetadata.BlobStartPosition, fileMetadata.BlobEndPosition);

            //для операции чтения блоб должен всегда существовать
            if (!this.File.Exists)
                throw new Exception(string.Format("Не удалось найти блоб по пути {0}",
                    this.File.FullName));

            byte[] content = null;
            using (FileStream fs = this.File.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                BlobStreamAdapter streamAdapter = new BlobStreamAdapter(fs);
                content = streamAdapter.Read(fileMetadata);
            }
            this.Container.DataAdapter.Logger.WriteFormatMessage("Blob.Read:Окончание чтения содержимого из блоба, blobStartPosition: {0}, blobEndPosition: {1}", fileMetadata.BlobStartPosition, fileMetadata.BlobEndPosition);

            return content;
        }

        /// <summary>
        /// Записывает содержимое файла в блоб.
        /// </summary>
        /// <param name="fileMetadata">Матаданные файла.</param>
        /// <param name="stream">Содержимое файла.</param>
        /// <param name="remoteTimeCreated">Время создания версии с удаленного узла.</param>
        /// <returns></returns>
        internal BlobFileInfo Write(IBlobFileMetadata fileMetadata, Stream stream, DateTime? remoteTimeCreated = null)
        {
            if (fileMetadata == null)
                throw new ArgumentNullException("fileMetadata");

            if (stream == null)
                throw new ArgumentNullException("stream");

            this.Container.DataAdapter.Logger.WriteFormatMessage("Blob.Write:Начало записи содержимого в блоб, file.UniqueID: {0}", fileMetadata.UniqueID);

            BlobFileInfo blobFileInfo = new BlobFileInfo();
            blobFileInfo.BlobID = this.ID;
            bool closeBlob = false;

            DateTime timeCreated;
            if (remoteTimeCreated.HasValue)
                timeCreated = remoteTimeCreated.Value;
            else
            {
                //начиная с 4ой версии хранится время в UTC
                timeCreated = DateTime.Now.ToUniversalTime();
            }

            using (FileStream fs = this.File.Open(FileMode.Append, FileAccess.Write, FileShare.Read))
            {
                BlobStreamAdapter streamAdapter = new BlobStreamAdapter(fs);
                blobFileInfo.BlobStartPosition = fs.Length;
                streamAdapter.Write(fileMetadata, stream, timeCreated);
                blobFileInfo.BlobEndPosition = fs.Length;
                blobFileInfo.TimeCreated = timeCreated;

                closeBlob = fs.Length > this.Container.DataAdapter.MaxBlobSize;
            }

            if (closeBlob)
            {
                this.Metadata.Closed = true;
                this.Container.DataAdapter.BlobMetadataAdapter.SaveBlob(this.Metadata);
            }

            this.Container.DataAdapter.Logger.WriteFormatMessage("Blob.Write:Окончание записи содержимого в блоб, file.UniqueID: {0}. Блоб был закрыт после записи файла: {1}",
                fileMetadata.UniqueID,
                closeBlob);

            return blobFileInfo;
        }

        internal void UpdateIntegrityPosition(long integrityPosition)
        {
            if (integrityPosition < 0)
                throw new ArgumentNullException("integrityPosition");

            if (this.Metadata.IntegrityPosition > integrityPosition)
                throw new ArgumentNullException("integrityPosition должна быть больше существующей позиции.");
            else if (this.Metadata.IntegrityPosition == integrityPosition)
                return;
            else
            {
                object blobLocker = this.Container.GetContainerLocker(ContainerLockerType.Metadata);
                lock (blobLocker)
                {
                    //необходимо обновить позицию целостности блоба
                    this.Metadata.UpdateIntegrityPosition(integrityPosition);
                    this.Container.DataAdapter.BlobMetadataAdapter.SaveBlob(this.Metadata);
                }
            }
        }
    }
}