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
    /// Определение типа метаданных, сохраняемого в БД.
    /// </summary>
    public class MetadataTypeDefinition
    {
        /// <summary>
        /// К-тор.
        /// </summary>
        /// <param name="type">Тип метаданных.</param>
        internal MetadataTypeDefinition(Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            this.Type = type;
            this.CheckType();
        }

        private Type _Type;
        /// <summary>
        /// Тип метаданных.
        /// </summary>
        public Type Type
        {
            get { return _Type; }
            private set { _Type = value; }
        }

        private bool __init_TypeUniqueKey = false;
        private string _TypeUniqueKey;
        /// <summary>
        /// Уникальный ключ типа.
        /// </summary>
        internal string TypeUniqueKey
        {
            get
            {
                if (!__init_TypeUniqueKey)
                {
                    lock (this)
                    {
                        if (!__init_TypeUniqueKey)
                        {
                            _TypeUniqueKey = this.Type.AssemblyQualifiedName;
                            __init_TypeUniqueKey = true;
                        }
                    }
                }
                return _TypeUniqueKey;
            }
        }

        /// <summary>
        /// Проверяет корректность определения типа метаданных.
        /// </summary>
        private void CheckType()
        {
            //проверка наличия атрибута MetadataClassAttribute
            object checkObj = this.ClassAttribute;

            //проверка наличия имени таблицы
            checkObj = this.TableName;

            //проверка наличия колонки идентификатора и проверка таблицы в базе данных
            checkObj = this.IdentityProperty;

            //проверка индексов
            checkObj = this.Indexes;            
        }

        private bool __init_ClassAttribute = false;
        private MetadataClassAttribute _ClassAttribute;
        /// <summary>
        /// Атрибут определения класса.
        /// </summary>
        private MetadataClassAttribute ClassAttribute
        {
            get
            {
                if (!__init_ClassAttribute)
                {
                    lock (this)
                    {
                        if (!__init_ClassAttribute)
                        {

                            object[] classAttributes = this.Type.GetCustomAttributes(typeof(MetadataClassAttribute), true);
                            if (classAttributes.Length > 0)
                                _ClassAttribute = (MetadataClassAttribute)classAttributes[0];

                            if (_ClassAttribute == null)
                                throw new Exception(string.Format("Тип {0} должен быть помечен атрибутом MetadataClassAttribute", this.Type.FullName));

                            __init_ClassAttribute = true;
                        }
                    }
                }
                return _ClassAttribute;
            }
        }


        #region Name

        private bool __init_TableName = false;
        private string _TableName;
        /// <summary>
        /// Название таблицы метаданных.
        /// </summary>
        public string TableName
        {
            get
            {
                if (!__init_TableName)
                {
                    lock (this)
                    {
                        if (!__init_TableName)
                        {
                            _TableName = this.ClassAttribute.TableName;
                            if (string.IsNullOrEmpty(_TableName))
                                throw new Exception(string.Format("Не задано название таблицы метаданных в определении типа {0}.", this.Type.FullName));
                            __init_TableName = true;
                        }
                    }
                }
                return _TableName;
            }
        }

        private bool __init_TablePrefix = false;
        private string _TablePrefix;
        /// <summary>
        /// 
        /// </summary>
        public string TablePrefix
        {
            get
            {
                if (!__init_TablePrefix)
                {
                    lock (this)
                    {
                        if (!__init_TablePrefix)
                        {
                            _TablePrefix = this.ClassAttribute.TablePrefix;
                            if (string.IsNullOrEmpty(_TablePrefix))
                                throw new Exception(string.Format("Не задан префикс таблицы в определении типа {0}.", this.Type.FullName));

                            if (string.IsNullOrEmpty(_TablePrefix))
                                _TablePrefix = this.TableName;

                            __init_TablePrefix = true;
                        }
                    }
                }
                return _TablePrefix;
            }
        }

        private bool __init_PartitionTableName = false;
        private string _PartitionTableName;
        /// <summary>
        /// 
        /// </summary>
        public string PartitionTableName
        {
            get
            {
                if (!__init_PartitionTableName)
                {
                    lock (this)
                    {
                        if (!__init_PartitionTableName)
                        {
                            _PartitionTableName = this.ClassAttribute.PartitionTableName;
                            if (string.IsNullOrEmpty(_PartitionTableName))
                                throw new Exception(string.Format("Не задано название таблицы для некорневого раздела данных в определении типа {0}.", this.Type.FullName));
                            __init_PartitionTableName = true;
                        }
                    }
                }
                return _PartitionTableName;
            }
        }

        #endregion


        #region Metadata properties

        private bool __init_IdentityProperty = false;
        private MetadataPropertyDefinition _IdentityProperty;
        /// <summary>
        /// Инкрементный столбец-идентификатор таблицы.
        /// </summary>
        public MetadataPropertyDefinition IdentityProperty
        {
            get
            {
                if (!__init_IdentityProperty)
                {
                    lock (this)
                    {
                        if (!__init_IdentityProperty)
                        {
                            foreach (MetadataPropertyDefinition property in this.AllMetadataProperties)
                            {
                                if (property.IsIdentity)
                                {
                                    if (_IdentityProperty == null)
                                        _IdentityProperty = property;
                                    else
                                        throw new Exception(string.Format("Определение типа {0} может содержать только одно свойство идентификатор.", this.Type.FullName));
                                }
                            }
                            //проверяем наличие столбца-идентификатора
                            if (_IdentityProperty == null)
                                throw new Exception(string.Format("Определение типа '{0}' не содержит свойство идентификатор. Пометьте одно из свойств типа атрибутом [property(IsIdentity=true)].", this.Type.FullName));

                            //проверяем тип столбца-идентификатора.
                            if (_IdentityProperty.Property.PropertyType != typeof(int))
                                throw new Exception(string.Format("Столбец-идентификатор {0} типа {1} определения типа {2} должен быть типа int.",
                                    _IdentityProperty.ColumnName, _IdentityProperty.Property.PropertyType.FullName, this.Type.FullName));

                            __init_IdentityProperty = true;
                        }
                    }
                }
                return _IdentityProperty;
            }
        }

        private bool __init_AllMetadataProperties = false;
        private DBCollection<MetadataPropertyDefinition> _AllMetadataProperties;
        /// <summary>
        /// Коллекция всех свойств типа метаданных.
        /// </summary>
        public DBCollection<MetadataPropertyDefinition> AllMetadataProperties
        {
            get
            {
                if (!__init_AllMetadataProperties)
                {
                    lock (this)
                    {
                        if (!__init_AllMetadataProperties)
                        {
                            _AllMetadataProperties = new DBCollection<MetadataPropertyDefinition>();
                                                       
                            Dictionary<string, bool> uniqueColumnNames = new Dictionary<string, bool>();
                            PropertyInfo[] allProps = this.Type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                            Type metaAttrType = typeof(MetadataPropertyAttribute);
                            foreach (PropertyInfo propInfo in allProps)
                            {
                                object[] metaAttrs = propInfo.GetCustomAttributes(metaAttrType, true);
                                if (metaAttrs != null && metaAttrs.Length > 0)
                                {
                                    MetadataPropertyAttribute metaAttr = (MetadataPropertyAttribute)metaAttrs[0];
                                    MetadataPropertyDefinition property = new MetadataPropertyDefinition(propInfo);
                                    property.UpdateMode = metaAttr.UpdateMode;

                                    //инициализируем название столбца.
                                    if (!string.IsNullOrEmpty(metaAttr.ColumnName))
                                        property.ColumnName = metaAttr.ColumnName;
                                    else
                                        property.ColumnName = propInfo.Name;

                                    property.ParameterName = string.Format("@{0}", property.ColumnName);

                                    if (!uniqueColumnNames.ContainsKey(property.ColumnName.ToLower()))
                                        uniqueColumnNames.Add(property.ColumnName.ToLower(), true);
                                    else
                                        throw new Exception(string.Format("Определение класса {0} уже содержит столбец {1}.", this.Type.FullName, property.ColumnName));                                    

                                    //Тип данных
                                    Type propertyType = propInfo.PropertyType;
                                    if (metaAttr.SqlTypeDefined)
                                        property.SqlType = metaAttr.SqlType;
                                    else if (propertyType == typeof(string))
                                    {
                                        property.SqlType = SqlDbType.NVarChar;
                                        property.Size = 250;
                                    }
                                    else if (propertyType == typeof(int) || propertyType == typeof(int?))
                                        property.SqlType = SqlDbType.Int;
                                    else if (propertyType == typeof(long) || propertyType == typeof(long?))
                                        property.SqlType = SqlDbType.BigInt;
                                    else if (propertyType == typeof(Guid) || propertyType == typeof(Guid?))
                                        property.SqlType = SqlDbType.UniqueIdentifier;
                                    else if (propertyType == typeof(bool) || propertyType == typeof(bool?))
                                        property.SqlType = SqlDbType.Bit;
                                    else if (propertyType == typeof(DateTime) || propertyType == typeof(DateTime?))
                                        property.SqlType = SqlDbType.DateTime;
                                    else if (propertyType == typeof(byte[]))
                                    {
                                        property.SqlType = SqlDbType.VarBinary;
                                        property.Size = -1;
                                    }
                                    else if (propertyType == typeof(double) || propertyType == typeof(double?))
                                        property.SqlType = SqlDbType.Float;
                                    else if (propertyType.IsEnum)
                                        property.SqlType = SqlDbType.Int;

                                    //признак столбца-идентификатора
                                    property.IsIdentity = metaAttr.IsIdentity;

                                    //размер
                                    if (metaAttr.Size > 0 || metaAttr.Size == -1)
                                        property.Size = metaAttr.Size;

                                    //изменяем устаревшие тип ntext, text и image на nvarchar(max), nvarchar(max) и varbinary(max).
                                    if (property.SqlType == SqlDbType.NText ||
                                        property.SqlType == SqlDbType.Text ||
                                        property.SqlType == SqlDbType.Image)
                                    {
                                        if (property.SqlType == SqlDbType.NText)
                                            property.SqlType = SqlDbType.NVarChar;

                                        else if (property.SqlType == SqlDbType.Text)
                                            property.SqlType = SqlDbType.VarChar;

                                        else if (property.SqlType == SqlDbType.Image)
                                            property.SqlType = SqlDbType.VarBinary;

                                        property.Size = -1;
                                    }
                                   
                                    //индексированный столбец
                                    property.Indexed = metaAttr.Indexed;                                   

                                    //возможность нулевых значений
                                    if (!property.IsIdentity)
                                        property.IsNullable = metaAttr.IsNullable;
                                    else
                                        property.IsNullable = false;

                                    //проверяем, что столбец типа SqlDbType.Timestamp не нулевой.
                                    if (property.SqlType == SqlDbType.Timestamp && property.IsNullable)
                                        throw new Exception(string.Format("Столбец [{0}] типа {1} должен быть помечен как ненулевой.", property.ColumnName, property.SqlType));

                                    _AllMetadataProperties.Add(property);
                                }
                            }                           

                            __init_AllMetadataProperties = true;
                        }
                    }
                }
                return _AllMetadataProperties;
            }
        }

        private bool __init_PropertiesByName = false;
        private Dictionary<string, MetadataPropertyDefinition> _PropertiesByName;
        /// <summary>
        /// Словать свойств метаданных по названию.
        /// </summary>
        private Dictionary<string, MetadataPropertyDefinition> PropertiesByName
        {
            get
            {
                if (!__init_PropertiesByName)
                {
                    lock (this)
                    {
                        if (!__init_PropertiesByName)
                        {
                            _PropertiesByName = new Dictionary<string, MetadataPropertyDefinition>();
                            foreach (MetadataPropertyDefinition classProperty in this.AllMetadataProperties)
                            {
                                string propName = classProperty.Property.Name.ToLower();
                                if (!_PropertiesByName.ContainsKey(propName))
                                    _PropertiesByName.Add(propName, classProperty);
                            }
                            __init_PropertiesByName = true;
                        }
                    }
                }
                return _PropertiesByName;
            }
        }

        /// <summary>
        /// Возвращает результат наличия свойства метаданных.
        /// </summary>
        /// <param name="propertyName">Навзвание свойства.</param>
        /// <returns>true, если свойство присутствует в определении класса.</returns>
        public bool ContainsProperty(string propertyName)
        {
            return this.ContainsProperty(propertyName, false);
        }

        /// <summary>
        /// Возвращает результат наличия свойства метаданных либо генерирует исключение.
        /// </summary>
        /// <param name="propertyName">Навзвание свойства.</param>
        /// <param name="throwNotFoundException">Если true, при отсутствии свойства сгенерируется исключение.</param>
        /// <returns>true, если свойство присутствует в определении класса.</returns>
        public bool ContainsProperty(string propertyName, bool throwNotFoundException)
        {
            if (string.IsNullOrEmpty(propertyName))
                throw new ArgumentNullException("propertyName");
            string propertyNameLow = propertyName.ToLower();
            bool result = this.PropertiesByName.ContainsKey(propertyNameLow);

            if (!result && throwNotFoundException)
                throw new Exception(string.Format("Определение типа сохраняемого объекта {0} не содержит свойство {1}.", this.Type.FullName, propertyName));

            return result;
        }

        private bool __init_ParameterProperties = false;
        private DBCollection<MetadataPropertyDefinition> _ParameterProperties;
        /// <summary>
        /// Коллекция свойств типа метаданных, значения которых передаются в качестве параметров при вставке или обновлении строки соответствующей таблицы метаданных.
        /// </summary>
        public DBCollection<MetadataPropertyDefinition> ParameterProperties
        {
            get
            {
                if (!__init_ParameterProperties)
                {
                    lock (this)
                    {
                        if (!__init_ParameterProperties)
                        {
                            _ParameterProperties = this.AllMetadataProperties.FindAll(new Predicate<MetadataPropertyDefinition>(delegate(MetadataPropertyDefinition property)
                            {
                                return (property.UpdateMode == MetadataPropertyUpdateMode.OnInsert || property.UpdateMode == MetadataPropertyUpdateMode.OnInsertAndUpdate);
                            }));
                            __init_ParameterProperties = true;
                        }
                    }
                }
                return _ParameterProperties;
            }
        }

        #endregion


        #region Interface implimintations

        private bool __init_IsMetadataObject = false;
        private bool _IsMetadataObject;
        /// <summary>
        /// Возвращает true, если объект реализует интерфейс IMetadataObject.
        /// </summary>
        public bool IsMetadataObject
        {
            get
            {
                if (!__init_IsMetadataObject)
                {
                    lock (this)
                    {
                        if (!__init_IsMetadataObject)
                        {
                            Type metadataObjectInterface = typeof(IMetadataObject);
                            _IsMetadataObject = metadataObjectInterface.IsAssignableFrom(this.Type);
                            __init_IsMetadataObject = true;
                        }
                    }
                }
                return _IsMetadataObject;
            }
        }

        internal void CheckIsMetadataObject()
        {
            if (!this.IsMetadataObject)
                throw new Exception(string.Format("Тип объекта, сохраняемого в базу данных {0}, не реализует интерфейс IMetadataObject.", this.Type.FullName));
        }        

        #endregion
                

        #region Indexes

        private bool __init_Indexes = false;
        private List<MetadataIndexDefinition> _Indexes;
        /// <summary>
        /// Индексы основной таблицы объектов.
        /// </summary>
        public List<MetadataIndexDefinition> Indexes
        {
            get
            {
                if (!__init_Indexes)
                {
                    lock (this)
                    {
                        if (!__init_Indexes)
                        {
                            _Indexes = this.GetIndexes();
                            __init_Indexes = true;
                        }
                    }
                }
                return _Indexes;
            }
        }

        private List<MetadataIndexDefinition> GetIndexes()
        {
            List<MetadataIndexDefinition> indexes = new List<MetadataIndexDefinition>();
            try
            {
                Dictionary<string, MetadataIndexDefinition> uniqueIndexes = new Dictionary<string, MetadataIndexDefinition>();
                Type indexAttrType = typeof(MetadataIndexAttribute);
                foreach (MetadataPropertyDefinition property in this.AllMetadataProperties)
                {
                    //если включена индексация по умолчанию, то добавляем индекс.
                    bool indexedByColumn = property.Indexed;
                    if (indexedByColumn)
                    {
                        string defaultIndexKey = property.ColumnName.ToLower();
                        if (uniqueIndexes.ContainsKey(defaultIndexKey))
                            throw new Exception(string.Format("Индекс по умолчанию с ключом {0} уже добавлен в коллеккцию индексов.", defaultIndexKey));

                        //добавляем индекс. В качестве имени столбца берем название свойства, т.к. имя может состоять из нескольких слов на русском.
                        MetadataIndexDefinition columnIndex = new MetadataIndexDefinition(property.Property.Name, this);
                        indexes.Add(columnIndex);
                        uniqueIndexes.Add(defaultIndexKey, columnIndex);

                        //устанавливаем, что индекс является дефалтовым для одного столбца
                        columnIndex.CreatedByColumn = true;

                        //добавляем столбец к индексу.
                        MetadataIndexColumnDefinition indexColumn = new MetadataIndexColumnDefinition(property, false, 0);
                        columnIndex.InitializingColumns.Add(indexColumn);
                    }

                    //добавляем индексы по атрибуту DBIndex
                    object[] attrs = property.Property.GetCustomAttributes(indexAttrType, false);
                    if (attrs.Length > 0)
                    {
                        foreach (MetadataIndexAttribute indexAttr in attrs)
                        {
                            if (indexAttr == null)
                                throw new Exception(string.Format("Не удалось получить DBIndexAttribute для свойства {0}.", property.Property.Name));                            

                            if (string.IsNullOrEmpty(indexAttr.RelativeName))
                                throw new Exception(string.Format("Не задано имя индекса в свойстве атрибута DBIndexAttribute.RelativeName для свойства {0}.", property.Property.Name));

                            MetadataIndexDefinition index = null;
                            string indexKey = indexAttr.RelativeName.ToLower();
                            if (uniqueIndexes.ContainsKey(indexKey))
                            {
                                index = uniqueIndexes[indexKey];
                                if (index.CreatedByColumn)
                                    throw new Exception(string.Format("Индекс {0} уже добавлен в коллекцию индексов при помощи атрибута Indexed.", index.RelativeName));
                            }
                            else
                            {
                                index = new MetadataIndexDefinition(indexAttr.RelativeName, this);
                                indexes.Add(index);
                                uniqueIndexes.Add(indexKey, index);
                            }
                            if (index == null)
                                throw new Exception(string.Format("Не удалось инициализировать индекс по ключу {0}.", indexKey));

                            //добавляем столбец к индексу.
                            MetadataIndexColumnDefinition indexColumn = new MetadataIndexColumnDefinition(property, indexAttr.IsDescending, indexAttr.ColumnOrder);
                            index.InitializingColumns.Add(indexColumn);
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Ошибка при формировании коллекции индексов класса {0}.", this.Type.FullName), ex);
            }

            return indexes;
        }

        #endregion


        #region Triggers

        private bool __init_TriggerTypeConstructors = false;
        private List<ConstructorInfo> _TriggerTypeConstructors;
        /// <summary>
        /// Типы триггеров основной таблицы объектов.
        /// </summary>
        public List<ConstructorInfo> TriggerTypeConstructors
        {
            get
            {
                if (!__init_TriggerTypeConstructors)
                {
                    lock (this)
                    {
                        if (!__init_TriggerTypeConstructors)
                        {
                            _TriggerTypeConstructors = new List<ConstructorInfo>();
                            Type[] triggerTypes = this.ClassAttribute.TriggerTypes;
                            if (triggerTypes != null)
                            {
                                Type baseTriggerType = typeof(DBTriggerSchema);
                                foreach (Type triggerType in triggerTypes)
                                {
                                    if (!triggerType.IsSubclassOf(baseTriggerType))
                                        throw new Exception(string.Format("Тип триггера {0} не является наследником базового типа триггера {1}.",
                                            triggerType.FullName, baseTriggerType.FullName));

                                    //получаем конструктор триггера.
                                    ConstructorInfo triggerConstructor = null;

                                    //получаем конструктор DBObject-типа.
                                    if (triggerConstructor == null)
                                        triggerConstructor = triggerType.GetConstructor(
                                            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(DBObjectTableSchemaAdapter) }, null);

                                    //получаем конструктор базового типа.
                                    if (triggerConstructor == null)
                                        triggerConstructor = triggerType.GetConstructor(
                                        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(DBTableSchemaAdapter) }, null);

                                    if (triggerConstructor == null)
                                        throw new Exception(string.Format("Не удалось получить конструктор триггера типа {0}.", triggerType.FullName));

                                    _TriggerTypeConstructors.Add(triggerConstructor);
                                }
                            }
                            __init_TriggerTypeConstructors = true;
                        }
                    }
                }
                return _TriggerTypeConstructors;
            }
        }

        #endregion
    }
}
