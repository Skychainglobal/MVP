using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Security.Principal;
using System.ServiceModel;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Storage.Engine;

namespace Storage.Service.Wcf
{
    public partial class Service : ServiceBase
    {
        internal static ServiceHost _serviceHost;
        internal static ServiceHost _replicationServiceHost;

        private static HttpListener _serviceListener;

        public Service()
        {
            InitializeComponent();
        }

        private bool __init_ListenerThread;
        private Thread _ListenerThread;
        private Thread ListenerThread
        {
            get
            {
                if (!__init_ListenerThread)
                {
                    _ListenerThread = new Thread(new ThreadStart(this.Listern));
                    __init_ListenerThread = true;
                }
                return _ListenerThread;
            }
        }

        private bool __init_Configuration;
        private ServiceConfiguration _Configuration;
        private ServiceConfiguration Configuration
        {
            get
            {
                if (!__init_Configuration)
                {
                    _Configuration = new ServiceConfiguration();
                    __init_Configuration = true;
                }
                return _Configuration;
            }
        }

        /// <summary>
        /// Запускает http слушатель для разрешения сессионных ссылок.
        /// </summary>
        internal void Listern()
        {
            if (_serviceListener != null)
            {
                _serviceListener.Close();
            }

            _serviceListener = new HttpListener();

            _serviceListener.Prefixes.Add(this.Configuration.ContentDeliveryHost);
            _serviceListener.AuthenticationSchemes = AuthenticationSchemes.Ntlm;
            _serviceListener.Start();

            this.EventLog.WriteEntry("Хост для отдачи содержимого файлов успешно запущен");

            Task.Factory.StartNew(() => Service.ListenHandler());
        }

        private static void ListenHandler()
        {
            //читаем входящие запросы в отдельных потоках
            ServiceConfiguration configuration = new ServiceConfiguration();
            SemaphoreSlim semaphore = new SemaphoreSlim(configuration.ContentDeliveryConnectionLimit, configuration.ContentDeliveryConnectionLimit);

            while (true)
            {
                semaphore.Wait();

                _serviceListener.GetContextAsync().ContinueWith(async (ctx) =>
                {
                    try
                    {
                        var context = await ctx;
                        await Service.ProcessFileRequest(context);
                        return;
                    }
                    catch (Exception ex)
                    {
                        EventLog logger = new EventLog();
                        logger.Source = "WSSC.Storage.Service.Wcf.Service";
                        logger.WriteEntry(string.Format("Ошибка при отдаче файла. Текст ошибки: {0}", ex));
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                });
            }
        }

        /// <summary>
        /// Обрабатывает запрос на получение содержимого файла по сессионной ссылке.
        /// </summary>
        /// <param name="context">Контекст запроса.</param>
        private static async Task ProcessFileRequest(HttpListenerContext context)
        {
            if (context == null)
                throw new ArgumentNullException("context");

            ContentDeliveryManager reader = new ContentDeliveryManager();
            await reader.Process(context);
        }

        /// <summary>
        /// Обработчик запуска службы.
        /// </summary>
        /// <param name="args">Аргументы запуска.</param>
        protected override void OnStart(string[] args)
        {
            try
            {
                AppDomain.CurrentDomain.SetPrincipalPolicy(PrincipalPolicy.WindowsPrincipal);
                //инициализация движка
                using (StorageEngine engine = new StorageEngine())
                    engine.Init();

                this.EventLog.WriteEntry("Инициализация успешно завершена");

                //wcf
                if (_serviceHost != null)
                {
                    _serviceHost.Close();
                }

                if (_replicationServiceHost != null)
                    _replicationServiceHost.Close();

                _serviceHost = new ServiceHost(typeof(StorageService));
                _serviceHost.Faulted += serviceHost_Faulted;
                _serviceHost.UnknownMessageReceived += serviceHost_UnknownMessageReceived;
                _serviceHost.Open();

                _replicationServiceHost = new ServiceHost(typeof(StorageReplicationService));
                _replicationServiceHost.Faulted += (s, e) =>
                {
                    this.EventLog.WriteEntry("ReplicationServiceHostFaulted!", EventLogEntryType.Error);
                };
                _replicationServiceHost.UnknownMessageReceived += serviceHost_UnknownMessageReceived;
                _replicationServiceHost.Open();

                this.EventLog.WriteEntry("Хост службы файлового хранилища запущен");

                //http listener
                this.Listern();
            }
            catch (Exception ex)
            {
                this.EventLog.WriteEntry("Ошибка запуска службы файлового хранилища. Текст ошибки: " + ex.ToString(), EventLogEntryType.Error);
            }
        }

        void serviceHost_UnknownMessageReceived(object sender, UnknownMessageReceivedEventArgs e)
        {
            this.EventLog.WriteEntry("UnknownMessageReceived!", EventLogEntryType.Error);
        }

        void serviceHost_Faulted(object sender, EventArgs e)
        {
            this.EventLog.WriteEntry("ServiceHostFaulted!", EventLogEntryType.Error);
        }

        /// <summary>
        /// Обработчик остановки службы.
        /// </summary>
        protected override void OnStop()
        {
            if (_serviceHost != null)
            {
                _serviceHost.Close();
                _serviceHost = null;
            }

            if (_replicationServiceHost != null)
            {
                _replicationServiceHost.Close();
                _replicationServiceHost = null;
            }

            if (_serviceListener != null)
            {
                _serviceListener.Close();
                _serviceListener = null;
            }
        }
    }
}
