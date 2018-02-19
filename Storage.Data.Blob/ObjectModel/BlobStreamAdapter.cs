using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Storage.Engine;
using Storage.Lib;

namespace Storage.Data.Blob
{
    /// <summary>
    /// Структура файла, которая хранится в блобе.
    /// Представляет собой файл с заголовком, метаданными и содержимым.
    /// </summary>
    internal class BlobStreamAdapter
    {
        /// <summary>
        /// Фиксированные байты системного заголовка.
        /// </summary>
        internal static byte[] SystemHeaderFixedBytes;

        /// <summary>
        /// Полная длина системного заголока.
        /// </summary>
        internal static int SystemHeaderLength;

        /*
        Схема единичного файла:
         [SystemHeader][FileHeader][Content]
            - [SystemHeader] = [{FixBytes}{FileHeaderSize}{HeaderHash}{FileContentHash}{HeaderVersion}{ContentLength}{92}]
            - [FileHeader] = [JsonHeaderString]
            - [Content] = byte[];
        */
        static BlobStreamAdapter()
        {
            SystemHeaderFixedBytes = Encoding.UTF8.GetBytes(BlobConsts.BlobFile.FileSystemHeader);
            BlobStreamAdapter.SystemHeaderLength =
                SystemHeaderFixedBytes.Length //системные байты
                + BlobConsts.BlobFile.HeaderSizeBytesLength //байты размера заголовка
                + BlobConsts.BlobFile.AllHashBytesLength //байты хеша заголовка и хеша содержимого
                + BlobConsts.BlobFile.HeaderVersionBytesLength //байты версии заголовка
                + BlobConsts.BlobFile.ContentSizeBytesLength //байты размера содержимого файла
                + 92; //фиксированное кол-во байт на "перспективу" (начинали со 100)
        }

        internal BlobStreamAdapter(Stream blobStream)
        {
            if (blobStream == null)
                throw new ArgumentNullException("blobStream");

            this.BlobStream = blobStream;
        }

        private bool __init_HashAlgorithm;
        private HashAlgorithm _HashAlgorithm;
        /// <summary>
        /// Алгоритм хеширования.
        /// </summary>
        private HashAlgorithm HashAlgorithm
        {
            get
            {
                if (!__init_HashAlgorithm)
                {
                    _HashAlgorithm = this.CreateHashAlgorithm();
                    __init_HashAlgorithm = true;
                }
                return _HashAlgorithm;
            }
        }

        /// <summary>
        /// Поток блоба.
        /// </summary>
        private Stream BlobStream { get; set; }

        private bool __init_BinarySerializer;
        private BinaryFormatter _BinarySerializer;
        /// <summary>
        /// Сериализатор данных в двоичном формате.
        /// </summary>
        private BinaryFormatter BinarySerializer
        {
            get
            {
                if (!__init_BinarySerializer)
                {
                    _BinarySerializer = new BinaryFormatter();
                    __init_BinarySerializer = true;
                }
                return _BinarySerializer;
            }
        }


        private HashAlgorithm CreateHashAlgorithm()
        {
            return MD5.Create();
        }

        internal byte[] Read(IBlobFileVersionMetadata versionMetadata)
        {
            if (versionMetadata == null)
                throw new ArgumentNullException("versionMetadata");

            //содержимое файла
            return this.ReadContent(
                versionMetadata.FileMetadata.UniqueID,
                versionMetadata.UniqueID,
                versionMetadata.BlobStartPosition,
                versionMetadata.BlobEndPosition);
        }

        internal byte[] Read(IBlobFileMetadata fileMetadata)
        {
            if (fileMetadata == null)
                throw new ArgumentNullException("fileMetadata");

            //содержимое файла
            return this.ReadContent(
                fileMetadata.UniqueID,
                fileMetadata.VersionUniqueID,
                fileMetadata.BlobStartPosition,
                fileMetadata.BlobEndPosition);
        }

