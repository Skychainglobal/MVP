using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Storage.Data.Blob
{
    internal class BlobConsts
    {
        public class Blobs
        {
            public const string Name = "Blob";
            public const string Extension = "stor";

            /// <summary>
            /// Максимальный размер блоба.
            /// 50 * Int32.MaxValue, т.е. ~107 GB
            /// </summary>
            public const long DefaultMaxBlobSize = 107374182400;

            /// <summary>
            /// Кол-во активных блоб файлов для контейнера
            /// </summary>
            public const int DefaultActiveBlobsCount = 5;

            /// <summary>
            /// % от активных блобов, который может быть заблокирован операциями потоковой передачи данных.
            /// </summary>
            public const int AllowedStreamingLocksLimit = 50;

            /// <summary>
            /// Размер заголовка блоба.
            /// </summary>
            public const int BlobHeaderSize = 1024 * 1024;//1 Mb

            /// <summary>
            /// Текущая версия заголовка блоба.
            /// </summary>
            public const int BlobHeaderCurrentVersion = 1;

            /// <summary>
            /// Заголовок блоба.
            /// </summary>
            public const string BlobSystemHeader = "Storage.BlobSystemHeader_D8A53E06-9C88-4189-919E-2B75B06054AF_";

        }

        public class Timeouts
        {
            public const int ReloadActiveBlobsMinutes = 5;
            public const int WaitingStreamingLockMinutes = 30;
        }

        public class BlobFile
        {
            /// <summary>
            /// Системный заголовок файла.
            /// </summary>
            public const string FileSystemHeader = "Storage.FileSystemHeader_7785649D-5FD6-4485-A7A9-F9456D607DF0_";

            /// <summary>
            /// Количество байтов, которые отведены под размер заголовка файла и размер системного заголовка.
            /// Размер заголовка int - следовательно умещаемся в 4байта
            /// </summary>
            public const int HeaderSizeBytesLength = 4;

            /// <summary>
            /// Количество байтов, которые отведены под размер файла и заголовка.
            /// Размер файла long - следовательно умещаемся в 4байта
            /// </summary>
            public const int ContentSizeBytesLength = 8;

            /// <summary>
            /// Количество байтов, которые отведены под хеш заголовка и содержимого файла
            /// </summary>
            public const int AllHashBytesLength = 32;

            /// <summary>
            /// Количество байтов, которые отведены под версию заголовка
            /// </summary>
            public const int HeaderVersionBytesLength = 4;

            /// <summary>
            /// Текущая версия заголовка файлов.
            /// </summary>
            public const int FileHeaderCurrentVersion = 1;
        }

        public class Logs
        {
            public class Scopes
            {
                public const string DataBlobAdapter = "Data.Blob";
            }
        }
    }
}