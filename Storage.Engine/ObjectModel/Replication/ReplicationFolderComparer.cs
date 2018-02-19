using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Storage.Engine
{
    public class ReplicationFolderComparer : /*IEqualityComparer<IReplicationFolderMetadata>,*/ IComparer<IReplicationFolderMetadata>
    {/*
        /// <summary>
        /// Сравнивает два объекта.
        /// </summary>
        /// <param name="x">Объект x.</param>
        /// <param name="y">Объект y.</param>
        /// <returns></returns>
        public bool Equals(IReplicationFolderMetadata x, IReplicationFolderMetadata y)
        {
            if (object.ReferenceEquals(x, y)) return true;

            if (x == null || y == null)
                return false;

            return x.SourceStorage.ID == y.SourceStorage.ID &&
                   x.Folder.ID == y.Folder.ID;
        }

        /// <summary>
        /// Возвращает хеш-код указанного объекта.
        /// </summary>
        /// <param name="obj">Объект для вычисления хеш-кода.</param>
        /// <returns></returns>
        public int GetHashCode(IReplicationFolderMetadata obj)
        {
            return new
            {
                x = obj.Folder.ID,
                y = obj.SourceStorage.ID
            }.GetHashCode();
        }
        */
        /// <summary>
        /// Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.
        /// </summary>
        /// <returns>
        /// A signed integer that indicates the relative values of <paramref name="x"/> and <paramref name="y"/>, as shown in the following table.Value Meaning Less than zero<paramref name="x"/> is less than <paramref name="y"/>.Zero<paramref name="x"/> equals <paramref name="y"/>.Greater than zero<paramref name="x"/> is greater than <paramref name="y"/>.
        /// </returns>
        /// <param name="first">The first object to compare.</param>
        /// <param name="second">The second object to compare.</param>
        public int Compare(IReplicationFolderMetadata first, IReplicationFolderMetadata second)
        {
            if (first == null)
                throw new ArgumentNullException("first");
            if (second == null)
                throw new ArgumentNullException("second");

            string url1 = first.Folder.Url;
            string url2 = second.Folder.Url;

            //чтобы с одинаковым URL сохранялись в SortedSet
            if (String.Equals(url1, url2, StringComparison.OrdinalIgnoreCase))
                return 1;

            return this.GetSlashCount(first.Folder.Url) - this.GetSlashCount(second.Folder.Url);
        }

        private readonly Dictionary<string, int> _countByUrl = new Dictionary<string, int>();

        private int GetSlashCount(string str)
        {
            if (String.IsNullOrEmpty(str))
                return 0;
            str = str.ToLower();

            int count;
            if (!_countByUrl.TryGetValue(str, out count))
            {
                _countByUrl.Add(str, count = str.Count(ch => ch == '/'));
            }
            return count;
        }
    }
}
