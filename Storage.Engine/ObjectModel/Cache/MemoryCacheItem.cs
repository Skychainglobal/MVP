using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Storage.Engine
{
    internal class MemoryCacheItem
    {
        internal MemoryCacheItem(DateTime timeCreated, TimeSpan lifetime, object obj)
        {
            if (timeCreated == DateTime.MinValue)
                throw new ArgumentNullException("timeCreated");

            if (lifetime == null || lifetime.TotalMilliseconds < 1)
                throw new ArgumentNullException("lifetime");

            if (obj == null)
                throw new ArgumentNullException("obj");

            this.Object = obj;
            this.TimeCreated = timeCreated;
            this.Lifetime = lifetime;
        }

        internal void Reinit(DateTime timeCreated, TimeSpan lifetime, object obj)
        {
            if (timeCreated == DateTime.MinValue)
                throw new ArgumentNullException("timeCreated");

            if (lifetime == null || lifetime.TotalMilliseconds < 1)
                throw new ArgumentNullException("lifetime");

            if (obj == null)
                throw new ArgumentNullException("obj");

            __init_Expired = false;

            this.Object = obj;
            this.TimeCreated = timeCreated;
            this.Lifetime = lifetime;
        }

        public DateTime TimeCreated { get; private set; }

        public TimeSpan Lifetime { get; private set; }

        public object Object { get; private set; }

        private bool __init_Expired;
        private DateTime _Expired;
        public DateTime Expired
        {
            get
            {
                if (!__init_Expired)
                {
                    _Expired = this.TimeCreated.Add(this.Lifetime);
                    __init_Expired = true;
                }
                return _Expired;
            }
        }

        public object GetObject()
        {
            if (DateTime.Now.Ticks > this.Expired.Ticks)
                this.Object = null;

            return this.Object;
        }
    }
}