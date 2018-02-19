using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Storage.Metadata.MSSQL
{
    /// <summary>
    /// Построитель запросов в БД.
    /// </summary>
    public class MetadataQueryBuilder
    {
        /// <summary>
        /// К-тор.
        /// </summary>
        /// <param name="table">Таблица метаданных.</param>
        /// <param name="typeDefinition">Определение типа метаданных.</param>
        public MetadataQueryBuilder(DBTable table, MetadataTypeDefinition typeDefinition)
        {
            if (table == null)
                throw new ArgumentNullException("table");
            if (typeDefinition == null)
                throw new ArgumentNullException("typeDefinition");

            this.Table = table;
            this.TypeDefinition = typeDefinition;
        }

        private DBTable _Table;
        /// <summary>
        /// Таблица, для которой формируются запросы.
        /// </summary>
        public DBTable Table
        {
            get { return _Table; }
            private set { _Table = value; }
        }

        private MetadataTypeDefinition _TypeDefinition;
        /// <summary>
        /// Определение типа метаданных.
        /// </summary>
        public MetadataTypeDefinition TypeDefinition
        {
            get { return _TypeDefinition; }
            private set { _TypeDefinition = value; }
        }

        private bool __init_InsertQuery = false;
        private string _InsertQuery;
        /// <summary>
        /// Запрос создания нового объекта.
        /// </summary>
        public string InsertQuery
        {
            get
            {
                if (!__init_InsertQuery)
                {
                    _InsertQuery = @"
            INSERT INTO [{TableName}]
            ({Columns})
            SELECT {Values}"
                        .ReplaceKey("TableName", this.Table.Name)
                        .ReplaceKey("Columns", String.Join(", ", this.TypeDefinition.ParameterProperties.Select(x => string.Format("[{0}]", x.ColumnName)).ToArray()))
                        .ReplaceKey("Values", String.Join(", ", this.TypeDefinition.ParameterProperties.Select(x => x.ParameterName).ToArray()));

                    __init_InsertQuery = true;
                }
                return _InsertQuery;
            }
        }

        private bool __init_UpdateQuery = false;
        private string _UpdateQuery;
        /// <summary>
        /// Запрос обновления данных в таблице метаданных.
        /// </summary>
        public string UpdateQuery
        {
            get
            {
                if (!__init_UpdateQuery)
                {
                    _UpdateQuery = null;
                    if (this.TypeDefinition.ParameterProperties.Count > 0)
                    {
                        _UpdateQuery = @"
                     UPDATE [{TableName}]
                     SET
                     {ColumnsSet}"
                            .ReplaceKey("TableName", this.Table.Name)
                            .ReplaceKey("ColumnsSet", string.Join(@", ", this.TypeDefinition.ParameterProperties.Select(x => string.Format("[{0}] = {1}", x.ColumnName, x.ParameterName)).ToArray()));
                    }
                    __init_UpdateQuery = true;
                }
                return _UpdateQuery;
            }
        }

        private bool __init_SelectQuery = false;
        private string _SelectQuery;
        /// <summary>
        /// Запрос выборки данных из таблицы метаданных.
        /// </summary>
        public string SelectQuery
        {
            get
            {
                if (!__init_SelectQuery)
                {
                    _SelectQuery = null;
                    if (this.TypeDefinition.AllMetadataProperties.Count > 0)
                    {
                        //формируем запрос выборки метаданных, добавляя перенос строки в конце для возможности добавления условий к запросу.
                        //не добавляем скобвки к названию таблицы и названиям столбцов.
                        _SelectQuery = @"SELECT {SelectColumns} FROM {TableName} WITH(NOLOCK)
"
                            .ReplaceKey("SelectColumns", this.SelectColumns)
                            .ReplaceKey("TableName", this.Table.Name)
                            ;
                    }
                    __init_SelectQuery = true;
                }
                return _SelectQuery;
            }
        }

        private bool __init_SelectColumns = false;
        private string _SelectColumns;
        /// <summary>
        /// Столбцы выборки данных объекта.
        /// </summary>
        internal string SelectColumns
        {
            get
            {
                if (!__init_SelectColumns)
                {
                    _SelectColumns = this.GetSelectColumnsSquence(false);
                    __init_SelectColumns = true;
                }
                return _SelectColumns;
            }
        }

        /// <summary>
        /// Возвращает последовательность столбцов для выборки.
        /// </summary>        
        /// <returns></returns>
        internal string GetSelectColumnsSquence(bool includeTableName)
        {
            //формируем последовательность столбцов для выборки.
            StringBuilder columnsBuilder = new StringBuilder();
            string tablePrefix = includeTableName ?
                this.Table.Name + "." :
                string.Empty;
            foreach (MetadataPropertyDefinition property in this.TypeDefinition.AllMetadataProperties)
            {
                if (columnsBuilder.Length > 0)
                    columnsBuilder.Append(", ");

                //формируем название столбца для выборки.
                columnsBuilder.AppendFormat("{0}{1}", tablePrefix, property.ColumnName);
            }

            //возвращаем сформированную последовательность столбцов для выборки.
            string columnsSequence = null;
            if (columnsBuilder.Length > 0)
                columnsSequence = columnsBuilder.ToString();
            return columnsSequence;
        }
    }
}
