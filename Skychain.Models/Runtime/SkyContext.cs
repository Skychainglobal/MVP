using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using SkyContextClass = Skychain.Models.Implementation.SkyContext;

namespace Skychain.Models.Runtime
{
    /// <summary>
    /// Содержит методы для работы с текущим контекстом системы.
    /// </summary>
    public class SkyContext
    {
        /// <summary>
        /// Возвращает текущий контекст выполнения http-запроса в рамках веб-приложения Skychain.
        /// Генерирует исключение в случае отсутствия контекста.
        /// </summary>
        public static ISkyContext Current
        {
            get { return RuntimeContext.Current.Properties.GetSingleton<ISkyContext>("SkyContext.Current", CreateCurrent); }
        }

        private static ISkyContext CreateCurrent(RuntimeContext context)
        {
            if (context.HttpContext != null)
                return SkyWebContext.Current.SkyContext;
            else
                return new SkyContextClass();
        }
    }
}
