using Skychain.Models.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skychain.Models.Services
{
    /// <summary>
    /// Представляет обработчик запроса к нейросети.
    /// </summary>
    public class SkyNetworkRequestHandler
    {
        internal SkyNetworkRequestHandler(int requestID)
        {
            if (requestID == 0)
                throw new ArgumentNullException("requestID");

            this.RequestID = requestID;
        }

        /// <summary>
        /// Идентификатор запроса.
        /// </summary>
        public int RequestID { get; private set; }


        /// <summary>
        /// Выполняет обработку запроса к нейросети.
        /// </summary>
        public void Process()
        {
            try
            {
                //сбрасываем текущий контекст выполнения для второстепенного потока выполнения запроса.
                RuntimeContext.ResetCurrent();

                //получаем экземпляр запроса.
                ISkyNetworkRequest request = SkyContext.Current.ObjectAdapters.NetworkRequests.GetObject(this.RequestID, true);

                //выполняем обработку запроса.
                try
                {
                    //вычисляем результат.
                    request.ComputeResult();
                }
                catch (Exception ex)
                {
                    //устанавливаем текст ошибки.
                    request.ErrorMessage = ex.ToString();
                }
                finally
                {
                    //устанавливаем статус завершённого запроса.
                    request.Complete();
                }
            }
            catch (Exception ex)
            {
                //устанавливаем признак необходимости большой задержки из-за непредвиденной ошибки.
                SkyServiceTimer.HasUnhandledExecutionError = true;

                //логируем необработанную ошибку.
                SkyNetworkRequestServiceTimer.WriteErrorLog(ex);
            }
        }
    }
}
