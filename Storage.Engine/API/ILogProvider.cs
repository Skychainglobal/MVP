using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Storage.Engine
{
    /// <summary>
    /// Интерфейс провайдера для логов.
    /// </summary>
    public interface ILogProvider
    {
        /// <summary>
        /// Записывает сообщение в журнал логов.
        /// </summary>
        /// <param name="message">Сообщение.</param>
        /// <param name="level">Уровень важности сообщения.</param>
        void WriteMessage(string message, LogLevel level = LogLevel.Verbose);

        /// <summary>
        /// Записывает сообщение с форматированием в журнал логов.
        /// </summary>
        /// <param name="message">Сообщение.</param>
        /// <param name="params">Параметры для форматированного сообщения.</param>
        void WriteFormatMessage(string message, params object[] @params);
    }

    /// <summary>
    /// Уровень важности сообщения.
    /// </summary>
    public enum LogLevel
    {
        Critical = 0,
        Error = 1,
        Warning = 2,
        Info = 3,
        Verbose = 4
    }
}