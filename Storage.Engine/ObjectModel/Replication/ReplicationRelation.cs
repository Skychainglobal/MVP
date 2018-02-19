using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Storage.Engine
{
    public enum ReplicationRelation
    {
        /// <summary>
        /// Сильная связь репликации говорит о том, что она настроена на данном узле.
        /// </summary>
        Strong = 1,
        /// <summary>
        /// Слабая связь репликации говорит о том, что о ней есть запись в метаданных.
        /// Но может быть ситуация, когда источник данной связи уже не существует.
        /// </summary>
        Weak = 2
    }
}