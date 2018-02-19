using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Storage.Engine
{
    /// <summary>
    /// Кэш с данными в оперативной памяти.
    /// </summary>
    internal class MemoryCache : ICacheProvider
    {
        private static object _locker = new object();

        private MemoryCache() { }

        //Singleton
        private static readonly Lazy<MemoryCache> instanceHolder = new Lazy<MemoryCache>(() => new MemoryCache());
        public static ICacheProvider Current { get { return instanceHolder.Value; } }

        private bool __init_Cache;
        private Dictionary<string, MemoryCacheItem> _Cache;
        private Dictionary<string, MemoryCacheItem> Cache
        {
            get
            {
                if (!__init_Cache)
                {
                    _Cache = new Dictionary<string, MemoryCacheItem>();
                    __init_Cache = true;
                }
                return _Cache;
            }
        }

        #region ICacheProvider Members

        public void AddObject(string key, object obj, TimeSpan lifetime)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException("key");

            if (obj == null)
                throw new ArgumentNullException("obj");

            lock (_locker)
            {
                if (this.Cache.ContainsKey(key))
                {
                    MemoryCacheItem existsCacheItem = this.Cache[key];
                    existsCacheItem.Reinit(DateTime.Now, lifetime, obj);
                }
                else
                {
                    MemoryCacheItem cacheItem = new MemoryCacheItem(DateTime.Now, lifetime, obj);
                    this.Cache.Add(key, cacheItem);
                }
            }
        }

        public object GetObject(string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException("key");

            object obj = null;
            if (this.Cache.ContainsKey(key))
            {
                MemoryCacheItem cacheItem = this.Cache[key];
                obj = cacheItem.GetObject();
            }

            return obj;
        }

        public T GetObject<T>(string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException("key");

            T typedObj = default(T);
            object obj = this.GetObject(key);
            if (obj != null)
                typedObj = (T)obj;

            return typedObj;
        }

        #endregion
    }
}