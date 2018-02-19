using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Storage.Engine
{
    public class EngineConsts
    {
        public class Logs
        {
            public class Scopes
            {
                public const string Engine = "Engine";
            }
        }

        public class CongfigParams
        {
            public const string ContentDeliveryHost = "ContentDeliveryHost";
            public const string AllowMetadataRestoring = "AllowMetadataRestoring";
        }

        public class Lifetimes
        {
            public static TimeSpan Folder = TimeSpan.FromSeconds(30);
        }

        public static class Periods
        {
            /// <summary>
            /// Время, спустя которое, если нода не отвечает, то её обработка останавливается. (в минутах)
            /// </summary>
            public const int SwitchOffUnaccessibleNode = 30 * 24 * 60; //30 дней в минутах

            /// <summary>
            /// Время, спустя которое происходит повторное обращение к узлам репликации. (в минутах)
            /// </summary>
            public const int GetNeighbor = 30; //30 минут

            /// <summary>
            /// Время между запросами получения файлов. (в минутах)
            /// </summary>
            public const int GetFiles = 1;
        }
    }
}