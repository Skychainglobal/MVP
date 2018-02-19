using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Storage.Metadata.MSSQL
{
    /// <summary>
    /// Представляет атрибут определения индекса таблицы, сопоставленной с классом.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class MetadataIndexAttribute : Attribute
    {
        private string _RelativeName;
        /// <summary>
        /// Название индекса, уникальное в рамках таблицы. Полное название индекса состоит из префикса таблицы и относительного названия индекса.
        /// </summary>
        public string RelativeName
        {
            get { return _RelativeName; }
            set { _RelativeName = value; }
        }

        private bool _IsVersionIndex;
        /// <summary>
        /// При установленном значении true, индекс применяется к таблице версий исходной таблицы.
        /// </summary>
        public bool IsVersionIndex
        {
            get { return _IsVersionIndex; }
            set { _IsVersionIndex = value; }
        }

        private int _ColumnOrder;
        /// <summary>
        /// Порядковый номер следования столбца в индексе.
        /// </summary>
        public int ColumnOrder
        {
            get { return _ColumnOrder; }
            set { _ColumnOrder = value; }
        }

        private bool _IsDescending = false;
        /// <summary>
        /// При установленном true, сортировка столбца индекса осуществляется по направлению DESC.
        /// </summary>
        public bool IsDescending
        {
            get { return _IsDescending; }
            set { _IsDescending = value; }
        }
    }
}
