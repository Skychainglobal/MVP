using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Storage.Metadata.MSSQL
{
    /// <summary>
    /// Определения индекса таблицы объектов метаданных.
    /// </summary>
    public class MetadataIndexDefinition
    {
        /// <summary>
        /// К-тор.
        /// </summary>
        /// <param name="relativeName">Суффикс имени индекса.</param>
        /// <param name="classDefinition">Определение класса.</param>
        internal MetadataIndexDefinition(string relativeName, MetadataTypeDefinition classDefinition)
        {
            if (string.IsNullOrEmpty(relativeName))
                throw new ArgumentNullException("relativeName");
            if (classDefinition == null)
                throw new Exception("classDefinition");

            this.RelativeName = relativeName;
            this.ClassDefinition = classDefinition;
        }

        private string _RelativeName;
        /// <summary>
        /// Суффикс имени индекса.
        /// </summary>
        public string RelativeName
        {
            get { return _RelativeName; }
            private set { _RelativeName = value; }
        }

        private bool _CreatedByColumn;
        /// <summary>
        /// Индекс созданный столбцами по умолчанию.
        /// </summary>
        internal bool CreatedByColumn
        {
            get { return _CreatedByColumn; }
            set { _CreatedByColumn = value; }
        }

        private MetadataTypeDefinition _ClassDefinition;
        /// <summary>
        /// Определение класса.
        /// </summary>
        public MetadataTypeDefinition ClassDefinition
        {
            get { return _ClassDefinition; }
            private set { _ClassDefinition = value; }
        }

        private bool __init_IndexColumns = false;
        private List<MetadataIndexColumnDefinition> _IndexColumns;
        /// <summary>
        /// Индексируемые столбцы, отсортированные в заданном порядке.
        /// </summary>
        public List<MetadataIndexColumnDefinition> IndexColumns
        {
            get
            {
                if (!__init_IndexColumns)
                {
                    lock (this)
                    {
                        if (!__init_IndexColumns)
                        {
                            _IndexColumns = new List<MetadataIndexColumnDefinition>();
                            if (this.InitializingColumns.Count == 0)
                                throw new Exception(string.Format("Отсутствуют столбцы в индексе {0}.", this.RelativeName));

                            Dictionary<int, MetadataIndexColumnDefinition> uniqueOrders = new Dictionary<int, MetadataIndexColumnDefinition>();
                            foreach (MetadataIndexColumnDefinition indexColumn in this.InitializingColumns)
                            {
                                if (!uniqueOrders.ContainsKey(indexColumn.ColumnOrder))
                                    uniqueOrders.Add(indexColumn.ColumnOrder, indexColumn);
                                else
                                {
                                    MetadataIndexColumnDefinition existingColumn = uniqueOrders[indexColumn.ColumnOrder];
                                    throw new Exception(string.Format("Ошибка инициализации столбца {0} индекса {1}. Индекс уже содержит столбец {2} с тем же порядковым номером {3}. Порядковые номера должны отличаться для каждого столбца.",
                                        indexColumn.ColumnName, this.RelativeName, existingColumn.ColumnName, existingColumn.ColumnOrder));
                                }
                                _IndexColumns.Add(indexColumn);
                            }

                            //сортируем индексы по порядковым номерам столбцов по возрастанию.
                            _IndexColumns.Sort(delegate(MetadataIndexColumnDefinition x, MetadataIndexColumnDefinition y)
                            {
                                return x.ColumnOrder.CompareTo(y.ColumnOrder);
                            });

                            __init_IndexColumns = true;
                        }
                    }
                }
                return _IndexColumns;
            }
        }

        private bool __init_InitializingColumns = false;
        private List<MetadataIndexColumnDefinition> _InitializingColumns;
        /// <summary>
        /// Индексируемые столбцы заданные при инициализации.
        /// </summary>
        internal List<MetadataIndexColumnDefinition> InitializingColumns
        {
            get
            {
                if (!__init_InitializingColumns)
                {
                    lock (this)
                    {
                        if (!__init_InitializingColumns)
                        {
                            _InitializingColumns = new List<MetadataIndexColumnDefinition>();
                            __init_InitializingColumns = true;
                        }
                    }
                }
                return _InitializingColumns;
            }
        }
    }
}
