using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Storage.Engine;

namespace Storage.Data.Blob
{
    /// <summary>
    /// Адаптер восстановления метаданных по данным.
    /// </summary>
    internal class RestoreBlobMetadataAdapter
    {
        public RestoreBlobMetadataAdapter(BlobDataAdapter dataAdapter)
        {
            if (dataAdapter == null)
                throw new ArgumentNullException("dataAdapter");

            this.DataAdapter = dataAdapter;
        }

        /// <summary>
        /// Адаптер данных.
        /// </summary>
        public BlobDataAdapter DataAdapter { get; private set; }

        private bool __init_Containers;
        private ICollection<IBlobContainerMetadata> _Containers;
        /// <summary>
        /// Существующие контейнеры.
        /// </summary>
        internal ICollection<IBlobContainerMetadata> Containers
        {
            get
            {
                if (!__init_Containers)
                {
                    _Containers = this.DataAdapter.BlobMetadataAdapter.GetBlobContainers();
                    __init_Containers = true;
                }
                return _Containers;
            }
        }

        public void Restore()
        {
            //проверяем поочередно каждый блоб каждого контейнера
            foreach (IBlobContainerMetadata containerMetadata in this.Containers)
            {
                try
                {
                    //получаем все блобы контейнера
                    ICollection<IBlobMetadata> blobs = this.DataAdapter.BlobMetadataAdapter.GetBlobs(containerMetadata.ID);
                    if (blobs == null || blobs.Count == 0)
                        continue;

                    BlobContainer container = new BlobContainer(this.DataAdapter, containerMetadata);
                    //Алгоритм восстановления метаданных
                    //1 - получаем все блобы всех контейнеров
                    //нужно посмотреть все блобы и закрытие и открытые. 
                    //потому что после запуска процедуры восстановления метаданных может пойти запись в не до конца восстановленный блоб
                    //и он станет закрытым, но восстановление не завершилось.

                    /*foreach (IBlobMetadata blobMetadata in blobs)
                    {
                        object tmp = blobMetadata.Closed;
                    }*/

                    //2 - для каждого блоба смотрим его позицию, до которой мы проверили целостность этого блоба
                    foreach (IBlobMetadata blobMetadata in blobs)
                    {
                        Blob blob = new Blob(container, blobMetadata);
                        try
                        {
                            long blobIntegrityPosition = blobMetadata.IntegrityPosition;
                            long filelenth = blob.File.Length;

                            //целостность метаданных файлов блоба до конца проверена
                            while (blobIntegrityPosition < filelenth)
                            {
                                using (FileStream fs = blob.File.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                                {
                                    BlobStreamAdapter streamAdapter = new BlobStreamAdapter(fs);
                                    if (blobIntegrityPosition == 0)
                                    {
                                        //блоб может начинаться с системного заголовка.
                                        //а может с файла
                                        //тогда нужно сместить начальную позицию на этот системный заголовок
                                        //читаем позицию первого файла
                                        long firstFilePosition = streamAdapter.GetFirstFilePosition();
                                        blob.UpdateIntegrityPosition(firstFilePosition);
                                        blobIntegrityPosition = blobMetadata.IntegrityPosition;
                                    }


                                    //заголовок из файла
                                    object fileHeaderObj = streamAdapter.GetFileHeader(blobIntegrityPosition);
                                    IFileHeader fileHeader = (IFileHeader)fileHeaderObj;
                                    IBlobFileHeader blobFileHeader = (IBlobFileHeader)fileHeaderObj;

                                    long fileStartPosition = blobMetadata.IntegrityPosition;
                                    if (fileStartPosition != blobFileHeader.ContentAbsoluteStartPosition)
                                        throw new Exception(string.Format("Позиция целостности блоба не соответствует позиции начала файла"));

                                    long fileEndPosition = fileStartPosition
                                        + BlobStreamAdapter.SystemHeaderLength
                                        + blobFileHeader.HeaderLength
                                        + blobFileHeader.ContentLength;

                                    //восстанавливаем метаданные, если их нет
                                    this.EnsureMetadata(blobMetadata, fileHeader, fileStartPosition, fileEndPosition);

                                    //увеличиваем позицию целостности метаданных блоба
                                    blob.UpdateIntegrityPosition(fileEndPosition);
                                    blobIntegrityPosition = blobMetadata.IntegrityPosition;
                                }
                            }
                        }
                        catch (Exception blobEx)
                        {
                            this.DataAdapter.Logger.WriteFormatMessage("Ошибка при восстановлении метаданных блоба с идентификатором {0} для контейнера {1}. Текст ошибки: {2}",
                               blob.ID,
                               containerMetadata.Path,
                               blobEx);
                        }
                    }

                }
                catch (Exception containerEx)
                {
                    this.DataAdapter.Logger.WriteFormatMessage("Ошибка при восстановлении метаданных контейнера {0}. Текст ошибки: {1}",
                        containerMetadata.Path,
                        containerEx);
                }
            }
        }

        private void EnsureMetadata(IBlobMetadata blobMetadata, IFileHeader fileHeader, long fileAbsoluteStartPosition, long fileAbsoluteEndPosition)
        {
            if (blobMetadata == null)
                throw new ArgumentNullException("blobMetadata");

            if (fileHeader == null)
                throw new ArgumentNullException("fileHeader");

            if (fileAbsoluteStartPosition < 0)
                throw new ArgumentNullException("fileAbsolutePosition");

            if (fileAbsoluteEndPosition < 0)
                throw new ArgumentNullException("fileAbsoluteEndPosition");

            if (fileAbsoluteStartPosition >= fileAbsoluteEndPosition)
                throw new Exception("fileAbsoluteStartPosition не может быть больше fileAbsoluteEndPosition");

            IFolderMetadata folderMetadata = this.DataAdapter.MetadataAdapter.EnsureFolder(fileHeader.FolderUrl);
            IBlobFileMetadata blobFileMetadata = this.DataAdapter.BlobMetadataAdapter.GetFile(folderMetadata, fileHeader.UniqueID);

            if (blobFileMetadata != null)
            {
                //файл есть, проверяем текущую версию, есть ли она
                ICollection<IBlobFileVersionMetadata> versions = this.DataAdapter.BlobMetadataAdapter.GetVersions(blobFileMetadata);
                bool versionExists = false;
                if (versions != null)
                {
                    foreach (IBlobFileVersionMetadata version in versions)
                    {
                        if (version.UniqueID == fileHeader.VersionUniqueID)
                        {
                            versionExists = true;
                            break;
                        }
                    }
                }

                if (!versionExists)
                    this.DataAdapter.BlobMetadataAdapter.AddExistsFileVersion(this.DataAdapter.MetadataAdapter.CurrentStorage, blobMetadata, folderMetadata, fileHeader, fileAbsoluteStartPosition, fileAbsoluteEndPosition);
            }
            else
            {
                //файла нет, создаем метаданные файла
                this.DataAdapter.BlobMetadataAdapter.AddExistsFileVersion(this.DataAdapter.MetadataAdapter.CurrentStorage, blobMetadata, folderMetadata, fileHeader, fileAbsoluteStartPosition, fileAbsoluteEndPosition);
            }
        }
    }
}