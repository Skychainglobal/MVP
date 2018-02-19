using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skychain.Models.Services
{
    /// <summary>
    /// Представляет интерфейс запуска сервиса.
    /// </summary>
    public interface ISkyService
    {
        /// <summary>
        /// Запускает выполнение сервиса.
        /// </summary>
        void Run();
    }
}
