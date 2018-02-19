using Storage.Lib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skychain.Models
{
    /// <summary>
    /// Представляет файле в хранилище.
    /// Содержит методы чтения, создания, обновления, удаления файла и проверки наличия файла.
    /// </summary>
    public interface ISkyFile
    {
        /// <summary>
        /// Уникальный идентификатор файла.
        /// </summary>
        Guid ID { get; }

        /// <summary>
        /// Возвращает true, если файл является новым и не имеет идентификатора.
        /// </summary>
        bool IsNew { get; }

        /// <summary>
        /// Контекст системы.
        /// </summary>
        ISkyContext Context { get; }

        /// <summary>
        /// Файл только для чтения.
        /// При установленном значении true загрузка новой версии файла запрещена.
        /// </summary>
        bool ReadOnly { get; }

        /// <summary>
        /// Имя файла.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Размер файла.
        /// </summary>
        long Size { get; }

        /// <summary>
        /// Дата создания файла.
        /// </summary>
        DateTime TimeCreated { get; }

        /// <summary>
        /// Дата последнего изменения файла.
        /// </summary>
        DateTime TimeModified { get; }

        /// <summary>
        /// Возвращает true, если файл существует в хранилище.
        /// </summary>
        bool Exists { get; }

        /// <summary>
        /// Генерирует исключение в случае отсутствия файла.
        /// </summary>
        void CheckExists();

        /// <summary>
        /// Загружает новый файл в хранилище, если файл не существует.
        /// Загружает версию файла в хранилище, если файл существует.
        /// Генерирует исключение в случае, если файл существует и является файлом только для чтения.
        /// </summary>
        /// <param name="fileName">Название загружаемого файла.</param>
        /// <param name="fileStream">Поток данных загружаемого файла.</param>
        void Upload(string fileName, Stream fileStream);

        /// <summary>
        /// Открывает поток чтения данных файла.
        /// </summary>
        Stream Open();

		/// <summary>
		/// Контент файла.
		/// </summary>
		byte[] Content { get; }

        /// <summary>
        /// Удаляет файл из хранилища.
        /// Генерирует исключение в случае, если файл является файлом только для чтения.
        /// </summary>
        void Delete();

        /// <summary>
        /// Возвращает сессионную ссылку на файл.
        /// </summary>
        /// <returns></returns>
        string GetSessionLink();
    }
}
