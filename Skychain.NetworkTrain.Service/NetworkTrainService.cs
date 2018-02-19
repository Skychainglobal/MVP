using Skychain.Models.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Skychain.NetworkTrain.Service
{
    public partial class NetworkTrainService : ServiceBase
    {
        public NetworkTrainService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            this.ServiceThread.Start();
        }

        protected override void OnStop()
        {
            this.ServiceThread.Abort();
        }


        private bool __init_ServiceThread = false;
        private Thread _ServiceThread;
        /// <summary>
        /// Управляющий поток сервиса.
        /// </summary>
        private Thread ServiceThread
        {
            get
            {
                if (!__init_ServiceThread)
                {
                    _ServiceThread = new Thread(this.ProcessRequests)
                    {
                        IsBackground = true,
                        Name = "Skychain.NetworkTrain.Service.ControlThread"
                    };
                    __init_ServiceThread = true;
                }
                return _ServiceThread;
            }
        }


        private void ProcessRequests()
        {
            SkyTrainRequestServiceTimer serviceTimer = new SkyTrainRequestServiceTimer();
            serviceTimer.Run();
        }
    }
}
