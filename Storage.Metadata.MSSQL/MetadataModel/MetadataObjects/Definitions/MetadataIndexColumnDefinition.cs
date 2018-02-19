using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Storage.Metadata.MSSQL
{
    /// <summary>
    /// Определение индексируемого столбца таблицы метаданных.
    /// </summary>
    public class MetadataIndexColumnDefinition
    {
        /// <summary>
        /// К-тор.
        /// </summary>
        /// <param name="property">Определение свойства объекта метаданных.</param>
        /// <param name="isDescending">Если true, сортировка осуществляется в обратном направлении.</param>
        /// <param name="columnOrder">Порядковый номер столбца в индексе.</param>
        internal MetadataIndexColumnDefinition(MetadataPropertyDefinition property, bool isDescending, int columnOrder)
        {
            if (property == null)
                throw new ArgumentNullException("property");

            this.Property = property;
            this.IsDescending = isDescending;
            this.ColumnOrder = columnOrder;
        }

        /// <summary>
        /// Казвание столбца в БД.
        /// </summary>
        public string ColumnName
        {
            get { return this.Property.ColumnName; }
        }

        private MetadataPropertyDefinition _Property;
        /// <summary>
        /// Определение сохраняемого в БД свойства объекта метаданных.
        /// </summary>
        public MetadataPropertyDefinition Property
        {
            get { return _Property; }
            private set { _Property = value; }
        }

        private bool _IsDescending;
        /// <summary>
        /// При установленном true, сортировка столбца индекса осуществляется по направлению DESC.
        /// </summary>
        public bool IsDescending
        {
            get { return _IsDescending; }
            private set { _IsDescending = value; }
        }

        private int _ColumnOrder;
        /// <summary>
        /// Порядковый номер следования столбца в индексе.
        /// </summary>
        public int ColumnOrder
        {
            get { return _ColumnOrder; }
            private set { _ColumnOrder = value; }
        }
    }
}
