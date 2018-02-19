using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Storage.Lib;

namespace Storage.Engine
{
    /// <summary>
    /// Адаптер данных хранилища.
    /// </summary>
    public interface IDataAdapter : IDisposable
    {
        /// <summary>
        /// Адаптер метаданных хранилища.
        /// </summary>
        IMetadataAdapter MetadataAdapter { get; }

        /// <summary>
        /// Удаляет файл из хранилища.
        /// </summary>
        /// <param name="folderMetadata">Метаданные папки.</param>
        /// <param name="fileUniqueID">Уникальный идентификатор файла.</param>
        /// <returns></returns>
        bool DeleteFile(IFolderMetadata folderMetadata, Guid fileUniqueID);

        /// <summary>
        /// Удаляет файл из хранилища.
        /// </summary>
        /// <param name="fileMetadata">Метаданные файла.</param>
        /// <returns></returns>
        bool DeleteFile(IFileMetadata fileMetadata);

        /// <summary>
        /// Записывает файл в хранилище.
        /// </summary>
        /// <param name="folderMetadata">Метаданные папки.</param>
        /// <param name="fileName">Имя файла.</param>
        /// <param name="stream">Содержимое файла.</param>
        /// <returns></returns>
        IFileVersionMetadata WriteFile(IFolderMetadata folderMetadata, string fileName, Stream stream);

        /// <summary>
        /// Записывает версию файла в хранилище.
        /// </summary>
        /// <param name="fileMetadata">Метаданные существующего файла.</param>
        /// <param name="stream">Содержимое файла.</param>
        /// <param name="fileName">Имя файла.</param>
        /// <returns></returns>
        IFileVersionMetadata WriteFileVersion(IFileMetadata fileMetadata, Stream stream, string fileName = null);

        /// <summary>
        /// Записывает файл с удаленного узла в текущее хранилище.
        /// </summary>
        /// <param name="folderMetadata">Метаданные папки для записи.</param>
        /// <param name="remoteFile">Файл с удаленного узла.</param>
        /// <returns></returns>
        IFileVersionMetadata WriteRemoteFile(IFolderMetadata folderMetadata, IRemoteFile remoteFile);

        /// <summary>
        /// Создаёт версию файла
        /// </summary>
        /// <param name="fileMetadata">Метаданные существующего файла.</param>
        /// <param name="remoteFile">Файл с удаленного узла.</param>
        /// <returns></returns>
        IFileVersionMetadata WriteRemoteFileVersion(IFileMetadata fileMetadata, IRemoteFile remoteFile);


        /// <summary>
        /// Считывает поток данных файла.
        /// </summary>
        /// <param name="fileMetadata">Метаданные файла.</param>
        /// <returns></returns>
        Stream ReadFileStream(IFileMetadata fileMetadata);

        /// <summary>
        /// Считывает поток данных версии файла.
        /// </summary>
        /// <param name="versionMetadata">Метаданные версии файла.</param>
        /// <returns></returns>
        Stream ReadFileVersionStream(IFileVersionMetadata versionMetadata);

        /// <summary>
        /// Возвращает метаданные файла.
        /// </summary>
        /// <param name="folderMetadata">Метаданные папки.</param>
        /// <param name="fileUniqueID">Уникальный идентификатор файла.</param>
        /// <returns></returns>
        IFileMetadata ReadFileMetadata(IFolderMetadata folderMetadata, Guid fileUniqueID);

        /// <summary>
        /// Возвращает коллекцию метаданных версий файла.
        /// </summary>
        /// <param name="fileMetadata">Метаданные файла.</param>
        /// <returns></returns>
        ICollection<IFileVersionMetadata> ReadFileVersionsMetadata(IFileMetadata fileMetadata);

        /// <summary>
        /// Возвращает содержимое файла хранилища.
        /// </summary>
        /// <param name="fileMetadata">Метаданные файла.</param>
        /// <returns></returns>
        byte[] ReadFileContent(IFileMetadata fileMetadata);

        /// <summary>
        /// Возвращает содержимое версии файла.
        /// </summary>
        /// <param name="versionMetadata">Метаданные версии файла.</param>
        /// <returns></returns>
        byte[] ReadFileVersionContent(IFileVersionMetadata versionMetadata);

        /// <summary>
        /// Восстановливает метаданные по данным.
        /// </summary>
        void RestoreMetadata();
    }
}