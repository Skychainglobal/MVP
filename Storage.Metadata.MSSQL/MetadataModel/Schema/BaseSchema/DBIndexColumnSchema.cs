using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Storage.Metadata.MSSQL
{
    /// <summary>
    /// Представляет столбец индекса схемы таблицы.
    /// </summary>
    public abstract class DBIndexColumnSchema
    {
        /// <summary>
        /// Создает экземпляр DBIndexColumnSchema.
        /// </summary>
        /// <param name="index">Индекс, к которому относится столбец.</param>
        protected DBIndexColumnSchema(DBIndexSchema index)
        {
            if (index == null)
                throw new ArgumentNullException("index");

            this.Index = index;
        }

        private DBIndexSchema _Index;
        /// <summary>
        /// Индекс.
        /// </summary>
        public DBIndexSchema Index
        {
            get { return _Index; }
            private set { _Index = value; }
        }

        /// <summary>
        /// Адаптер схемы таблицы.
        /// </summary>
        public DBTableSchemaAdapter SchemaAdapter
        {
            get { return this.Index.SchemaAdapter; }
        }


        /// <summary>
        /// Инициализирует название столбца индекса.
        /// </summary>
        /// <returns></returns>
        protected abstract string InitName();


        private bool __init_Name = false;
        private string _Name;
        /// <summary>
        /// Название столбца индекса.
        /// </summary>
        public string Name
        {
            get
            {
                if (!__init_Name)
                {
                    _Name = this.InitName();
                    if (string.IsNullOrEmpty(_Name))
                        throw new Exception(string.Format("Не задано название столбца индекса в схеме таблицы {0}.", this.SchemaAdapter.TableName));

                    __init_Name = true;
                }
                return _Name;
            }
        }

        private bool __init_NameLow = false;
        private string _NameLow;
        /// <summary>
        /// Название столбца индекса в нижнем регистре.
        /// </summary>
        protected internal string NameLow
        {
            get
            {
                if (!__init_NameLow)
                {
                    if (!string.IsNullOrEmpty(this.Name))
                        _NameLow = this.Name.ToLower();
                    __init_NameLow = true;
                }
                return _NameLow;
            }
        }
        
        /// <summary>
        /// Инициализирует значение свойства IsDescending.
        /// </summary>
        /// <returns></returns>
        protected abstract bool InitIsDescending();

        private bool __init_IsDescending = false;
        private bool _IsDescending;
        /// <summary>
        /// Возвращает true, если столбец в индексе сортируется по убыванию.
        /// </summary>
        public bool IsDescending
        {
            get
            {
                if (!__init_IsDescending)
                {
                    _IsDescending = this.InitIsDescending();
                    __init_IsDescending = true;
                }
                return _IsDescending;
            }
        }

        private bool __init_Definition = false;
        private string _Definition;
        /// <summary>
        /// Определение столбца в индексе.
        /// </summary>
        public string Definition
        {
            get
            {
                if (!__init_Definition)
                {
                    string directionString = !this.IsDescending ? "ASC" : "DESC";
                    _Definition = string.Format("[{0}] {1}", this.Name, directionString);
                    __init_Definition = true;
                }
                return _Definition;
            }
        }

        private bool __enabled_Ordinal;

        private int _Ordinal;
        /// <summary>
        /// Порядковый номер столбца в индексе.
        /// </summary>
        public int Ordinal
        {
            get
            {
                if (!this.__enabled_Ordinal)
                    throw new Exception("Получение значения свойства Ordinal доступно только после инициализации коллекции столбцов индекса.");
                return _Ordinal;
            }
            internal set
            {
                _Ordinal = value;
                __enabled_Ordinal = true;
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
                this.NameLow == existingColumnToCompare.NameLow &&
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
