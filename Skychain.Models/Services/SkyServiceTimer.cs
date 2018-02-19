using Skychain.Models.Runtime;
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
    /// Представляет таймера запуска выполнения сервиса.
    /// </summary>
    public abstract class SkyServiceTimer
    {
        /// <summary>
        /// Создаёт новый экземпляр таймера запуска выполнения сервиса.
        /// </summary>
        protected SkyServiceTimer()
        {
            //устанавливаем время запуска сервиса.
            ServiceStartTime = DateTime.Now;
        }


        /// <summary>
        /// Время запуска сервиса.
        /// </summary>
        internal static DateTime ServiceStartTime;


        private bool __init_LogName = false;
        private string _LogName;
        /// <summary>
        /// Название лога сервиса.
        /// </summary>
        private string LogName
        {
            get
            {
                if (!__init_LogName)
                {
                    _LogName = this.InitLogName();
                    if (string.IsNullOrEmpty(_LogName))
                        throw new Exception("Failed to get service log name.");
                    __init_LogName = true;
                }
                return _LogName;
            }
        }


        /// <summary>
        /// Инициализирует название лога сервиса.
        /// </summary>
        protected abstract string InitLogName();


        /// <summary>
        /// Создаёт сервис для запуска его выполнения.
        /// </summary>
        protected abstract ISkyService CreateService();


        /// <summary>
        /// Запускает таймер.
        /// </summary>
        public void Run()
        {
            while (true)
            {
                //признак, устанавливаемый при возникновении ошибки обработки.
                bool hasError = false;
                try
                {
                    //сбрасываем текущий контекст выполнения.
                    RuntimeContext.ResetCurrent();

                    //создаём и запускаем экземпляр сервиса.
                    ISkyService service = this.CreateService();
                    if (service == null)
                        throw new Exception("Failed to get instance of the ISkyService.");
                    service.Run();
                }
                catch (Exception ex)
                {
                    //устанавливаем признак наличия ошибки.
                    hasError = true;

                    //записываем ошибку в лог.
                    WriteErrorLog(ex, this.LogName);
                }
                finally
                {
                    //при наличии ошибки, увеличиваем интервал таймера до 30 секунд, чтобы не забивать лог ошибок.
                    if (hasError)
                        Thread.Sleep(30000);

                    //при наличии необработанных ошибок в порождённых потоков обработки, приостанавливаем на 30 сек, чтобы не забивать лог.
                    else if (HasUnhandledExecutionError)
                    {
                        //приостанавливаем работу.
                        Thread.Sleep(30000);

                        //сбрасываем идентификатор основного потока, если установлена соответствующая директива из порождённого потока обработки.
                        HasUnhandledExecutionError = false;
                    }

                    //при отсутствии ошибок выполняем таймер с частотой в 1 секунду.
                    else
                        Thread.Sleep(1000);
                }
            }
        }


        #region WriteErrorLog

        private static bool _HasUnhandledExecutionError;
        /// <summary>
        /// Возвращает true, если при выполнении обработки возникла необработанная ошибка.
        /// </summary>
        internal static bool HasUnhandledExecutionError
        {
            get { return _HasUnhandledExecutionError; }
            set { _HasUnhandledExecutionError = value; }
        }


        /// <summary>
        /// Возвращает true, если ошибка является принудительным выключением сервиса.
        /// </summary>
        /// <param name="error">Ошибка, проверяемая на наличие ThreadAbortException.</param>
        private static bool IsThreadAbortException(Exception error)
        {
            if (error == null)
                return false;

            bool isThreadAbort = false;
            Exception innerException = error;
            while (innerException != null)
            {
                if (innerException is ThreadAbortException)
                {
                    isThreadAbort = true;
                    break;
                }
                innerException = innerException.InnerException;
            }

            return isThreadAbort;
        }

        private static object __lock_ErrorLog = new object();
        /// <summary>
        /// Записывает ошибку в лог ошибок в файл на диске, расположенный в исполняемой папке сервиса.
        /// В случае если файл недоступен, записывает ошибку в журнал событий операционной системы.
        /// </summary>
        /// <param name="error">Ошибка, возникшая во время выполнения.</param>
        /// <param name="logName">Название лога сервиса.</param>
        internal static void WriteErrorLog(Exception error, string logName)
        {
            //выходим, если не передана ошибка.
            if (error == null)
                return;
            if (string.IsNullOrEmpty(logName))
                return;

            //получаем текст ошибки безопасным способом.
            string errorMessage = null;
            string stackTraceAboveException = null;
            string newLine = null;
            try
            {
                newLine = Environment.NewLine;
                stackTraceAboveException = Environment.StackTrace;
                errorMessage = error.ToString();

                //проверяем, является ли ошибка принудительной остановкой сервиса.
                bool isThreadAbort = IsThreadAbortException(error);
                if (isThreadAbort)
                    errorMessage = string.Format("The service was forcibly stopped. --->{0}{1}", newLine, errorMessage);
            }
            catch
            {
                errorMessage = "The error text could not be retrieved due to the error of calling Exception.ToString().";
            }

            //выходим, если не удалось получить текст ошибки.
            if (string.IsNullOrEmpty(errorMessage))
                return;

            //выходим, если по каким-то причинам статическая переменная блокировки записи лога не задана.
            if (__lock_ErrorLog == null)
                return;

            lock (__lock_ErrorLog)
            {
                //пытаемся записать ошибку в файл.
                try
                {
                    //название файла лога.
                    //файл распологается в корне диска C:\.
                    string fileName = Path.GetPathRoot(Environment.CurrentDirectory) + string.Format("{0}.log", logName);

                    //если файл существует и размер файла превышает предельно допустимый (10МБ), удаляем файл.
                    FileInfo errorsLogFile = new FileInfo(fileName);
                    if (errorsLogFile.Exists)
                    {
                        if (errorsLogFile.Length > 1024 * 1024 * 10)
                            File.Delete(fileName);
                    }

                    //записываем текст ошибки в файл.
                    File.AppendAllText(
                        fileName,
                        string.Format("[{0}] {2}{1}{1}", DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss"), newLine, errorMessage),
                        Encoding.UTF8);
                }
                catch
                {
                    //пытаемся записать ошибку в журнал событий операционной системы.
                    try
                    {
                        EventLog.WriteEntry(
                            logName,
                            string.Format("{0}{1}{1}{1}Stack trace above exception:{1}{2}", errorMessage, newLine, stackTraceAboveException),
                            EventLogEntryType.Error);
                    }
                    catch { }
                }
            }
        }

        #endregion
    }
}
