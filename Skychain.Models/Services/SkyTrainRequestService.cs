using Skychain.Models.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Skychain.Models.Services
{
    /// <summary>
    /// Представляет сервис обработки запросов тренировки нейросети.
    /// </summary>
    public class SkyTrainRequestService : ISkyService
    {
        internal SkyTrainRequestService()
        {
        }


        /// <summary>
        /// Выполняет обработку новых запросов к нейросетям.
        /// </summary>
        public void Run()
        {
            //получаем запросы для обработки.
            IEnumerable<ISkyTrainRequest> creatingRequests = SkyContext.Current.ObjectAdapters.TrainRequests.GetObjects(dbSet => dbSet
                .Where(x => x.Status == SkyTrainRequestStatus.Creating)
                .OrderBy(x => x.ID));

            //запускаем обработку запросов.
            foreach (ISkyTrainRequest request in creatingRequests)
            {
                //помечаем запрос как обрабатываемый.
                request.Process();

                //создаём и запускаем поток для обработки.
                Thread requestThread = new Thread(ProcessRequest);

                //устанавливаем поток фоновым.
                requestThread.IsBackground = true;

                //создаём обработчик запроса.
                SkyTrainRequestHandler requestHandler = new SkyTrainRequestHandler(request.ID);

                //запускаем обработку запроса.
                requestThread.Start(requestHandler);
            }
        }


        /// <summary>
        /// Запускает обработку запроса.
        /// </summary>
        /// <param name="requestHandler">Обрабатываемый запрос.</param>
        private static void ProcessRequest(object requestHandler)
        {
            ((SkyTrainRequestHandler)requestHandler).Process();
        }
    }
}
