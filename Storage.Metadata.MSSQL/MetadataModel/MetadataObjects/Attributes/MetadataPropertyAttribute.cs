using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Storage.Metadata.MSSQL
{
    /// <summary>
    /// Атрибут сохраняемого в БД свойства объекта метаданных.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class MetadataPropertyAttribute : Attribute
    {
        /// <summary>
        /// К-тор.
        /// </summary>
        /// <param name="columnName">Название столбца в БД.</param>
        public MetadataPropertyAttribute(string columnName)
        {
            this.ColumnName = columnName;
            this.UpdateMode = MetadataPropertyUpdateMode.OnInsertAndUpdate;
        }

        /// <summary>
        /// К-тор.
        /// </summary>
        /// <param name="columnName">Название столбца в БД.</param>
        /// <param name="updateMode">Тип обновления значения свойства.</param>
        public MetadataPropertyAttribute(string columnName, MetadataPropertyUpdateMode updateMode)
        {
            this.ColumnName = columnName;
            this.UpdateMode = updateMode;
        }

        private SqlDbType _SqlType = SqlDbType.NVarChar;
        /// <summary>
        /// Sql-тип данных столбца.
        /// </summary>
        public SqlDbType SqlType
        {
            get { return _SqlType; }
            set
            {
                _SqlType = value;
                this.SqlTypeDefined = true;
            }
        }

        private bool _SqlTypeDefined = false;
        /// <summary>
        /// Возвращает в true, если значение свойства SqlType явно задано в объявлении атрибута.
        /// </summary>
        internal bool SqlTypeDefined
        {
            get { return _SqlTypeDefined; }
            private set { _SqlTypeDefined = value; }
        }

        private string _ColumnName;
        /// <summary>
        /// Название столбца, соответствующего свойству.
        /// </summary>
        public string ColumnName
        {
            get { return _ColumnName; }
            set { _ColumnName = value; }
        }

        private int _Size;
        /// <summary>
        /// Размер столбца. При установленном значении -1 принимает значение max.
        /// </summary>
        public int Size
        {
            get { return _Size; }
            set { _Size = value; }
        }

        private bool _IsNullable = true;
        /// <summary>
        /// Столбец поддерживает значения NULL.
        /// </summary>
        public bool IsNullable
        {
            get { return _IsNullable; }
            set { _IsNullable = value; }
        }

        private bool _IsIdentity;
        /// <summary>
        /// Столбец является инкрементным идентификатором строк таблицы.
        /// </summary>
        public bool IsIdentity
        {
            get { return _IsIdentity; }
            set { _IsIdentity = value; }
        }

        private bool _Indexed;
        /// <summary>
        /// При установленном true, создает индекс по умолчанию для столбца основной таблицы объектов.
        /// </summary>
        public bool Indexed
        {
            get { return _Indexed; }
            set { _Indexed = value; }
        }

        private MetadataPropertyUpdateMode _UpdateMode;
        /// <summary>
        /// Тип обновления значения свойства объекта сохраняемого в БД.
        /// </summary>
        public MetadataPropertyUpdateMode UpdateMode
        {
            get { return _UpdateMode; }
            internal set { _UpdateMode = value; }
        }
    }

    // <summary>
    /// Тип обновления значения свойства объекта сохраняемого в БД.
    /// </summary>
    public enum MetadataPropertyUpdateMode
    {
        /// <summary>
        /// Способ сохранения свойства объекта определяется системой.
        /// </summary>
        SqlManaged = 0,

        /// <summary>
        /// Значение свойства объекта сохраняется в БД только при создании объекта.
        /// </summary>
        OnInsert = 1,

        /// <summary>
        /// Значение свойства объекта сохраняется в БД при создании и изменении объекта.
        /// </summary>
        OnInsertAndUpdate = 2
    }
}
