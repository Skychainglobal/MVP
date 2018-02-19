using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Storage.Metadata.MSSQL
{
    /// <summary>
    /// Представляет класс, содержащий параметры столбца существующего индекса таблицы.
    /// </summary>
    public class DBIndexColumnInfo
    {
        internal DBIndexColumnInfo(DataRow data, DBIndexInfo index)
        {
            if (data == null)
                throw new ArgumentNullException("data");
            if (index == null)
                throw new ArgumentNullException("index");

            this.Data = data;
            this.Index = index;
        }

        private DataRow _Data;
        private DataRow Data
        {
            get { return _Data; }
            set { _Data = value; }
        }

        private DBIndexInfo _Index;
        /// <summary>
        /// Индекс, в котором содержистя столбец.
        /// </summary>
        public DBIndexInfo Index
        {
            get { return _Index; }
            private set { _Index = value; }
        }

        private bool __init_Reader = false;
        private DataRowReader _Reader;
        private DataRowReader Reader
        {
            get
            {
                if (!__init_Reader)
                {
                    _Reader = new DataRowReader(this.Data);
                    __init_Reader = true;
                }
                return _Reader;
            }
        }

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
                    _Name = this.Reader.GetStringValue("ColumnName");
                    if (string.IsNullOrEmpty(_Name))
                        throw new Exception("Не удалось получить название столбца индекса.");
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
        internal string NameLow
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

        private bool __init_Column = false;
        private DBColumnInfo _Column;
        /// <summary>
        /// Столбец.
        /// </summary>
        public DBColumnInfo Column
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

        private bool __init_IsDescending = false;
        private bool _IsDescending;
        /// <summary>
        /// Направление сортировки столбца в индексе.
        /// </summary>
        public bool IsDescending
        {
            get
            {
                if (!__init_IsDescending)
                {
                    _IsDescending = this.Reader.GetBooleanValue("IsDescending");
                    __init_IsDescending = true;
                }
                return _IsDescending;
            }
        }

        private bool __init_Definition = false;
        private string _Definition;
        /// <summary>
        /// Текстовое определение столбца в индексе.
        /// </summary>
        public string Definition
        {
            get
            {
                if (!__init_Definition)
                {
                    if (!string.IsNullOrEmpty(this.Name))
                    {
                        string sortDirection = !this.IsDescending ? "ASC" : "DESC";
                        _Definition = string.Format("[{0}] {1}", this.Name, sortDirection);
                    }
                    __init_Definition = true;
                }
                return _Definition;
            }
        }

        private bool __init_Ordinal = false;
        private int _Ordinal;
        /// <summary>
        /// Порядковый номер столбца в индексе.
        /// </summary>
        public int Ordinal
        {
            get
            {
                if (!__init_Ordinal)
                {
                    _Ordinal = this.Reader.GetIntegerValue("Ordinal");
                    if (_Ordinal == 0)
                        throw new Exception(string.Format("Не удалось получить значение порядкового номера для столбца [{0}] индекса [{1}].", this.Name, this.Index.Name));
                    __init_Ordinal = true;
                }
                return _Ordinal;
            }
        }

        /// <summary>
        /// Текстовое представление экземпляра DBIndexColumnInfo.
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