        private byte[] ReadContent(Guid fileUniqueID, Guid fileVersionUniqueID, long blobStartPosition, long blobEndPosition)
        {
            if (fileUniqueID == Guid.Empty)
                throw new ArgumentNullException("fileUniqueID");

            if (fileVersionUniqueID == Guid.Empty)
                throw new ArgumentNullException("fileVersionUniqueID");

            if (blobStartPosition < 0)
                throw new ArgumentNullException("blobStartPosition");

            if (blobEndPosition < 1)
                throw new ArgumentNullException("blobEndPosition");

            if (blobStartPosition >= blobEndPosition)
                throw new Exception(string.Format("Индекс начала файла не может быть равным или больше индекса окончания файла"));

            long length = blobEndPosition - blobStartPosition;
            if (blobEndPosition > this.BlobStream.Length)
                throw new IndexOutOfRangeException("Индекс окончания файла превысил размер блоба");

            //сырые данные файла (со всеми заголовками и хешами)
            byte[] rawData = new byte[length];
            if (length > Int32.MaxValue)
                throw new Exception(string.Format("Файлы размером более 2GB в данный момент не поддерживаются"));
            else
            {
                this.BlobStream.Seek(blobStartPosition, SeekOrigin.Begin);
                this.BlobStream.Read(rawData, 0, rawData.Length);
            }

            //1 - проверка системного префикса.
            //системный заголовок занимает первые _systemHeaderLength байт
            for (int i = 0; i < SystemHeaderFixedBytes.Length; i++)
            {
                if (SystemHeaderFixedBytes[i] != rawData[i])
                    throw new Exception(string.Format("Битый заголовок файла (позиция {0}). Не удалось прочитать системный заголовок файла.",
                        i));
            }

            //проверка заголовка.
            object header = this.GetFileHeader(rawData);
            IBlobFileHeader blobHeader = (IBlobFileHeader)header;

            long contentLength = blobHeader.ContentLength;
            if (contentLength == 0)
                throw new Exception(string.Format("Не удалось считать размер содержимого файла"));

            byte[] content = new byte[contentLength];
            Array.Copy(rawData, BlobStreamAdapter.SystemHeaderLength + blobHeader.HeaderLength, content, 0, contentLength);

            //проверка идентификаторов версий
            IFileHeader fileHeader = (IFileHeader)header;
            if (fileUniqueID != fileHeader.UniqueID)
                throw new Exception(string.Format("Идентификатор файла в метаданных не совпадает с идентификатором файла в блобе"));

            if (fileVersionUniqueID != fileHeader.VersionUniqueID)
                throw new Exception(string.Format("Идентификатор версии файла в метаданных не совпадает с идентификатором версии файла в блобе"));

            byte[] originalHeaderHash = new byte[16];//хеш заголовка файла из самого блоба
            Array.Copy(rawData, SystemHeaderFixedBytes.Length + BlobConsts.BlobFile.HeaderSizeBytesLength, originalHeaderHash, 0, originalHeaderHash.Length);
            //хеш вычисленный по байтам заголовка
            byte[] currentHeaderHash = this.HashAlgorithm.ComputeHash(rawData, BlobStreamAdapter.SystemHeaderLength, blobHeader.HeaderLength);
            bool headerHashInvalid = !originalHeaderHash.SequenceEqual(currentHeaderHash);
            if (headerHashInvalid)
                throw new Exception(string.Format("Битый заголовок файла. Не удалось прочитать системный заголовок файла."));

            //3 - проверка хеша содержимого.
            byte[] originalContentHash = new byte[16];//хеш содержимого файла из самого блоба
            Array.Copy(rawData, SystemHeaderFixedBytes.Length + BlobConsts.BlobFile.HeaderSizeBytesLength + originalHeaderHash.Length, originalContentHash, 0, originalContentHash.Length);
            byte[] currentContentHash = this.HashAlgorithm.ComputeHash(content);
            bool contentHashInvalid = !originalContentHash.SequenceEqual(currentContentHash);
            if (contentHashInvalid)
                throw new Exception(string.Format("Битое содержимое файла. Не удалось прочитать содержимое файла."));

            rawData = null;
            originalHeaderHash = null;
            currentHeaderHash = null;
            originalContentHash = null;
            currentContentHash = null;

            return content;
        }

