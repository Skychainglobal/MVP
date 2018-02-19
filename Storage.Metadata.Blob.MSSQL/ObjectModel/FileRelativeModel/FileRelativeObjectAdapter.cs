using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Storage.Engine;
using Storage.Metadata.MSSQL;

namespace Storage.Metadata.Blob.MSSQL
{
    /// <summary>
    /// Представляет адаптер для работы с типом метаданных, привязанных к метаданным файлов. Хранение которых осуществляется в распределенных таблицах относительно метаданных файлов.
    /// </summary>
    /// <typeparam name="T">Тип метаданных.</typeparam>
    public abstract class FileRelativeObjectAdapter<T> : DistributedTableAdapter<T>
    {
        /// <summary>
        /// К-тор.
        /// </summary>
        /// <param name="schemaAdapter">Адаптер метаданных системы.</param>        
        internal FileRelativeObjectAdapter(MetadataAdapter metadataAdapter)
            : base(metadataAdapter)
        {
            this.CheckIsFileRelative();
        }

        private bool __init_IsFileRelative = false;
        private bool _IsFileRelative;
        /// <summary>
        /// Если true, тип метаданных адаптера реализует интерфейс IFileRelativeObject.
        /// </summary>
        private bool IsFileRelative
        {
            get
            {
                if (!__init_IsFileRelative)
                {
                    Type fileRelativeObjectInterface = typeof(IFileRelativeObject);
                    _IsFileRelative = fileRelativeObjectInterface.IsAssignableFrom(this.Type);
                    __init_IsFileRelative = true;
                }
                return _IsFileRelative;
            }
        }

        /// <summary>
        /// Проверяет, что тип метаданных адаптера реализует интерфейс IFileRelativeObject.
        /// </summary>
        private void CheckIsFileRelative()
        {
            if (!this.IsFileRelative)
                throw new Exception(string.Format("Тип объекта, связанного с метаданными файлов {0}, не реализует интерфейс IFileRelativeObject.", this.Type.FullName));
        }

        /// <summary>
        /// Получение названия раздела таблицы по объекту метаданных.
        /// </summary>
        /// <param name="metadataObject">Объект метаданных.</param>
        /// <returns>Название раздела таблицы соответствующего метаданным файла и текущему типу метаданных.</returns>
        protected override string GetTableName(T metadataObject)
        {
            this.CheckIsFileRelative();

            IFileRelativeObject relativeObject = (IFileRelativeObject)metadataObject;
            if (relativeObject.FileMetadata == null)
                throw new Exception(String.Format("Не удалось получить метаданные файла у объекта: {0}", metadataObject.ToString()));

            return this.GetTableName(relativeObject.FileMetadata);
        }

        /// <summary>
        /// Получение названия раздела таблицы по метаданным файла и текущему типу метаданных.
        /// </summary>
        /// <param name="metadata">Метаданные файла.</param>
        /// <returns>Название раздела таблицы соответствующего метаданным файла и текущему типу метаданных.</returns>
        protected string GetTableName(IFileMetadata metadata)
        {
            FileMetadata fileMetadata = (FileMetadata)metadata;
            return string.Format("{0}_{1}", fileMetadata.Adapter.GetTableName(fileMetadata.UniqueID, fileMetadata.FolderMetadata), this.TypeDefinition.TableName);
        }
    }
}
