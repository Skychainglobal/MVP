using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Storage.Engine
{
    /// <summary>
    /// Интерфейс для слоя движка представляющий св-ва объекта метаданных.
    /// </summary>
    public interface IEngineObjectMetadata
    {
        /// <summary>
        /// Локальный идентификатор объекта метаданных.
        /// </summary>
        int ID { get; }
    }
}