using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Storage.Engine;
using Storage.Lib;

namespace Storage.Service.Wcf
{
    /// <summary>
    /// Класс с параметрами конфигурации сервиса.
    /// </summary>
    internal class ServiceConfiguration
    {
        private bool __init_MaxBufferRequestSize;
        private int _MaxBufferRequestSize;
        /// <summary>
        /// Максимальный размер запроса в буферном режиме.
        /// </summary>
        internal int MaxBufferRequestSize
        {
            get
            {
                if (!__init_MaxBufferRequestSize)
                {
                    string maxBufferRequestSize = ConfigReader.GetStringValue(ServiceConsts.CongfigParams.MaxBufferRequestSize);
                    _MaxBufferRequestSize = Convert.ToInt32(maxBufferRequestSize);
                    if (_MaxBufferRequestSize < 0)
                        throw new Exception(string.Format("Максимальный размер запроса в буферном режиме не может быть меньше 0"));

                    __init_MaxBufferRequestSize = true;
                }
                return _MaxBufferRequestSize;
            }
        }

        private bool __init_ContentDeliveryPermissionGroups;
        private List<string> _ContentDeliveryPermissionGroups;
        /// <summary>
        /// Группы пользователей, которые имеют доступ на открытие файлов.
        /// </summary>
        internal List<string> ContentDeliveryPermissionGroups
        {
            get
            {
                if (!__init_ContentDeliveryPermissionGroups)
                {
                    _ContentDeliveryPermissionGroups = new List<string>();

                    string permissionGroupString = ConfigReader.GetStringValue(ServiceConsts.CongfigParams.ContentDeliveryManagerPermissionGroup);
                    if (string.IsNullOrEmpty(permissionGroupString))
                        throw new Exception(string.Format("Не задана группа(ы) пользователей с доступом к веб-сервису"));

                    _ContentDeliveryPermissionGroups = permissionGroupString.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries).ToList();

                    __init_ContentDeliveryPermissionGroups = true;
                }
                return _ContentDeliveryPermissionGroups;
            }
        }

        private bool __init_ContentDeliveryConnectionLimit;
        private int _ContentDeliveryConnectionLimit;
        /// <summary>
        /// Кол-во одновременных подключений для отдачи содержимого файлов
        /// </summary>
        internal int ContentDeliveryConnectionLimit
        {
            get
            {
                if (!__init_ContentDeliveryConnectionLimit)
                {
                    int connectionsLimit = ConfigReader.GetIntegerValue(ServiceConsts.CongfigParams.ContentDeliveryManagerConnectionsLimit, false);
                    if (connectionsLimit > 0)
                        _ContentDeliveryConnectionLimit = connectionsLimit;
                    else
                    {
                        //кол-во подключений по-умолчанию для отдачи содержимого файлов.
                        _ContentDeliveryConnectionLimit = ServiceConsts.Defaults.ContentDeliveryManagerConnectionsLimit;
                    }

                    __init_ContentDeliveryConnectionLimit = true;
                }
                return _ContentDeliveryConnectionLimit;
            }
        }

        private bool __init_ContentDeliveryHost;
        private string _ContentDeliveryHost;
        /// <summary>
        /// Хост службы отдачи содержимого файлов.
        /// </summary>
        internal string ContentDeliveryHost
        {
            get
            {
                if (!__init_ContentDeliveryHost)
                {
                    _ContentDeliveryHost = ConfigReader.GetStringValue(EngineConsts.CongfigParams.ContentDeliveryHost);
                    //хост для прослушки должен обязательно заканчиваться /, иначе будет exception
                    if (!_ContentDeliveryHost.EndsWith("/"))
                        _ContentDeliveryHost += '/';

                    __init_ContentDeliveryHost = true;
                }
                return _ContentDeliveryHost;
            }
        }

        private bool __init_WcfCallsPermissionGroups;
        private List<string> _WcfCallsPermissionGroups;
        /// <summary>
        /// Группы пользователей, которые имеют доступ на вызовы методов веб-сервиса.
        /// </summary>
        internal List<string> WcfCallsPermissionGroups
        {
            get
            {
                if (!__init_WcfCallsPermissionGroups)
                {
                    _WcfCallsPermissionGroups = new List<string>();

                    string permissionGroupString = ConfigReader.GetStringValue(ServiceConsts.CongfigParams.WcfCallsPermissionGroup);
                    if (string.IsNullOrEmpty(permissionGroupString))
                        throw new Exception(string.Format("Не задана группа(ы) пользователей с доступом к веб-сервису"));

                    _WcfCallsPermissionGroups = permissionGroupString.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries).ToList();


                    __init_WcfCallsPermissionGroups = true;
                }
                return _WcfCallsPermissionGroups;
            }
        }


    }
}