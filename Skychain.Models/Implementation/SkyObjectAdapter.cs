using Skychain.Models.Entity;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Skychain.Models.Implementation
{
    /// <summary>
    /// Представляет адаптер объектов системы.
    /// </summary>
    /// <typeparam name="TObject">Тип объекта системы.</typeparam>
    /// <typeparam name="TEntity">Тип сохраняемого объекта.</typeparam>
    /// <typeparam name="IObject">Тип интерфейса объекта.</typeparam>
    public class SkyObjectAdapter<TObject, TEntity, IObject> : ISkyObjectAdapter<IObject, TEntity>
        where TEntity : SkyEntity
        where TObject : SkyObject<TObject, TEntity, IObject>, IObject
        where IObject : ISkyObject
    {
        internal SkyObjectAdapter(SkyContext context)
        {
            if (context == null)
                throw new ArgumentNullException("context");

            this.Type = typeof(TObject);
            this.Context = context;
        }

        /// <summary>
        /// Тип объекта.
        /// </summary>
        public Type Type { get; private set; }

        /// <summary>
        /// Контекст системы.
        /// </summary>
        public SkyContext Context { get; private set; }


        private bool __init_Constructor = false;
        private ConstructorInfo _Constructor;
        /// <summary>
        /// Конструктор объекта.
        /// </summary>
        private ConstructorInfo Constructor
        {
            get
            {
                if (!__init_Constructor)
                {
                    Type[] ctorParameterTypes = new Type[] { typeof(TEntity), typeof(SkyObjectAdapter<TObject, TEntity, IObject>) };
                    _Constructor = this.Type.GetConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, ctorParameterTypes, null);
                    if (_Constructor == null)
                        throw new Exception(string.Format("Failed to get the object constructor of type {0}.", this.Type.FullName));

                    __init_Constructor = true;
                }
                return _Constructor;
            }
        }


        private bool __init_EntityConstructor = false;
        private ConstructorInfo _EntityConstructor;
        /// <summary>
        /// Конструктор сохраняемого объекта.
        /// </summary>
        private ConstructorInfo EntityConstructor
        {
            get
            {
                if (!__init_EntityConstructor)
                {
                    _EntityConstructor = typeof(TEntity).GetConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, Type.EmptyTypes, null);
                    if (_EntityConstructor == null)
                        throw new Exception(string.Format("Failed to get the object constructor of type {0}.", typeof(TEntity).FullName));

                    __init_EntityConstructor = true;
                }
                return _EntityConstructor;
            }
        }


        /// <summary>
        /// Создаёт новый экземпляр сохраняемого объекта.
        /// </summary>
        internal TEntity CreateEntity()
        {
            return (TEntity)this.EntityConstructor.Invoke(null);
        }

        /// <summary>
        /// Создаёт новый экземпляр объекта на основе сохраняемых данных.
        /// </summary>
        /// <param name="entity">Сохраняемые данные. При отсутствии данных создаётся экземпляр, соответствующий новому объекту.</param>
        private TObject CreateObject(TEntity entity)
        {
            TObject skyObject = (TObject)this.Constructor.Invoke(new object[] { entity, this });
            return skyObject;
        }

        /// <summary>
        /// Создаёт новый объект, не сохранённый в базу данных.
        /// </summary>
        public TObject CreateObject()
        {
            return this.CreateObject(null);
        }


        private bool __init_ObjectsByID = false;
        private Dictionary<int, TObject> _ObjectsByID;
        /// <summary>
        /// Словарь объектов системы по идентификаторам объектов.
        /// </summary>
        private Dictionary<int, TObject> ObjectsByID
        {
            get
            {
                if (!__init_ObjectsByID)
                {
                    _ObjectsByID = new Dictionary<int, TObject>();
                    __init_ObjectsByID = true;
                }
                return _ObjectsByID;
            }
        }



        /// <summary>
        /// Инициализирует существующий объект по сохраняемым данным.
        /// </summary>
        /// <param name="entity">Объект сохраняемых данных.</param>
        private TObject Initialize(TEntity entity)
        {
            if (entity == null)
                throw new ArgumentNullException("entity");
            if (entity.ID == 0)
                throw new Exception("Entity is not exists.");

            //получаем новый или существующий экземпляр объекта.
            TObject skyObject = null;
            if (!this.ObjectsByID.ContainsKey(entity.ID))
            {
                skyObject = this.CreateObject(entity);
                this.ObjectsByID.Add(entity.ID, skyObject);
            }
            else
                skyObject = this.ObjectsByID[entity.ID];

            //ругаемся при отсутствии экземпляра.
            if (skyObject == null)
                throw new Exception(string.Format("Failed to get object by ID={0}.", entity.ID));

            //возвращаем инициализированный объект.
            return skyObject;
        }


        /// <summary>
        /// Возвращает объект системы по идентификатору сохраняемого объекта.
        /// </summary>
        /// <param name="entityID">Идентификатор сохраняемого объекта.</param>
        /// <param name="throwNotFoundException">При установленном параметре true генерирует исключение в случае отсутствия экземпляра.</param>
        public TObject GetObject(int entityID, bool throwNotFoundException)
        {
            if (entityID == 0)
                throw new ArgumentNullException("id");

            //получаем новый или существующий экземпляр.
            TObject skyObject = null;
            if (!this.ObjectsByID.ContainsKey(entityID))
                skyObject = this.GetObject(dbSet => dbSet.Find(entityID));
            else
                skyObject = this.ObjectsByID[entityID];

            //ругаемся при отсутствии экземпляра.
            if (throwNotFoundException && skyObject == null)
                throw new Exception(string.Format("Object of type {0} with ID={1} is not exists.", this.Type.FullName, entityID));

            //возвращаем экземпляр.
            return skyObject;
        }


        /// <summary>
        /// Добавляет только что созданный объект в словарь существующих экземпляров объектов.
        /// </summary>
        /// <param name="skyObject">Добавляемый экземпляр.</param>
        internal void AddCreatedObject(TObject skyObject)
        {
            if (skyObject == null)
                throw new ArgumentNullException("skyObject");
            skyObject.CheckExists();

            if (!this.ObjectsByID.ContainsKey(skyObject.ID))
                this.ObjectsByID.Add(skyObject.ID, skyObject);
        }

        /// <summary>
        /// Удаляет удалённый объект из словаря существующих экземпляров.
        /// </summary>
        /// <param name="entityID">Идентификатор удаляемого объекта.</param>
        internal void RemoveDeletedObject(int entityID)
        {
            if (entityID == 0)
                throw new ArgumentNullException("entityID");

            //получаем удалённый объект.
            TObject skyObject = null;
            if (this.ObjectsByID.ContainsKey(entityID))
                skyObject = this.ObjectsByID[entityID];

            //ругаемся при отсутствии экземпляра.
            if (skyObject == null)
                throw new Exception(string.Format("Object of type {0} with ID={1} is not exists.",
                    this.Type.FullName, entityID));

            //проверяем, что объект был действительно удалён.
            if (!skyObject.Deleted)
                throw new Exception(string.Format("Cannot remove object of type {0} with ID={1} because it is not deleted.",
                    this.Type.FullName, entityID));

            //удаляем объект из словаря объектов.
            this.ObjectsByID.Remove(entityID);
        }

        /// <summary>
        /// Возвращает все объекты данного типа.
        /// </summary>
        public IEnumerable<TObject> GetObjects()
        {
            return this.GetObjects(entitySet => entitySet.ToArray());
        }


        /// <summary>
        /// Возвращает объект данного типа по сохранённому объекту, полученному из соответсвующего набора данных.
        /// </summary>
        /// <param name="entityResolver">Метод, возвращающий сохранённый объект по набору данных.</param>
        public TObject GetObject(Func<DbSet<TEntity>, TEntity> entityResolver)
        {
            if (entityResolver == null)
                throw new ArgumentNullException("queryAction");

            TObject skyObject = null;
            this.Context.ObjectAdapters.ExecuteQuery(context =>
            {
                TEntity entity = entityResolver(context.Set<TEntity>());
                if (entity != null)
                    skyObject = this.Initialize(entity);
            });
            return skyObject;
        }

        /// <summary>
        /// Возвращает объекты данного типа для массива сохранённых объектов, полученного из соответсвующего набора данных.
        /// </summary>
        /// <param name="entitiesResolver">Метод, возвращающий массив сохранённых объектов по набору данных.</param>
        public IEnumerable<TObject> GetObjects(Func<DbSet<TEntity>, IEnumerable<TEntity>> entitiesResolver)
        {
            if (entitiesResolver == null)
                throw new ArgumentNullException("queryAction");

            IEnumerable<TObject> skyObjects = null;
            this.Context.ObjectAdapters.ExecuteQuery(context =>
            {
                IEnumerable<TEntity> entities = entitiesResolver(context.Set<TEntity>());
                if (entities == null)
                    entities = new TEntity[] { };

                //выполняем приведение к массиву, 
                //чтобы экземпляры были выбраны из базы данных и проинициализированы в текущий момент.
                skyObjects = new ReadOnlyCollection<TObject>(entities.Select(x => this.Initialize(x)).ToList());
            });
            return skyObjects;
        }

        /// <summary>
        /// Выполняет команду в контексте подключения к набору данных и возвращает результат заданного типа.
        /// </summary>
        /// <typeparam name="TResult">Тип возвращаемого результата.</typeparam>
        /// <param name="entitySetCommand">Команда, выполняющая запрос к набору данных и возвращающая результат.</param>
        public TResult ExecuteQuery<TResult>(Func<DbSet<TEntity>, TResult> entitySetCommand)
        {
            if (entitySetCommand == null)
                throw new ArgumentNullException("entitySetCommand");

            TResult result = default(TResult);
            this.Context.ObjectAdapters.ExecuteQuery(context =>
            {
                result = entitySetCommand(context.Set<TEntity>());
            });
            return result;
        }

        /// <summary>
        /// Выполняет команду в контексте подключения к набору данных и возвращает результат заданного типа.
        /// </summary>
        /// <typeparam name="TResult">Тип возвращаемого результата.</typeparam>
        /// <param name="entitySetCommand">Команда, выполняющая запрос к набору данных и возвращающая результат.</param>
        public TResult ExecuteQuery<TResult>(Func<DbSet<TEntity>, SkyEntityContext, TResult> entitySetCommand)
        {
            if (entitySetCommand == null)
                throw new ArgumentNullException("entitySetCommand");

            TResult result = default(TResult);
            this.Context.ObjectAdapters.ExecuteQuery(context =>
            {
                result = entitySetCommand(context.Set<TEntity>(), context);
            });
            return result;
        }

        /// <summary>
        /// Выполняет команду в контексте подключения к набору данных.
        /// </summary>
        /// <param name="entitySetCommand">Команда, выполняющая запрос к набору данных.</param>
        public void ExecuteQuery(Action<DbSet<TEntity>, SkyEntityContext> entitySetCommand)
        {
            if (entitySetCommand == null)
                throw new ArgumentNullException("entitySetCommand");

            this.Context.ObjectAdapters.ExecuteQuery(context =>
            {
                entitySetCommand(context.Set<TEntity>(), context);
            });
        }


        ISkyContext ISkyObjectAdapter<IObject, TEntity>.Context => this.Context;

        IObject ISkyObjectAdapter<IObject, TEntity>.GetObject(int entityID)
        {
            return this.GetObject(entityID, false);
        }

        IObject ISkyObjectAdapter<IObject, TEntity>.GetObject(int entityID, bool throwNotFoundException)
        {
            return this.GetObject(entityID, throwNotFoundException);
        }

        IObject ISkyObjectAdapter<IObject, TEntity>.GetObject(Func<DbSet<TEntity>, TEntity> entityResolver)
        {
            return this.GetObject(entityResolver);
        }

        IEnumerable<IObject> ISkyObjectAdapter<IObject, TEntity>.GetObjects()
        {
            return this.GetObjects();
        }

        IEnumerable<IObject> ISkyObjectAdapter<IObject, TEntity>.GetObjects(Func<DbSet<TEntity>, IEnumerable<TEntity>> entitiesResolver)
        {
            return this.GetObjects(entitiesResolver);
        }

    }

}
