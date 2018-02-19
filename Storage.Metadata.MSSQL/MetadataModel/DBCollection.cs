using System;
using System.Linq;
using System.Text;
using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Storage.Metadata.MSSQL
{
    /// <summary>
    /// Представляет типизированную коллекцию объектов модуля метаданных хранилища.
    /// </summary>
    /// <typeparam name="T">Тип объектов</typeparam>
    public class DBCollection<T> : IList<T>, ICollection<T>, IEnumerable<T>, IList, ICollection, IEnumerable
    {
        protected internal DBCollection()
        {
            this.Collection = new List<T>();
        }

        protected internal DBCollection(IEnumerable<T> collection)
        {
            this.Collection = new List<T>(collection);
        }

        private List<T> _Collection;
        protected internal List<T> Collection
        {
            get { return _Collection; }
            set { _Collection = value; }
        }

        #region Supported Methods

        protected internal void Add(T item)
        {
            this.Collection.Add(item);
        }

        protected internal void AddRange(IEnumerable<T> collection)
        {
            this.Collection.AddRange(collection);
        }

        protected internal bool Remove(T item)
        {
            return this.Collection.Remove(item);
        }

        protected internal void Clear()
        {
            this.Collection.Clear();
        }

        internal bool Contains(T item)
        {
            return this.Collection.Contains(item);
        }

        /// <summary>
        /// Возвращает элемент коллекции по заданному индексу.
        /// </summary>
        /// <param name="index">Индекс элемента в коллекци.</param>
        /// <returns></returns>
        public T this[int index]
        {
            get { return this.Collection[index]; }
        }

        /// <summary>
        /// Возвращает количество элементов, содержащихся в коллекции.
        /// </summary>
        public int Count
        {
            get { return this.Collection.Count; }
        }

        protected internal DBCollection<T> FindAll(Predicate<T> match)
        {
            List<T> resultList = this.Collection.FindAll(match);
            DBCollection<T> result = new DBCollection<T>(resultList);
            return result;
        }

        #endregion

        /// <summary>
        /// Строковое представление коллекции.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("Count = {0}", this.Collection.Count);
        }

        #region IList<T> Members

        int IList<T>.IndexOf(T item)
        {
            throw new NotImplementedException();
        }

        void IList<T>.Insert(int index, T item)
        {
            throw new NotImplementedException();
        }

        void IList<T>.RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        T IList<T>.this[int index]
        {
            get
            {
                return this[index];
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        #endregion

        #region ICollection<T> Members

        void ICollection<T>.Add(T item)
        {
            throw new NotImplementedException();
        }

        void ICollection<T>.Clear()
        {
            throw new NotImplementedException();
        }

        bool ICollection<T>.Contains(T item)
        {
            return ((ICollection<T>)this.Collection).Contains(item);
        }

        void ICollection<T>.CopyTo(T[] array, int arrayIndex)
        {
            ((ICollection<T>)this.Collection).CopyTo(array, arrayIndex);
        }

        int ICollection<T>.Count
        {
            get { return this.Count; }
        }

        bool ICollection<T>.IsReadOnly
        {
            get { return ((ICollection<T>)this.Collection).IsReadOnly; }
        }

        bool ICollection<T>.Remove(T item)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IEnumerable<T> Members

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return this.Collection.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.Collection.GetEnumerator();
        }

        #endregion

        #region IList Members

        int IList.Add(object value)
        {
            throw new NotImplementedException();
        }

        void IList.Clear()
        {
            throw new NotImplementedException();
        }

        bool IList.Contains(object value)
        {
            return ((IList)this.Collection).Contains(value);
        }

        int IList.IndexOf(object value)
        {
            return ((IList)this.Collection).IndexOf(value);
        }

        void IList.Insert(int index, object value)
        {
            throw new NotImplementedException();
        }

        bool IList.IsFixedSize
        {
            get { return ((IList)this.Collection).IsFixedSize; }
        }

        bool IList.IsReadOnly
        {
            get { return ((IList)this.Collection).IsReadOnly; }
        }

        void IList.Remove(object value)
        {
            throw new NotImplementedException();
        }

        void IList.RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        object IList.this[int index]
        {
            get
            {
                return ((IList)this.Collection)[index];
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        #endregion

        #region ICollection Members

        void ICollection.CopyTo(Array array, int index)
        {
            ((ICollection)this.Collection).CopyTo(array, index);
        }

        int ICollection.Count
        {
            get { return this.Count; }
        }

        bool ICollection.IsSynchronized
        {
            get { return ((ICollection)this.Collection).IsSynchronized; }
        }

        object ICollection.SyncRoot
        {
            get { return ((ICollection)this.Collection).SyncRoot; }
        }

        #endregion
    }
}
