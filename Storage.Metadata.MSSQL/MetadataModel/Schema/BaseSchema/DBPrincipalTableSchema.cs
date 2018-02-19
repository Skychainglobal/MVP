using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Storage.Metadata.MSSQL
{
    /// <summary>
    /// Представляет класс для работы со схемой таблицы данных.
    /// </summary>
    public abstract class DBPrincipalTableSchema : DBTableSchema
    {
        /// <summary>
        /// Создает экземпляр DBPrincipalTableSchema.
        /// </summary>
        /// <param name="schemaAdapter">Адаптер.</param>
        protected DBPrincipalTableSchema(DBTableSchemaAdapter schemaAdapter)
            : base(schemaAdapter)
        {
        }

        /// <summary>
        /// Добавляет столбец в схему таблицы.
        /// </summary>
        /// <param name="column">Схема столбца.</param>
        public void AddColumn(DBColumnSchema column)
        {
            this.AddColumnInternal(column);
        }

        /// <summary>
        /// Добавляет столбец в схему таблицы.
        /// </summary>
        /// <param name="column">Схема столбца.</param>
        internal override void AddColumnInternal(DBColumnSchema column)
        {
            if (column == null)
                throw new ArgumentNullException("column");

            //чтобы не было ошибки уже существующего столбца при добавлении в таблицу.
            this.PreInitColumnsChange();

            //добавляем столбец к своей схеме
            base.AddColumnInternal(column);
        }

        /// <summary>
        /// Удаляет столбец из схемы таблицы.
        /// </summary>
        /// <param name="columnName">Название удаляемого столбца.</param>
        public void DeleteColumn(string columnName)
        {
            this.DeleteColumnInternal(columnName);
        }

        /// <summary>
        /// Удаляет столбец из схемы таблицы.
        /// </summary>
        /// <param name="columnName">Название удаляемого столбца.</param>
        internal override void DeleteColumnInternal(string columnName)
        {
            if (string.IsNullOrEmpty(columnName))
                throw new ArgumentNullException("columnName");

            //получаем схему столбца
            DBColumnSchema column = this.GetColumn(columnName, true);

            //чтобы не было ошибки несуществующего столбца при удалении из таблицы, 
            //инициализируем коллекцию столбцов таблицы.
            this.PreInitColumnsChange();

            //удаляем столбец из своей схемы.
            base.DeleteColumnInternal(columnName);
        }
    }
}
