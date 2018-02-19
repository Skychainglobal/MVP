using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Storage.Lib;

namespace Storage.Engine
{
    /// <summary>
    /// Интерфейс адаптера сессионных ссылок.
    /// </summary>
    public interface ISessionResolver
    {
        /// <summary>
        /// Возвращает сессионную ссылку по токену.
        /// </summary>
        /// <param name="token">Токен файла.</param>
        /// <returns></returns>
        string GetSessionLink(IFileToken token);

        /// <summary>
        /// Возвращает токен файла по ссылке.
        /// </summary>
        /// <param name="sessionLink">Сессионная ссылка.</param>
        /// <returns></returns>
        IFileToken Resolve(string sessionLink);
    }
}