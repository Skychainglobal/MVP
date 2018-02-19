using Skychain.Models.Entity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skychain.Models.Implementation
{
    /// <summary>
    /// Представляет базовый тип объекта системы.
    /// </summary>
    /// <typeparam name="TObject">Тип объекта системы.</typeparam>
    /// <typeparam name="TEntity">Тип сохраняемого объекта.</typeparam>
    /// <typeparam name="IObject">Тип интерфейса объекта.</typeparam>
    public abstract class SkyObject<TObject, TEntity, IObject> : ISkyObject
        where TEntity : SkyEntity
        where TObject : SkyObject<TObject, TEntity, IObject>, IObject
        where IObject : ISkyObject
    {
        internal SkyObject(TEntity entity, SkyObjectAdapter<TObject, TEntity, IObject> adapter)
        {
            if (adapter == null)
                throw new ArgumentNullException("adapter");
            if (entity == null)
                entity = adapter.CreateEntity();

            this.Entity = entity;
            this.Adapter = adapter;
        }
        

        /// <summary>
        /// Сохраняемые данные экземпляра класса.
        /// </summary>
        protected TEntity Entity { get; private set; }


        /// <summary>
        /// Адаптер объектов данного типа.
        /// </summary>
        private SkyObjectAdapter<TObject, TEntity, IObject> Adapter { get; set; }


        /// <summary>
        /// Контекст системы.
        /// </summary>
        public SkyContext Context
        {
            get { return this.Adapter.Context; }
        }

        /// <summary>
        /// Тип объекта.
        /// </summary>
        internal Type InstanceType
        {
            get { return this.Adapter.Type; }
        }

        /// <summary>
        /// Уникальный идентификатор объекта в базе данных.
        /// </summary>
        public int ID
        {
            get { return this.Entity.ID; }
        }


        /// <summary>
        /// Возвращает true, если объект является новым и не был сохранён в базу данных.
        /// </summary>
        public bool IsNew
        {
            get { return this.Entity.ID == 0; }
        }


        /// <summary>
        /// Номер версии объекта, увеличиваемый при изменении любого из свойств набора данных.
        /// </summary>
        public int VersionNumber
        {
            get { return this.Entity.VersionNumber; }
            private set { this.Entity.VersionNumber = value; }
        }

        /// <summary>
        /// Дата и время сохранения нового объекта.
        /// </summary>
        public DateTime TimeCreated
        {
            get
            {
                return this.Entity.TimeCreated.HasValue ?
                  this.Entity.TimeCreated.Value : DateTime.MinValue;
            }
            private set
            {
                if (value != DateTime.MinValue)
                    this.Entity.TimeCreated = value;
                else
                    this.Entity.TimeCreated = null;
            }
        }

        /// <summary>
        /// Дата и время последнего изменения свойств объекта.
        /// </summary>
        public DateTime TimeModified
        {
            get
            {
                return this.Entity.TimeModified.HasValue ?
                  this.Entity.TimeModified.Value : DateTime.MinValue;
            }
            private set
            {
                if (value != DateTime.MinValue)
                    this.Entity.TimeModified = value;
                else
                    this.Entity.TimeModified = null;
            }
        }

        /// <summary>
        /// Возвращает true, если экземпляр объекта был создан в текущем контексте выполнения кода.
        /// </summary>
        public bool JustCreated { get; private set; }

        /// <summary>
        /// Обновляет объект в базе данных.
        /// </summary>
        public virtual void Update()
        {
            //проверяем, что объект не был удалён.
            if (this.Deleted)
                throw new Exception(string.Format("Updating a deleted object of type {0} is forbidden.", this.InstanceType.FullName));

            this.Context.ObjectAdapters.ExecuteQuery((SkyEntityContext context) =>
            {
                //изменяем свойства при сохранении.
                this.VersionNumber++;
                DateTime now = DateTime.Now;
                this.TimeModified = now;

                //добавляем или изменяем объект.
                if (this.IsNew)
                {
                    this.TimeCreated = now;
                    context.Set<TEntity>().Add(this.Entity);
                    this.JustCreated = true;
                }
                else
                {
                    context.Set<TEntity>().Attach(this.Entity);
                    context.Entry(this.Entity).State = System.Data.Entity.EntityState.Modified;
                }

                //сохраняем изменения.
                context.SaveChanges();
            });

            //добавляем новый экземпляр в словарь.
            if (this.JustCreated)
                this.Adapter.AddCreatedObject((TObject)this);
        }

        /// <summary>
        /// Удаляет объект из базы данных.
        /// </summary>
        public virtual void Delete()
        {
            //проверяем, что объект существует.
            this.CheckExists();

            //удаляем объект.
            this.Context.ObjectAdapters.ExecuteQuery((SkyEntityContext context) =>
            {
                //удаляем объект.
                context.Set<TEntity>().Attach(this.Entity);
                context.Set<TEntity>().Remove(this.Entity);

                //сохраняем изменения.
                context.SaveChanges();
            });

            //устанавливаем признак удалённого объекта.
            this.Deleted = true;

            //удаляем объект из словаря объектов.
            this.Adapter.RemoveDeletedObject(this.ID);
        }

        /// <summary>
        /// Возвращает true, если объект был удалён в контексте выполнения кода.
        /// </summary>
        public bool Deleted { get; private set; }


        /// <summary>
        /// Удаляет коллекцию дочерних объектов заданного типа.
        /// </summary>
        /// <typeparam name="TChild">Тип дочернего объекта.</typeparam>
        /// <param name="children">Коллекия дочерних объекто.</param>
        internal void DeleteChildren<TChild>(IEnumerable<TChild> children)
            where TChild : ISkyObject
        {
            if (children == null)
                throw new ArgumentNullException("children");

            //выходим при отсутствии дочерних объектов.
            if (!children.Any())
                return;

            //формируем массив удаляемых объектов, чтобы избежать ошибку изменения коллекции.
            TChild[] childrenArray = children.ToArray();

            //удаляем дочерние объекты.
            foreach (TChild child in childrenArray)
                child.Delete();
        }


        private bool __init_ChangeInfo = false;
        private PropertyChangeInfo _ChangeInfo;
        /// <summary>
        /// Содержит информацию об изменениях свойств данного экземпляра.
        /// </summary>
        protected PropertyChangeInfo ChangeInfo
        {
            get
            {
                if (!__init_ChangeInfo)
                {
                    _ChangeInfo = new PropertyChangeInfo();
                    __init_ChangeInfo = true;
                }
                return _ChangeInfo;
            }
        }


        private bool __init_ContextManager = false;
        private OperationContextManager _ContextManager;
        /// <summary>
        /// Управляет контекстами операций данного экземпляра.
        /// </summary>
        protected OperationContextManager ContextManager
        {
            get
            {
                if (!__init_ContextManager)
                {
                    _ContextManager = new OperationContextManager();
                    __init_ContextManager = true;
                }
                return _ContextManager;
            }
        }


        /// <summary>
        /// Проверяет, что объект существует и был сохранён в базу данных.
        /// Генерирует исключение в ином случае.
        /// </summary>
        public void CheckExists()
        {
            if (this.IsNew)
                throw new Exception(string.Format("Object of type {0} is not exists in database.", this.InstanceType.FullName));
        }


        /// <summary>
        /// Проверяет, что объект является новым и ещё не был сохранён в базу данных.
        /// Генерирует исключение в ином случае.
        /// </summary>
        public void CheckIsNew()
        {
            if (!this.IsNew)
                throw new Exception(string.Format("Operation not available because object of type {0} is already exists in database.", this.InstanceType.FullName));
        }


        ISkyContext ISkyObject.Context => this.Context;
    }
}
