using System;
using System.Collections.Generic;

namespace Storage.Lib
{
    /// <summary>
    /// Опции операции загрузки файла.
    /// </summary>
    public class GetFileOptions
    {
        /// <summary>
        /// Загружать содержимое файла?
        /// </summary>
        public bool LoadContent { get; set; }
    }
}