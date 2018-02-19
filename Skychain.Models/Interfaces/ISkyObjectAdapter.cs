using Skychain.Models.Entity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skychain.Models
{
    /// <summary>
    /// Представляет адаптер объектов системы.
    /// </summary>
    public interface ISkyObjectAdapter<IObject, TEntity> 
        where IObject : ISkyObject
        where TEntity : SkyEntity
    {
        /// <summary>
        /// Контекст системы.
        /// </summary>
        ISkyContext Context { get; }

        /// <summary>
        /// Возвращает объект системы по идентификатору сохраняемого объекта.
        /// </summary>
        /// <param name="entityID">Идентификатор сохраняемого объекта.</param>
        IObject GetObject(int entityID);

        /// <summary>
        /// Возвращает объект системы по идентификатору сохраняемого объекта.
        /// </summary>
        /// <param name="entityID">Идентификатор сохраняемого объекта.</param>
        /// <param name="throwNotFoundException">При установленном параметре true генерирует исключение в случае отсутствия экземпляра.</param>
        IObject GetObject(int entityID, bool throwNotFoundException);

        /// <summary>
        /// Возвращает объект данного типа по сохранённому объекту, полученному из соответсвующего набора данных.
        /// </summary>
        /// <param name="entityResolver">Метод, возвращающий сохранённый объект по набору данных.</param>
        IObject GetObject(Func<DbSet<TEntity>, TEntity> entityResolver);

        /// <summary>
        /// Возвращает все объекты данного типа.
        /// </summary>
        IEnumerable<IObject> GetObjects();

        /// <summary>
        /// Возвращает объекты данного типа для массива сохранённых объектов, полученного из соответствующего набора данных.
        /// </summary>
        /// <param name="entitiesResolver">Метод, возвращающий массив сохранённых объектов по набору данных.</param>
        IEnumerable<IObject> GetObjects(Func<DbSet<TEntity>, IEnumerable<TEntity>> entitiesResolver);

        /// <summary>
        /// Выполняет команду в контексте подключения к набору данных и возвращает результат заданного типа.
        /// </summary>
        /// <typeparam name="TResult">Тип возвращаемого результата.</typeparam>
        /// <param name="entitySetCommand">Команда, выполняющая запрос к набору данных и возвращающая результат.</param>
        TResult ExecuteQuery<TResult>(Func<DbSet<TEntity>, TResult> entitySetCommand);

        /// <summary>
        /// Выполняет команду в контексте подключения к набору данных и возвращает результат заданного типа.
        /// </summary>
        /// <typeparam name="TResult">Тип возвращаемого результата.</typeparam>
        /// <param name="entitySetCommand">Команда, выполняющая запрос к набору данных и возвращающая результат.</param>
        TResult ExecuteQuery<TResult>(Func<DbSet<TEntity>, SkyEntityContext, TResult> entitySetCommand);

        /// <summary>
        /// Выполняет команду в контексте подключения к набору данных.
        /// </summary>
        /// <param name="entitySetCommand">Команда, выполняющая запрос к набору данных.</param>
        void ExecuteQuery(Action<DbSet<TEntity>, SkyEntityContext> entitySetCommand);
    }
}
