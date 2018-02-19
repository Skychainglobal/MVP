using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Storage.Engine;

namespace Storage.Data.Blob
{
    /// <summary>
    /// Адаптер метаданных блоба.
    /// </summary>
    public interface IBlobMetadataAdapter : IDisposable
    {
        /// <summary>
        /// Адаптер метаданных.
        /// </summary>
        IMetadataAdapter MetadataAdapter { get; }

        /// <summary>
        /// Дефолтные контейнеры блобов.
        /// </summary>
        ICollection<IBlobContainerMetadata> DefaultContainers { get; }

        /// <summary>
        /// Получает все контейнеры блобов.
        /// </summary>
        /// <returns></returns>
        ICollection<IBlobContainerMetadata> GetBlobContainers();

        /// <summary>
        /// Получает контейнеры блобов по идентификатору папки.
        /// </summary>
        /// <param name="folderID">Идентификатор папки.</param>
        /// <returns></returns>
        ICollection<IBlobContainerMetadata> GetBlobContainers(int folderID);

        /// <summary>
        /// Получает контейнер блоба.
        /// </summary>
        /// <param name="id">Идентификатор блоба.</param>
        /// <returns></returns>
        IBlobContainerMetadata GetBlobContainer(int id);

        /// <summary>
        /// Создает контейнер блоба.
        /// </summary>
        /// <param name="name">Уникальное контейнера.</param>
        /// <param name="path">Путь до директории контейнера.</param>
        /// <param name="folderID">Идентификатор папки.</param>
        /// <returns></returns>
        IBlobContainerMetadata CreateBlobContainer(string name, string path, int folderID);

        /// <summary>
        /// Сохраняет контейнер.
        /// </summary>
        /// <param name="containerMetadata">Метаданные контейнера.</param>
        void SaveBlobContainer(IBlobContainerMetadata containerMetadata);

        /// <summary>
        /// Возвращает метаданные файла.
        /// </summary>
        /// <param name="folderMetadata">Метаданные папки.</param>
        /// <param name="fileUniqueID">Уникальный идентификатор файла.</param>
        /// <returns></returns>
        IBlobFileMetadata GetFile(IFolderMetadata folderMetadata, Guid fileUniqueID);

        /// <summary>
        /// Возвращает коллекцию версий файла.
        /// </summary>
        /// <param name="fileMetadata">Метаданные файла.</param>
        /// <returns></returns>
        ICollection<IBlobFileVersionMetadata> GetVersions(IBlobFileMetadata fileMetadata);

        /// <summary>
        /// Создает метаданные файла.
        /// </summary>
        /// <param name="folderMetadata">Метаданные папки.</param>
        /// <param name="fileName">Имя файла.</param>
        /// <returns></returns>
        IBlobFileMetadata CreateFile(IFolderMetadata folderMetadata, string fileName);

        /// <summary>
        /// Создает метаданные существующего файла.
        /// </summary>
        /// <param name="storageMetadata">Метаданные хранилища.</param>
        /// <param name="blobMetadata">Метаданные блоба.</param>
        /// <param name="folderMetadata">Метаданные папки.</param>
        /// <param name="fileHeader">Заголовок файла.</param>
        /// <param name="blobStartPosition">Начальная позиция файла в блобе.</param>
        /// <param name="blobEndPosition">Конечная позиция файла в блобе.</param>
        /// <returns></returns>
        IBlobFileMetadata AddExistsFileVersion(IStorageMetadata storageMetadata, IBlobMetadata blobMetadata, IFolderMetadata folderMetadata, IFileHeader fileHeader, long blobStartPosition, long blobEndPosition);

        /// <summary>
        /// Сохраняет метаданные файла.
        /// </summary>
        /// <param name="fileMetadata">Файл.</param>
        /// <param name="versionTimeCreated">Дата создания версии файла.</param>
        IBlobFileVersionMetadata SaveFile(IBlobFileMetadata fileMetadata, DateTime versionTimeCreated);

        /// <summary>
        /// Удаляет метаданные файла.
        /// </summary>
        /// <param name="fileMetadata">Файл.</param>
        void DeleteFile(IBlobFileMetadata fileMetadata);



        /// <summary>
        /// Создает блоб.
        /// </summary>
        /// <param name="name">Имя блоба.</param>
        /// <param name="containerID">Идентификатор контейнера.</param>
        /// <returns></returns>
        IBlobMetadata CreateBlob(string name, int containerID);

        /// <summary>
        /// Возвращает коллекцию блобов контейнера.
        /// </summary>
        /// <param name="containerID">Идентификатор контейнера.</param>
        /// <returns></returns>
        ICollection<IBlobMetadata> GetBlobs(int containerID);

        /// <summary>
        /// Возвращает коллекцию активных блобов контейнера.
        /// </summary>
        /// <param name="containerID">Идентификатор контейнера.</param>
        /// <returns></returns>
        ICollection<IBlobMetadata> GetActiveBlobs(int containerID);

        /// <summary>
        /// Сохраняет блоб.
        /// </summary>
        /// <param name="blob">Метаданные блоба.</param>
        void SaveBlob(IBlobMetadata blob);

        /// <summary>
        /// Возвращает блоб по идентификатору.
        /// </summary>
        /// <param name="id">Идентификатор блоба.</param>
        /// <returns></returns>
        IBlobMetadata GetBlob(int id);
    }
}