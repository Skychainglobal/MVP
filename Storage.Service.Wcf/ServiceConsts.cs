using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Storage.Service.Wcf
{
    internal class ServiceConsts
    {
        public class CongfigParams
        {
            public const string WcfCallsPermissionGroup = "WcfCalls_PermissionGroup";
            public const string ContentDeliveryManagerPermissionGroup = "ContentDeliveryManager_PermissionGroup";
            public const string ContentDeliveryManagerConnectionsLimit = "ContentDeliveryManager_ConnectionsLimit";
            public const string MaxBufferRequestSize = "MaxBufferRequestSize";
        }

        public class Defaults
        {
            public const int ContentDeliveryManagerConnectionsLimit = 1000;
        }

        public class Logs
        {
            public class Scopes
            {
                public const string WcfService = "WCF.StorageService";
                public const string WcfContentDeliveryManager = "WCF.StorageService.ContentDeliveryManager";
            }
        }

        public class Replication
        {
            public const int ReplicationPort = 7070;
        }

        public class TransportParams
        {
            public class Binding
            {
                public const int MaxBufferSize = int.MaxValue;
                public const int MaxMessageSize = int.MaxValue;
            }

            public class ReaderQuotas
            {
                public const int MaxDepth = 32;
                public const int MaxStringContentLength = int.MaxValue;
                public const int MaxBytesPerRead = 4096;
            }
        }
    }
}