using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Storage.Data.Blob
{
    /// <summary>
    /// Класс блокировки файла блоба.
    /// </summary>
    internal class ContainerLocker
    {
        public ContainerLocker() { }

        /// <summary>
        /// Объект синхронизации потоков для операций с директорией контейнера.
        /// </summary>
        public object DirectoryLocker = new object();

        /// <summary>
        /// Объект синхронизации потоков для операций с метаданными.
        /// </summary>
        public object MetadataLocker = new object();

        /// <summary>
        /// Объект синхронизации потоков для операции ожидания доступности потоковой передачи.
        /// </summary>
        public object StreamingWaitingLocker = new object();

        /// <summary>
        /// Объект синхронизации потоков для операции подсчета/редактирования кол-ва активных потоковых передач.
        /// </summary>
        public object StreamingLocker = new object();
    }

    internal enum ContainerLockerType
    {
        Directory = 0,
        Metadata = 1,
        StreamingWaiting = 2,
        Streaming = 3,
    }
}