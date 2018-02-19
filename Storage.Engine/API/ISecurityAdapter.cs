using System;
using System.Security.Principal;

namespace Storage.Engine
{
    /// <summary>
    /// Интерфейс адаптера безопастности.
    /// </summary>
    public interface ISecurityAdapter
    {
        /// <summary>
        /// Проверяет доступ на токен.
        /// </summary>
        /// <param name="token">Токен доступа к файлу.</param>
        /// <param name="identity">Объект, идентифицирующий пользователя.</param>
        /// <returns></returns>
        bool ValidateToken(IFileToken token, IIdentity identity);

        /// <summary>
        /// Возвращает строковый идентификатор безопасности для пользователя. 
        /// </summary>
        /// <param name="userIdentifier">Строка, идентифицирующая пользователя.</param>
        /// <returns></returns>
        string GetSecurityIdentifier(string userIdentifier);
    }
}
