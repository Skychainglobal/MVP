using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Storage.Engine
{
    /// <summary>
    /// Метаданные потоковой передачи данных.
    /// </summary>
    public interface IAccessToken
    {
        /// <summary>
        /// Уникальный идентификатор токена.
        /// </summary>
        Guid UniqueID { get; }

        /// <summary>
        /// Дата до которой токен считается валидным.
        /// </summary>
        DateTime Expired { get; }

        /// <summary>
        /// Токен валиден?
        /// </summary>
        bool IsExpired { get; }
    }
}