        /// <summary>
        /// Сохраняет содержимое файла с заголовками в поток.
        /// </summary>
        /// <param name="fileMetadata">Метаданные файла.</param>
        /// <param name="stream">Содержимое файла.</param>
        /// <param name="versionTimeCreated">Дата создания версии файла.</param>
        internal void Write(IBlobFileMetadata fileMetadata, Stream stream, DateTime versionTimeCreated)
        {
            if (fileMetadata == null)
                throw new ArgumentNullException("fileMetadata");

            if (fileMetadata.UniqueID == Guid.Empty)
                throw new ArgumentNullException("fileMetadata.UniqueID");

            if (string.IsNullOrEmpty(fileMetadata.Name))
                throw new ArgumentNullException("fileMetadata.Name");

            if (stream == null)
                throw new ArgumentNullException("stream");

            if (!this.BlobStream.CanWrite)
                throw new Exception(string.Format("Для сохранения файла в поток необходимо передать поток с правами на запись"));

            bool contentLengthCalculated = false;
            long contentLength = 0;
            if (stream.CanSeek)
            {
                contentLength = stream.Length;
                contentLengthCalculated = true;
            }

            byte[] systemHeaderInfoBytes = new byte[BlobStreamAdapter.SystemHeaderLength];
            byte[] headerBytes = null;
            int headerLength = 0;

            //запись текущей версии заголовка
            long blobOffset = this.BlobStream.Position;
            using (MemoryStream fileHeaderStream = new MemoryStream())
            {
                //сериализация заголовка
                this.SerializeHeader(fileHeaderStream, fileMetadata, contentLength, versionTimeCreated);

                //после сериализации заголовка становится известна длина
                headerLength = (int)fileHeaderStream.Length;
                headerBytes = new byte[headerLength];
                fileHeaderStream.Position = 0;
                fileHeaderStream.Read(headerBytes, 0, headerLength);
            }

            //хеш заголовка и содерижмого
            byte[] headerHash = this.HashAlgorithm.ComputeHash(headerBytes);

            //длина заголовка
            byte[] headerLengthBytes = BitConverter.GetBytes(headerLength);
            //номер версии заголовка
            byte[] headerVersionBytes = BitConverter.GetBytes(BlobConsts.BlobFile.FileHeaderCurrentVersion);


            //заполнение системного заголовка
            //а - фиксированные байты
            int headerCursor = 0;
            Array.Copy(SystemHeaderFixedBytes, 0, systemHeaderInfoBytes, headerCursor, SystemHeaderFixedBytes.Length);
            headerCursor += SystemHeaderFixedBytes.Length;

            //б - размер заголовка файла
            Array.Copy(headerLengthBytes, 0, systemHeaderInfoBytes, headerCursor, headerLengthBytes.Length);
            headerCursor += headerLengthBytes.Length;

            //в - хеш заголовка файла
            Array.Copy(headerHash, 0, systemHeaderInfoBytes, headerCursor, headerHash.Length);
            headerCursor += headerHash.Length;

            //г - хеш содержимого файла
            long tmpcontentHashPosition = this.BlobStream.Position + headerCursor; //абсолютная позиция в файле байтов длины содержимого
            //пустой хеш содержимого, рассчитывается после записи
            byte[] contentHash = new byte[16];
            Array.Copy(contentHash, 0, systemHeaderInfoBytes, headerCursor, contentHash.Length);
            headerCursor += contentHash.Length;

            //д - версия заголовка файла
            Array.Copy(headerVersionBytes, 0, systemHeaderInfoBytes, headerCursor, headerVersionBytes.Length);
            headerCursor += headerVersionBytes.Length;

            //е - размер содержимого файла
            long tmpContentSizePosition = this.BlobStream.Position + headerCursor; //абсолютная позиция в файле байтов длины содержимого
            byte[] contentLengthBytes = BitConverter.GetBytes(contentLength);
            Array.Copy(contentLengthBytes, 0, systemHeaderInfoBytes, headerCursor, contentLengthBytes.Length);
            headerCursor += contentLengthBytes.Length;

            //1 - запись системного заголовка
            this.BlobStream.Write(systemHeaderInfoBytes, 0, systemHeaderInfoBytes.Length);

            //2 - запись заголовка файла
            this.BlobStream.Write(headerBytes, 0, headerBytes.Length);

            //3 - запись содержимого
            long startContentPosition = this.BlobStream.Position;

            int readBufferSize = 4 * 1024;
            byte[] buffer = new byte[readBufferSize];
            int read = 0;
            HashAlgorithm tmpHashAlgorithm = this.CreateHashAlgorithm();

            byte[] writeBuffer = new byte[readBufferSize];

            int writePosition = 0;
            while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
            {
                tmpHashAlgorithm.TransformBlock(buffer, 0, read, null, 0);

                if ((writePosition + read) >= writeBuffer.Length)
                {
                    //дозаполняем буфер
                    int restBufferSize = writeBuffer.Length - writePosition;
                    Array.Copy(buffer, 0, writeBuffer, writePosition, restBufferSize);
                    writePosition += restBufferSize;

                    //скидываем буфер записи на диск
                    this.BlobStream.Write(writeBuffer, 0, writePosition);

                    int newSessionPosition = read - restBufferSize;
                    if (newSessionPosition > 0)
                    {
                        //остальные данные опять загоняем в буфер
                        Array.Copy(buffer, restBufferSize, writeBuffer, 0, newSessionPosition);
                    }

                    writePosition = newSessionPosition;
                }
                else
                {
                    //записываем только в буфер, на диск не скидываем
                    Array.Copy(buffer, 0, writeBuffer, writePosition, read);
                    writePosition += read;
                }
            }

            if (writePosition > 0)
                this.BlobStream.Write(writeBuffer, 0, writePosition);

            tmpHashAlgorithm.TransformFinalBlock(buffer, 0, read);


            //физически записанная длина файла
            long endContentPosition = this.BlobStream.Position;
            contentLength = endContentPosition - startContentPosition;

            //4 - переписываем 16 пустых байт хеша содержимого, после его вычисления
            contentHash = tmpHashAlgorithm.Hash;
            long contentHashBytesAbsolutePosition = FileStructure.GetContentHashBytesAbsolutePosition(blobOffset);
            this.BlobStream.Seek(contentHashBytesAbsolutePosition, SeekOrigin.Begin);
            this.BlobStream.Write(contentHash, 0, contentHash.Length);


            //5 - в 100 зарезервированных байт пишем длину содержимого (ранее была в заголовке)
            //теперь обязательно есть в системном заголовке, а в файловом заголовке есть, если пришел буферный поток
            //делаем это только в случае потоковой передачи, если длина известна сразу, то она и запишется сразу
            if (!contentLengthCalculated)
            {
                long contentLengBytesAbsolutePosition = FileStructure.GetContentLengBytesAbsolutePosition(blobOffset);
                contentLengthBytes = BitConverter.GetBytes(contentLength);
                this.BlobStream.Seek(contentLengBytesAbsolutePosition, SeekOrigin.Begin);
                this.BlobStream.Write(contentLengthBytes, 0, contentLengthBytes.Length);
            }

            fileMetadata.Size = contentLength;
        }

