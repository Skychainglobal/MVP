using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Storage.Lib;

namespace Storage.Engine
{
    /// <summary>
    /// Адаптер метаданных хранилища.
    /// </summary>
    public interface IMetadataAdapter : IDisposable
    {
        /// <summary>
        /// Текущий инстанс ноды файлового хранилища.
        /// </summary>
        IStorageMetadata CurrentStorage { get; }

        /// <summary>
        /// Возвращает коллекцию папок.
        /// </summary>
        /// <param name="parentFolderID">Идентификатор родительской папки, для которой необходимо вернуть коллекцию дочерних папок.</param>
        /// <returns></returns>
        ICollection<IFolderMetadata> GetFolders(int parentFolderID = 0);

        /// <summary>
        /// Возвращает папку.
        /// </summary>
        /// <param name="folderUrl">Адрес папки.</param>
        /// <param name="throwIfNotExists">Выбросить исключение, если папка не существует?</param>
        /// <returns></returns>
        IFolderMetadata GetFolder(string folderUrl, bool throwIfNotExists = true);

        /// <summary>
        /// Возвращает папку.
        /// </summary>
        /// <param name="folderUrl">Адрес папки.</param>
        /// <returns></returns>
        IFolderMetadata EnsureFolder(string folderUrl);

        ICollection<IFolderMetadata> GetFolders(ICollection<string> folderUrls);

        IStorageMetadata GetStorage(Guid uniqueID);

        IStorageMetadata GetStorage(string host);

        ICollection<IStorageMetadata> GetStorages();

        IStorageMetadata CreateCurrentStorageNode(string host);

        IStorageMetadata CreateStorageNode(string host, Guid uniqueID);

        /// <summary>        
        /// Создает токен для операции с файловым хранилищем.
        /// </summary>
        /// <returns></returns>
        IToken GenerateToken();

        /// <summary>
        /// Возвращает действующий токен на операцию с файловым хранилищем.
        /// </summary>
        /// <param name="tokenUniqueID">Уникальный идентификатор токена.</param>
        IToken GetToken(Guid tokenUniqueID);

        /// <summary>
        /// Удаляет токен из хранилища метаданных.
        /// </summary>
        /// <param name="tokenUniqueID">Уникальный идентификатор токена.</param>
        /// <returns></returns>
        void RemoveToken(Guid tokenUniqueID);

        void SaveStorage(IStorageMetadata storage);

        /// <summary>
        /// Возвращает все реплицируемые папки.
        /// </summary>
        /// <returns></returns>
        ICollection<IReplicationFolderMetadata> GetReplicationFolders();

        /// <summary>
        /// Возвращает настройку репликации для папки.
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="sourceStorage"></param>
        /// <returns></returns>
        IReplicationFolderMetadata GetReplicationFolder(IFolderMetadata folder, IStorageMetadata sourceStorage);

        /// <summary>
        /// Создаёт объект папки для репликации.
        /// </summary>
        IReplicationFolderMetadata CreateReplicationFolder(IFolderMetadata folder, IStorageMetadata sourceStorage);

        /// <summary>
        /// Возвращает папку репликации.
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="sourceStorage"></param>
        /// <returns></returns>
        IReplicationFolderMetadata EnsureReplicationFolder(IFolderMetadata folder, IStorageMetadata sourceStorage);

        /// <summary>
        /// Сохраняет объект папки для репликации.
        /// </summary>
        /// <param name="folderMetadata"></param>
        /// <returns></returns>
        void SaveReplicationFolder(IReplicationFolderMetadata folderMetadata);

        /// <summary>
        /// Возвращает файлы, подлежащие репликации.
        /// </summary>
        /// <param name="folder">Папка.</param>
        /// <param name="from">С какого момента времени, брать файлы.</param>
        /// <returns>Пара [GUID файла, GUID версии]</returns>
        Tuple<Guid, Guid>[] GetReplicationFiles(IStorageMetadata requestNode, IFolderMetadata folder, DateTime from);
    }
}