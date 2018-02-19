using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Storage.Engine
{
    /// <summary>
    /// Логгер в журнал событий Windows.
    /// </summary>
    internal class EventViewerLogger : ILogProvider
    {
        private const string _LogName = "Storage";
        private const string _LogSource = "Skychain.Storage";

        public EventViewerLogger(string scope)
            : this()
        {

        }

        /// <summary>
        /// Логгер в журнал событий Windows.
        /// </summary>
        public EventViewerLogger()
        {
            //Если источник записи лога существует, то проверяем, что он пишет в нужный нам лог.
            if (EventLog.SourceExists(_LogSource))
            {
                //если пишет не в тот лог, то
                if (EventLog.LogNameFromSourceName(_LogSource, ".") != _LogName)
                {
                    //удаляем источник записи из ненужного нам лога
                    EventLog.DeleteEventSource(_LogSource, ".");

                    //создаём источник записи для нужного нам лога. 
                    // ! Если лог не существует, то он будет автоматически создан при создании источника записи для этого лога.
                    EventLog.CreateEventSource(new EventSourceCreationData(_LogSource, _LogName)
                    {
                        MachineName = "."
                    });
                }
            }
            else
            {
                //создаём источник записи для нужного нам лога
                EventLog.CreateEventSource(new EventSourceCreationData(_LogSource, _LogName)
                {
                    MachineName = "."
                });
            }
        }

        /// <summary>
        /// Записывает сообщение в журнал событий Windows.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="level"></param>
        private void WriteEvent(string message, LogLevel level/*, bool checkLogLevel*/)
        {/*
            if (checkLogLevel)
            {
                if (level > this.AllowedLogLevel)
                    return;
            }*/

            EventLogEntryType entryType;
            switch (level)
            {
                case LogLevel.Critical:
                case LogLevel.Error:
                    entryType = EventLogEntryType.Error;
                    break;
                case LogLevel.Warning:
                    entryType = EventLogEntryType.Warning;
                    break;
                case LogLevel.Info:
                case LogLevel.Verbose:
                    entryType = EventLogEntryType.Information;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("level", level, null);
            }

            try
            {
                EventLog.WriteEntry(_LogName, message, entryType);
            }
            catch (Exception ex)
            {
                /*ошибка записи в системный журнал логов*/
            }
        }

        /// <summary>
        /// Записывает сообщение в журнал логов.
        /// </summary>
        /// <param name="message">Сообщение.</param>
        /// <param name="level">Уровень важности сообщения.</param>
        public void WriteMessage(string message, LogLevel level = LogLevel.Verbose)
        {
            this.WriteEvent(message, level/*, true*/);
        }

        /// <summary>
        /// Записывает сообщение с форматированием в журнал логов.
        /// </summary>
        /// <param name="message">Сообщение.</param>
        /// <param name="params">Параметры для форматированного сообщения.</param>
        public void WriteFormatMessage(string message, params object[] @params)
        {
            this.WriteEvent(String.Format(message, @params), LogLevel.Info/*, true*/);
        }
    }
}
