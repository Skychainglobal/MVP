using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace Storage.Metadata.MSSQL
{
    /// <summary>
    /// Определение сохраняемого в БД свойства объекта метаданных.
    /// </summary>
    public class MetadataPropertyDefinition
    {
        /// <summary>
        /// К-тор.
        /// </summary>
        /// <param name="property">Свойство типа.</param>
        internal MetadataPropertyDefinition(PropertyInfo property)
        {
            if (property == null)
                throw new ArgumentNullException("property");
            this.Property = property;
        }

        private PropertyInfo _Property;
        /// <summary>
        /// Свойство типа.
        /// </summary>
        public PropertyInfo Property
        {
            get { return _Property; }
            private set { _Property = value; }
        }

        private string _ColumnName;
        /// <summary>
        /// Название столбца, соответствующего свойству.
        /// </summary>
        public string ColumnName
        {
            get { return _ColumnName; }
            internal set { _ColumnName = value; }
        }

        private string _ParameterName;
        /// <summary>
        /// Название параметра столбца.
        /// </summary>
        public string ParameterName
        {
            get { return _ParameterName; }
            internal set { _ParameterName = value; }
        }

        private SqlDbType _SqlType = SqlDbType.Int;
        /// <summary>
        /// Тип данных столбца.
        /// </summary>
        public SqlDbType SqlType
        {
            get { return _SqlType; }
            internal set { _SqlType = value; }
        }

        private int _Size = 0;
        /// <summary>
        /// Размер столбца для размерных столбцов.
        /// </summary>
        public int Size
        {
            get { return _Size; }
            internal set { _Size = value; }
        }

        private bool _IsIdentity;
        /// <summary>
        /// Возвращает true, если cтолбец является инрементным идентификатором таблицы.
        /// </summary>
        public bool IsIdentity
        {
            get { return _IsIdentity; }
            internal set { _IsIdentity = value; }
        }

        private bool _IsNullable;
        /// <summary>
        /// Столбец поддерживает значения NULL.
        /// </summary>
        public bool IsNullable
        {
            get { return _IsNullable; }
            set { _IsNullable = value; }
        }

        private bool _Indexed;
        /// <summary>
        /// Возращает true, если для столбца основной таблицы объекта установлен индекс по умолчанию.
        /// </summary>
        public bool Indexed
        {
            get { return _Indexed; }
            internal set { _Indexed = value; }
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

        /// <summary>
        /// Строковое представление экземпляра класса MetadataProperty.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (!string.IsNullOrEmpty(this.Property.Name))
                return this.Property.Name;
            return base.ToString();
        }
    }
}
