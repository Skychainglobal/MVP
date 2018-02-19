using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Storage.Engine
{
    /// <summary>
    /// Токен.
    /// </summary>
    public interface IToken
    {
        /// <summary>
        /// Уникальный идентификатор токена.
        /// </summary>
        Guid UniqueID { get; }

        /// <summary>
        /// Дата, до которой выдан токен.
        /// </summary>
        DateTime Expired { get; }

        /// <summary>
        /// Возвращает результат валидации токена.
        /// </summary>
        /// <returns></returns>
        bool IsValid();
    }
}