using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Storage.Metadata.MSSQL
{
    /// <summary>
    /// Представляет кластеризованный индекс, соответствующий первичному ключу.
    /// </summary>
    public class DBClusteredIndex : DBIndex
    {
        /// <summary>
        /// Создает экземпляр DBClusteredIndex.
        /// </summary>
        /// <param name="schema">Схема индекса.</param>
        /// <param name="table">Таблица.</param>
        internal DBClusteredIndex(DBIndexSchema schema, DBTable table)
            : base(schema, table)
        {
        }

        private bool __init_Name = false;
        private string _Name;
        /// <summary>
        /// Полное название индекса первичного ключа, уникальное в рамках базы данных.
        /// </summary>
        public override string Name
        {
            get
            {
                if (!__init_Name)
                {
                    _Name = string.Format("PK_{0}", this.Table.Prefix);
                    __init_Name = true;
                }
                return _Name;
            }
        }

        /// <summary>
        /// Сбрасываем инициализацию имени индекса и зависимых от него свойств.
        /// </summary>
        internal override void ResetName()
        {
            this.__init_Name = false;

            base.ResetName();
        }

        /// <summary>
        /// Возвращает существующий кластеризованный индекс существующей таблицы, соответвующий схеме индекса.
        /// </summary>
        /// <param name="existingTable">Существующая таблица.</param>
        /// <returns></returns>
        protected override DBIndexInfo GetExistingIndex(DBTableInfo existingTable)
        {
            if (existingTable == null)
                throw new ArgumentNullException("existingTable");

            //получаем кластеризованный индекс по имени.
            DBIndexInfo existingIndex = existingTable.GetIndex(this.Name);

            //обрабатываем ошибку изменения существующих индексов SharePoint-таблиц, 
            //у которых состав столбцов совпадает с состовом столбцов индексов, определенных в SPXObjects.
            if (!this.Schema.SchemaAdapter.IsPermanentSchema)
            {
                //если индекс по имени не получили, пытаемся найти индекс по признаку первичного ключа - такой индекс в таблице может быть только один.
                if (existingIndex == null)
                    existingIndex = existingTable.PrimaryKey;
            }

            //если для кластеризованного индекса получили обычный, ругаемся.
            if (existingIndex != null && !existingIndex.IsPrimaryKey)
                throw new Exception(string.Format("Существующий индекс [{0}] таблицы {1} должен быть кластеризованным индексом.", this.Name, existingTable.Name));

            return existingIndex;
        }

        /// <summary>
        /// Возвращает текст запроса создания индекса первичного ключа.
        /// </summary>
        internal override string GetInitQuery()
        {
            string columns = this.GetColumnsQuery();
            string query = @"
IF NOT EXISTS(SELECT name FROM sys.indexes WITH(NOLOCK) WHERE name = N'{IndexNameText}')
BEGIN
    ALTER TABLE [dbo].[{TableName}] ADD CONSTRAINT [{IndexName}] PRIMARY KEY CLUSTERED
    (
        {Columns}
    ) WITH (
        PAD_INDEX  = OFF, 
        STATISTICS_NORECOMPUTE = OFF, 
        IGNORE_DUP_KEY = OFF, 
        ALLOW_ROW_LOCKS  = ON, 
        ALLOW_PAGE_LOCKS  = ON
    ) ON [PRIMARY]
END"
                .ReplaceKey("IndexNameText", this.Name.QueryEncode())
                .ReplaceKey("IndexName", this.Name)
                .ReplaceKey("TableName", this.Table.Name)
                .ReplaceKey("Columns", columns)
                ;
            return query;
        }
    }
}