        private void SerializeHeader(MemoryStream fileHeaderStream, IBlobFileMetadata fileMetadata, long contentLength, DateTime versionTimeCreated)
        {
            if (fileHeaderStream == null)
                throw new ArgumentNullException("fileHeaderStream");

            if (fileMetadata == null)
                throw new ArgumentNullException("fileMetadata");

            //текущая версия заголовка = BlobConsts.BlobFile.FileHeaderCurrentVersion
            JsonBlobFileHeaderV1 header = new JsonBlobFileHeaderV1()
            {
                ContentAbsoluteStartPosition = this.BlobStream.Position,
                ContentLength = contentLength,
                FileName = fileMetadata.Name,
                FolderUrl = fileMetadata.FolderMetadata.Url,
                UniqueID = fileMetadata.UniqueID,
                TimeCreated = versionTimeCreated,
                VersionUniqueID = fileMetadata.VersionUniqueID,
            };

            JsonDataSerializer.Serialize(fileHeaderStream, header);
        }

        private object GetFileHeader(byte[] contentWithHeadersData)
        {
            if (contentWithHeadersData == null)
                throw new ArgumentNullException("fileData");

            object header = null;

            //сразу за системным заголовком 4 байта, в которых указана длина заголовка файла
            //длина заголовка файла
            int headerLength = BitConverter.ToInt32(contentWithHeadersData, SystemHeaderFixedBytes.Length);
            if (headerLength < 1)
                throw new Exception(string.Format("Не удалось считать длину заголовка файла"));

