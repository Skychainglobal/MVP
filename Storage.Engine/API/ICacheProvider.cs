using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Storage.Engine
{
    public interface ICacheProvider
    {
        void AddObject(string key, object obj, TimeSpan lifetime);

        object GetObject(string key);

        T GetObject<T>(string key);
    }
}