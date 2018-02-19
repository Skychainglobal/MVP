using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Storage.Data.Blob
{
    /// <summary>
    /// Класс с информацией о блокировках контейнера.
    /// </summary>
    internal class ContainerLockInfo
    {
        private bool __init_StreamingLocks;
        private Dictionary<string, DateTime> _StreamingLocks;
        private Dictionary<string, DateTime> StreamingLocks
        {
            get
            {
                if (!__init_StreamingLocks)
                {
                    _StreamingLocks = new Dictionary<string, DateTime>();
                    __init_StreamingLocks = true;
                }
                return _StreamingLocks;
            }
        }

        public void AddStreamingLock(Blob blob)
        {
            if (blob == null)
                throw new ArgumentNullException("blob");

            string pathLower = blob.File.FullName.ToLower();
            if (!this.StreamingLocks.ContainsKey(pathLower))
                this.StreamingLocks.Add(pathLower, DateTime.Now);
        }

        public void RemoveStreamingLock(Blob blob)
        {
            if (blob == null)
                throw new ArgumentNullException("blob");

            string pathLower = blob.File.FullName.ToLower();
            if (this.StreamingLocks.ContainsKey(pathLower))
                this.StreamingLocks.Remove(pathLower);
        }

        public bool LockExists(Blob blob)
        {
            if (blob == null)
                throw new ArgumentNullException("blob");

            string pathLower = blob.File.FullName.ToLower();
            return this.StreamingLocks.ContainsKey(pathLower);
        }
    }
}