            byte[] headerBytes = new byte[headerLength];
            Array.Copy(contentWithHeadersData, BlobStreamAdapter.SystemHeaderLength, headerBytes, 0, headerLength);

            int headerVersionNumber = BitConverter.ToInt32(contentWithHeadersData, SystemHeaderFixedBytes.Length + BlobConsts.BlobFile.HeaderSizeBytesLength + BlobConsts.BlobFile.AllHashBytesLength);
            using (MemoryStream headerStream = new MemoryStream(headerBytes))
            {
                headerStream.Position = 0;
                JsonBlobFileHeaderV1 typedHeader = JsonDataSerializer.Deserialize<JsonBlobFileHeaderV1>(headerStream);
                typedHeader.HeaderLength = headerLength;

                if (typedHeader.ContentLength == 0)
                {
                    //длина на момент записи заголовка не была вычислина (потоковая передача)
                    //смотрим эту длину по системному заголовку
                    long contentLengBytesAbsolutePosition = FileStructure.GetContentLengBytesAbsolutePosition();
                    typedHeader.ContentLength = BitConverter.ToInt64(contentWithHeadersData, (int)contentLengBytesAbsolutePosition);
                }

                header = typedHeader;
            }

            return header;
        }

        internal object GetFileHeader(long startPosition)
        {
            if (startPosition < 0)
                throw new ArgumentNullException("startPosition");

            //читаем системный заголовок
            this.BlobStream.Seek(startPosition, SeekOrigin.Begin);
            byte[] systemHeaderBytes = new byte[BlobStreamAdapter.SystemHeaderLength];
            this.BlobStream.Read(systemHeaderBytes, 0, BlobStreamAdapter.SystemHeaderLength);

            //проверка целостности системного заголовка
            for (int i = 0; i < SystemHeaderFixedBytes.Length; i++)
            {
                if (SystemHeaderFixedBytes[i] != systemHeaderBytes[i])
                    throw new Exception(string.Format("Битый заголовок файла (позиция {0}). Не удалось прочитать системный заголовок файла.",
                        i));
            }

            //целостность системного заголовока не нарушена, значит можно поднять заголовок файла
            //сразу за фиксированной частью системного заголовка есть 4 байта, в которых указана длина заголовка файла
            //длина заголовка файла
            int headerLength = BitConverter.ToInt32(systemHeaderBytes, SystemHeaderFixedBytes.Length);
            if (headerLength < 1)
                throw new Exception(string.Format("Не удалось считать длину заголовка файла"));

            byte[] headerBytes = new byte[headerLength];
            this.BlobStream.Seek(startPosition + BlobStreamAdapter.SystemHeaderLength, SeekOrigin.Begin);
            this.BlobStream.Read(headerBytes, 0, headerBytes.Length);

            int headerVersionNumber = BitConverter.ToInt32(systemHeaderBytes, SystemHeaderFixedBytes.Length + BlobConsts.BlobFile.HeaderSizeBytesLength + BlobConsts.BlobFile.AllHashBytesLength);
            object header = null;

