using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Storage.Metadata.MSSQL
{
    /// <summary>
    /// Представляет некластеризованный индекс.
    /// </summary>
    public class DBNonclusteredIndex : DBIndex
    {
        /// <summary>
        /// Создает экземпляр DBNonclusteredIndex.
        /// </summary>
        /// <param name="schema">Схема индекса.</param>
        /// <param name="table">Таблица.</param>
        internal DBNonclusteredIndex(DBIndexSchema schema, DBTable table)
            : base(schema, table)
        {
        }

        private bool __init_Name = false;
        private string _Name;
        /// <summary>
        /// Полное название индекса, уникальное в рамках базы данных.
        /// </summary>
        public override string Name
        {
            get
            {
                if (!__init_Name)
                {
                    _Name = DBNonclusteredIndex.GetIndexName(this.Table.Prefix, this.Schema.RelativeName);
                    __init_Name = true;
                }
                return _Name;
            }
        }

        /// <summary>
        /// Возвращает название индекса по префиксу таблицы и относительному названию индекса.
        /// </summary>
        /// <param name="tablePrefix">Префикс таблицы.</param>
        /// <param name="indexRelativeName">Относительное название индекса.</param>
        /// <returns></returns>
        internal static string GetIndexName(string tablePrefix, string indexRelativeName)
        {
            if (string.IsNullOrEmpty(tablePrefix))
                throw new ArgumentNullException("tablePrefix");
            if (string.IsNullOrEmpty(indexRelativeName))
                throw new ArgumentNullException("indexRelativeName");

            string indexName = string.Format("IX_{0}_{1}", tablePrefix, indexRelativeName);
            return indexName;
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
        /// Возвращает существующий некластеризованный индекс существующей таблицы, соответвующий схеме индекса.
        /// </summary>
        /// <param name="existingTable">Существующая таблица.</param>
        /// <returns></returns>
        protected override DBIndexInfo GetExistingIndex(DBTableInfo existingTable)
        {
            if (existingTable == null)
                throw new ArgumentNullException("existingTable");


            //получаем некластеризованный индекс по имени.
            DBIndexInfo existingIndex = existingTable.GetIndex(this.Name);

            //если индекс по имени не получили, пытаемся найти индекс по набору столбцов.
            if (existingIndex == null)
            {
                foreach (DBIndexInfo indexInfo in existingTable.Indexes)
                {
                    //пропускаем индекс с первичным ключом.
                    if (indexInfo.IsPrimaryKey)
                        continue;

                    if (!this.Schema.SchemaAdapter.IsPermanentSchema)
                    {
                        //т.к. индексов с соответствующем набором столбцов может быть несколько то выбираем тот, имя которого оканчивается на DBIndex.RelativeName.
                        //а если такового не имеется, то выбираем первый попавшийся индекс с набором столбцов, соответствующих схеме.
                        if (indexInfo.NameLow.EndsWith("_" + this.Schema.RelativeNameLow))
                        {
                            existingIndex = indexInfo;
                            break;
                        }
                        else if (this.ColumnsEqual(indexInfo) && existingIndex == null)
                        {
                            if (!this.Table.ContainsIndex(indexInfo.Name))
                                existingIndex = indexInfo;
                        }
                    }
                    else
                    {
                        //обрабатываем ошибку изменения существующих индексов SharePoint-таблиц, 
                        //у которых состав столбцов совпадает с состовом столбцов индексов, определенных в SPXObjects.
                        if (indexInfo.NameLow.StartsWith(("IX_" + this.Table.Prefix).ToLower()) ||
                            indexInfo.NameLow.StartsWith(("IX_" + this.Table.OriginalPrefix).ToLower()) ||
                            indexInfo.NameLow.StartsWith(("Ind_" + this.Table.Prefix).ToLower()) ||
                            indexInfo.NameLow.StartsWith(("Ind_" + this.Table.OriginalPrefix).ToLower()))
                        {

                            if (this.ColumnsEqual(indexInfo) && existingIndex == null)
                            {
                                if (!this.Table.ContainsIndex(indexInfo.Name))
                                    existingIndex = indexInfo;
                            }
                        }
                    }
                }
            }

            //если для обычного индекса получили кластеризованный, ругаемся.
            if (existingIndex != null && existingIndex.IsPrimaryKey)
                throw new Exception(string.Format("Существующий индекс [{0}] таблицы {1} должен быть некластеризованным индексом.", this.Name, existingTable.Name));

            return existingIndex;
        }

        /// <summary>
        /// Возвращает текст запроса создания индекса.
        /// </summary>
        internal override string GetInitQuery()
        {
            string columns = this.GetColumnsQuery();
            string query = @"
IF NOT EXISTS(SELECT name FROM sys.indexes WITH(NOLOCK) WHERE name = N'{IndexNameText}')
BEGIN
    CREATE NONCLUSTERED INDEX [{IndexName}] ON [dbo].[{TableName}] 
    (
        {Columns}
    ) WITH (
        PAD_INDEX  = OFF, 
        STATISTICS_NORECOMPUTE = OFF, 
        SORT_IN_TEMPDB = OFF, 
        IGNORE_DUP_KEY = OFF, 
        DROP_EXISTING = OFF, 
        ONLINE = OFF, 
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
