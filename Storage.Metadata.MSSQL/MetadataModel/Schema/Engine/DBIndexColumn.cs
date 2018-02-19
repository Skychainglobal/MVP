using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Storage.Metadata.MSSQL
{
    /// <summary>
    /// Представляет столбец индекса таблицы.
    /// </summary>
    public class DBIndexColumn
    {
        /// <summary>
        /// Создает экземпляр DBIndexColumn.
        /// </summary>
        /// <param name="schema">Схема столбца индекса.</param>
        /// <param name="index">Индекс, к которому относится столбец.</param>
        public DBIndexColumn(DBIndexColumnSchema schema, DBIndex index)
        {
            if (schema == null)
                throw new ArgumentNullException("schema");
            if (index == null)
                throw new ArgumentNullException("index");

            this.Schema = schema;
            this.Index = index;
        }

        private DBIndexColumnSchema _Schema;
        /// <summary>
        /// Схема столбца индекса.
        /// </summary>
        public DBIndexColumnSchema Schema
        {
            get { return _Schema; }
            private set { _Schema = value; }
        }

        private DBIndex _Index;
        /// <summary>
        /// Индекс, к которому односится столбец.
        /// </summary>
        public DBIndex Index
        {
            get { return _Index; }
            private set { _Index = value; }
        }

        /// <summary>
        /// Название столбца индекса.
        /// </summary>
        public string Name
        {
            get { return this.Schema.Name; }
        }

        /// <summary>
        /// Название столбца индекса в нижнем регистре.
        /// </summary>
        internal string NameLow
        {
            get { return this.Schema.NameLow; }
        }

        /// <summary>
        /// Определение столбца в индексе.
        /// </summary>
        public string Definition
        {
            get { return this.Schema.Definition; }
        }

        /// <summary>
        /// Возвращает true, если столбец в индексе сортируется по убыванию.
        /// </summary>
        public bool IsDescending
        {
            get { return this.Schema.IsDescending; }
        }

        /// <summary>
        /// Порядковый номер столбца в индексе.
        /// </summary>
        public int Ordinal
        {
            get { return this.Schema.Ordinal; }
        }

        private bool __init_Column = false;
        private DBColumn _Column;
        /// <summary>
        /// Столбец таблицы, присутствующий в схеме индекса.
        /// </summary>
        public DBColumn Column
        {
            get
            {
                if (!__init_Column)
                {
                    _Column = this.Index.Table.GetColumn(this.Name, true);
                    __init_Column = true;
                }
                return _Column;
            }
        }

        private bool __init_ExistingColumn = false;
        private DBIndexColumnInfo _ExistingColumn;
        /// <summary>
        /// Сущесвтующий столбец индекса.
        /// </summary>
        public DBIndexColumnInfo ExistingColumn
        {
            get
            {
                if (!__init_ExistingColumn)
                {
                    if (this.Index.ExistingIndex != null)
                        _ExistingColumn = this.Index.ExistingIndex.GetColumn(this.Name);
                    else
                        _ExistingColumn = null;
                    __init_ExistingColumn = true;
                }
                return _ExistingColumn;
            }
        }

        private bool __init_Exists = false;
        private bool _Exists;
        /// <summary>
        /// Возвращает true, если столбец присутствует в индексе.
        /// </summary>
        public bool Exists
        {
            get
            {
                if (!__init_Exists)
                {
                    _Exists = this.ExistingColumn != null;
                    __init_Exists = true;
                }
                return _Exists;
            }
        }

        private bool __init_IndexRecreateRequired = false;
        private bool _IndexRecreateRequired;
        /// <summary>
        /// Возвращает true, если вследствие изменения параметров столбца индекса требуется пересоздание индекса.
        /// </summary>
        public bool IndexRecreateRequired
        {
            get
            {
                if (!__init_IndexRecreateRequired)
                {
                    _IndexRecreateRequired =
                        !this.Exists || !this.Schema.EqualsTo(this.ExistingColumn);
                    __init_IndexRecreateRequired = true;
                }
                return _IndexRecreateRequired;
            }
        }

        /// <summary>
        /// Возвращает true, если сравниваемый с данным столбцом индекса существующий столбец индекса имеет одинаковое название, 
        /// порядковый номер в индексе и направление сортировки.
        /// </summary>
        /// <param name="existingColumnToCompare">Существующий столбец индекса для сравнения.</param>
        /// <returns></returns>
        internal bool EqualsTo(DBIndexColumnInfo existingColumnToCompare)
        {
            if (existingColumnToCompare == null)
                throw new ArgumentNullException("existingColumnToCompare");

            bool equals =
                (!this.Column.Exists && this.NameLow == existingColumnToCompare.NameLow ||
                this.Column.Exists && this.Column.ExistingColumn.NameLow == existingColumnToCompare.NameLow) &&
                this.Ordinal == existingColumnToCompare.Ordinal &&
                this.IsDescending == existingColumnToCompare.IsDescending;

            return equals;
        }

        /// <summary>
        /// Строковое представление экземпляра DBIndexColumnSchema.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (!string.IsNullOrEmpty(this.Definition))
                return this.Definition;
            return base.ToString();
        }
    }
}
