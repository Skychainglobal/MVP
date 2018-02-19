﻿using Skychain.Models.Runtime;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Skychain.Models.Services
{
    /// <summary>
    /// Представляет таймер запуска обработки запросов тренировки нейросети.
    /// </summary>
    public class SkyTrainRequestServiceTimer : SkyServiceTimer
    {
        /// <summary>
        /// Создаёт новый экземпляр таймера запуска обработки запросов тренировки нейросети.
        /// </summary>
        public SkyTrainRequestServiceTimer()
        {
        }


        /// <summary>
        /// Создаёт новый экземпляр сервиса для его выполнения.
        /// </summary>
        protected override ISkyService CreateService()
        {
            return new SkyTrainRequestService();
        }

        /// <summary>
        /// Возвращает название лога.
        /// </summary>
        protected override string InitLogName()
        {
            return "Skychain.TrainRequest.Service";
        }


        /// <summary>
        /// Записывает ошибку в лог ошибок в файл на диске, расположенный в исполняемой папке сервиса.
        /// В случае если файл недоступен, записывает ошибку в журнал событий операционной системы.
        /// </summary>
        /// <param name="error">Ошибка, возникшая во время выполнения.</param>
        internal static void WriteErrorLog(Exception error)
        {
            SkyServiceTimer.WriteErrorLog(error, "Skychain.TrainRequest.Service");
        }
    }
}