            using (MemoryStream headerStream = new MemoryStream(headerBytes))
            {
                headerStream.Position = 0;
                JsonBlobFileHeaderV1 typedHeader = JsonDataSerializer.Deserialize<JsonBlobFileHeaderV1>(headerStream);
                typedHeader.HeaderLength = headerLength;

                if (typedHeader.ContentLength == 0)
                {
                    //длина на момент записи заголовка не была вычислина (потоковая передача)
                    //смотрим эту длину по системному заголовку
                    long contentLengBytesAbsolutePosition = FileStructure.GetContentLengBytesAbsolutePosition();
                    typedHeader.ContentLength = BitConverter.ToInt64(systemHeaderBytes, (int)contentLengBytesAbsolutePosition);
                }

                header = typedHeader;
            }

            return header;
        }

        /// <summary>
        /// Получает позицию первого файла на основе структуры блоба.
        /// </summary>
        /// <returns></returns>
        internal long GetFirstFilePosition()
        {
            long firstFileStartPosition = 0;
            byte[] blobSystemHeaderBytes = Encoding.UTF8.GetBytes(BlobConsts.Blobs.BlobSystemHeader);

            //если в блобе есть столько байт, сколько нужно на системный заголовок
            //если меньше, то значит записали файл без заголовка в блоб и файл очень маленький, меньше размера заголовка блоба
            if (this.BlobStream.Length >= blobSystemHeaderBytes.Length)
            {
                //считываем из файла первые n байт и сравниваем с системным заголовком
                byte[] buffer = new byte[blobSystemHeaderBytes.Length];
                this.BlobStream.Read(buffer, 0, buffer.Length);
                if (buffer.SequenceEqual(blobSystemHeaderBytes))
                {
                    //последовательности идентичны, значит блоб начинается с заголовка, а не сразу с файла
                    //в этом случае конец размера всего зарезервированного места под системный заголовок
                    //будем считать началом первого файла
                    firstFileStartPosition = BlobConsts.Blobs.BlobHeaderSize;
                }
            }

            return firstFileStartPosition;
        }

        internal PartitionStream ReadStream(IBlobFileVersionMetadata blobFileVersionMetadata)
        {
            if (blobFileVersionMetadata == null)
                throw new ArgumentNullException("blobFileVersionMetadata");

            if (blobFileVersionMetadata.BlobStartPosition >= blobFileVersionMetadata.BlobEndPosition)
                throw new Exception(string.Format("Индекс начала файла не может быть равным или больше индекса окончания файла"));

            //поток закрывает объект, предоставляющий средства потоковой передачи
            IBlobFileHeader fileHeader = (IBlobFileHeader)this.GetFileHeader(blobFileVersionMetadata.BlobStartPosition);
            long contentStartPosition = blobFileVersionMetadata.BlobStartPosition + BlobStreamAdapter.SystemHeaderLength + fileHeader.HeaderLength;
            PartitionStream stream = new PartitionStream(this.BlobStream, contentStartPosition, blobFileVersionMetadata.Size);

            return stream;
        }

        internal PartitionStream ReadStream(IBlobFileMetadata fileMetadata)
        {
            if (fileMetadata == null)
                throw new ArgumentNullException("fileMetadata");

            if (fileMetadata.BlobStartPosition >= fileMetadata.BlobEndPosition)
                throw new Exception(string.Format("Индекс начала файла не может быть равным или больше индекса окончания файла"));

            //поток закрывает объект, предоставляющий средства потоковой передачи
            IBlobFileHeader fileHeader = (IBlobFileHeader)this.GetFileHeader(fileMetadata.BlobStartPosition);
            long contentStartPosition = fileMetadata.BlobStartPosition + BlobStreamAdapter.SystemHeaderLength + fileHeader.HeaderLength;
            PartitionStream stream = new PartitionStream(this.BlobStream, contentStartPosition, fileMetadata.Size);

            return stream;
        }
    }
}