using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Storage.Engine;
using Storage.Lib;

namespace Storage.Data.Blob
{
    /// <summary>
    /// Контейнер блобов.
    /// </summary>
    internal class BlobContainer
    {
        private static readonly object _commonLocker = new object();

        /// <summary>
        /// Словарь с кол-вом залоченных блобов для каждого контейнера.
        /// </summary>
        private static readonly Dictionary<string, ContainerLockInfo> _containerStreamingLocks = new Dictionary<string, ContainerLockInfo>();

        /// <summary>
        /// Словарь объектов синхронизации потоков для операции lock.
        /// Для каждого файла контейнера создается свой объект синхронизации.
        /// </summary>
        private static Dictionary<string, ContainerLocker> _containerLockers = new Dictionary<string, ContainerLocker>();

        /// <summary>
        /// Словарь объектов синхронизации потоков для единичного блоба.
        /// Для каждого блоба создается свой объект синхронизации.
        /// </summary>
        private static Dictionary<string, object> _blobLockers = new Dictionary<string, object>();


        public BlobContainer(BlobDataAdapter dataAdapter, IBlobContainerMetadata metadata)
        {
            if (dataAdapter == null)
                throw new ArgumentNullException("dataAdapter");

            if (metadata == null)
                throw new ArgumentNullException("metadata");

            this.DataAdapter = dataAdapter;
            this.Metadata = metadata;
        }

        /// <summary>
        /// Адаптер данных.
        /// </summary>
        public BlobDataAdapter DataAdapter { get; private set; }

        /// <summary>
        /// Метаданные контейнера.
        /// </summary>
        public IBlobContainerMetadata Metadata { get; private set; }

        private bool __init_ID;
        private int _ID;
        /// <summary>
        /// Идентификатор контейнера.
        /// </summary>
        public int ID
        {
            get
            {
                if (!__init_ID)
                {
                    _ID = this.Metadata.ID;
                    if (_ID < 1)
                        throw new Exception(string.Format("Для контейнера блобов должен быть задан идентификатор."));

                    __init_ID = true;
                }
                return _ID;
            }
        }

        private bool __init_Path;
        private string _Path;
        /// <summary>
        /// Путь до директории контейнера.
        /// </summary>
        private string Path
        {
            get
            {
                if (!__init_Path)
                {
                    _Path = this.Metadata.Path;
                    if (string.IsNullOrEmpty(_Path))
                        throw new Exception(string.Format("Для контейнера с идентификатором {0} не задан путь.",
                            this.ID));

                    __init_Path = true;
                }
                return _Path;
            }
        }

        private bool __init_Logger;
        private ILogProvider _Logger;
        /// <summary>
        /// Логгер.
        /// </summary>
        internal ILogProvider Logger
        {
            get
            {
                if (!__init_Logger)
                {
                    _Logger = this.DataAdapter.Logger;
                    __init_Logger = true;
                }
                return _Logger;
            }
        }

        private bool __init_Directory;
        private DirectoryInfo _Directory;
        /// <summary>
        /// Директория контейнера.
        /// </summary>
        internal DirectoryInfo Directory
        {
            get
            {
                if (!__init_Directory)
                {
                    this.Logger.WriteMessage("BlobContainer.Directory:Начало получения директории контейнера");

                    _Directory = new DirectoryInfo(this.Path);
                    if (!_Directory.Exists)
                    {
                        object directoryLocker = this.GetContainerLocker(ContainerLockerType.Directory);
                        lock (directoryLocker)
                        {
                            if (!_Directory.Exists)
                                _Directory = System.IO.Directory.CreateDirectory(this.Path);
                        }
                    }
                    this.Logger.WriteMessage("BlobContainer.Directory:Окончание получения директории контейнера");


                    __init_Directory = true;
                }
                return _Directory;
            }
        }


        /// <summary>
        /// Записывает файл на физический носитель.
        /// </summary>
        /// <param name="fileMetadata">Метаданные файла.</param>
        /// <param name="stream">Содержимое файла.</param>
        /// <param name="remoteTimeCreated">Время создания версии с удаленного узла.</param>
        /// <returns></returns>
        public BlobFileInfo Write(IBlobFileMetadata fileMetadata, Stream stream, DateTime? remoteTimeCreated = null)
        {
            if (fileMetadata == null)
                throw new ArgumentNullException("fileMetadata");

            if (string.IsNullOrEmpty(fileMetadata.Name))
                throw new ArgumentNullException("fileMetadata.Name");

            if (stream == null)
                throw new ArgumentNullException("stream");

            this.Logger.WriteFormatMessage("BlobContainer.Write:Начало записи содержимого файла в блоб, file.UniqueID: {0}", fileMetadata.UniqueID);
            BlobFileInfo blobFileInfo;

            //блоб, на который была получена блокировка для потоковой передачи
            Blob streamingLockedBlob = null;

            try
            {
                //текущий блоб для записи файла
                Blob currentBlob = null;
                List<Blob> activeBlobs = null;

                //сначала необходимо определить возможно ли осуществить запись
                //в потоковом режиме (не исчерпан ли лимит)
                bool isStreamed = !stream.CanSeek;
                bool useStreamingLocksLimit = this.DataAdapter.AllowedStreamingLocksLimit != 1;

                if (isStreamed && useStreamingLocksLimit)
                {
                    //проверка на то, что хотя бы 1а потоковая передача может пройти
                    this.ValidateStreamingLocks();

                    //в ситуации, когда одновременно пришло несколько событий потоковой передачи данных
                    //чтобы не лочить все блобы контейнера под запись потоковых файлов и оставить место
                    //под запись буферных файлов делаем проверку на % залоченных под потоковую передачу блобов
                    //потоковая передача данных подразумевает передачу больших файлов
                    object containerLocker = this.GetContainerLocker(ContainerLockerType.StreamingWaiting);
                    lock (containerLocker)
                    {
                        //последняя дата загрузки активных блобов
                        DateTime lastLoadActiveBlobsTime = DateTime.Now;
                        //дата начала ожидания потоковой блокировки текущим потоком
                        DateTime startWaitingStreamingLockTime = DateTime.Now;

                        //вычисляем возможна ли потоковая передача
                        bool limitOverflowed = true;
                        while (limitOverflowed)
                        {
                            //ограничение на продолжительность ожидания блокировки
                            TimeSpan waitingTime = DateTime.Now.Subtract(startWaitingStreamingLockTime);
                            if (waitingTime.TotalMinutes >= BlobConsts.Timeouts.WaitingStreamingLockMinutes)
                                throw new Exception(string.Format("Истекло время, отведенное на операцию ожидания потоковой блокировки блоба. Дата начала ожидания получения потоковой блокировки {0:dd.MM.yyyy HH:mm:ss}",
                                    startWaitingStreamingLockTime));

                            //перед очередной операцией проверки лимита на потоковые передачи
                            //чтобы не грузить ЦП усыпляем поток
                            Thread.Sleep(50);

                            bool reloadActiveBlobs = activeBlobs == null
                                || DateTime.Now.Subtract(lastLoadActiveBlobsTime).TotalMinutes >= BlobConsts.Timeouts.ReloadActiveBlobsMinutes;

                            if (!reloadActiveBlobs)
                            {
                                //проверяем, что есть закрытые блобы, в выборке, которую получили ранее
                                foreach (Blob tmpBlob in activeBlobs)
                                {
                                    if (tmpBlob.Metadata.Closed)
                                    {
                                        //среди активных блобов появился закрытый во время текущей сессии
                                        //=> необходимо перезагрузить актуальный список.
                                        reloadActiveBlobs = true;
                                        break;
                                    }
                                }
                            }

                            //за время ожидания возмно состав активных блобов изменился
                            if (reloadActiveBlobs)
                            {
                                //выбираем заново список активных блобов
                                activeBlobs = this.GetActiveBlobs();
                                lastLoadActiveBlobsTime = DateTime.Now;
                            }

                            //кол-во текущих локов, вызванных потоковой передачей
                            //т.к. словарь статический, то lock выше на конейнер недостаточно
                            //поэтому получаем число активных потоковых локов через ф-ию, внутри которой общий lock
                            int containerStreamingLocks = this.GetContainerStreamingLocksCount(activeBlobs);

                            //количество локов, которые будут, если мы заблокируем еще 1 файл текущей передачей
                            int containerStreamingLocksWithNewLock = containerStreamingLocks + 1;
                            double containerStreamingLocksUsage = (double)containerStreamingLocksWithNewLock / this.DataAdapter.ActiveBlobsCount;

                            limitOverflowed = containerStreamingLocksUsage > this.DataAdapter.AllowedStreamingLocksLimit;
                        }

                        //вышли из цикла ожидания, значит освободилась как минимум одна ячейка 
                        //для выполнения операции потоковой передачи
                        //сразу занимаем эту ячейку
                        currentBlob = this.GetRandomBlob(activeBlobs);
                        this.AddStreamingLock(currentBlob);
                        streamingLockedBlob = currentBlob;
                    }
                }
                else
                {
                    activeBlobs = this.GetActiveBlobs();
                    currentBlob = this.GetRandomBlob(activeBlobs);
                }

                //т.к. общий лок на static переменную это узкое место для записи в разные файлы
                //разные диски и т.п., то на каждый файл в статическом словаре создается свой объект синхронизации
                //таким образом получается, что запись в файл1 НЕ блокирует запись в файл2
                object blobFileLocker = this.GetBlobLocker(currentBlob);
                lock (blobFileLocker)
                {
                    //запись файла в блоб
                    blobFileInfo = currentBlob.Write(fileMetadata, stream, remoteTimeCreated);
                }
            }
            catch (Exception ex)
            {
                string errorMessage = string.Format("BlobContainer.Write:Ошибка записи содержимого файла в блоб, file.UniqueID: {0}, текст ошибки: {1}",
                    fileMetadata.UniqueID,
                    ex);
                this.Logger.WriteMessage(errorMessage, LogLevel.Error);
                throw ex;
            }
            finally
            {
                if (streamingLockedBlob != null)
                {
                    //за данную операцию была получена блокировка для потоковой передачи.
                    //снимаем ее независимо от успешного результата выполнения операции.
                    this.RemoveStreamingLock(streamingLockedBlob);
                }
            }

            this.Logger.WriteFormatMessage("BlobContainer.Write:Окончание записи содержимого файла в блоб, file.UniqueID: {0}", fileMetadata.UniqueID);

            return blobFileInfo;
        }

        #region Streaming Locks
        private void ValidateStreamingLocks()
        {
            double minStreamingLocks = 1.0d / this.DataAdapter.ActiveBlobsCount;
            if (minStreamingLocks > this.DataAdapter.AllowedStreamingLocksLimit)
            {
                //даже минимальное кол-во потоковых блокировок не умещается в лимит
                throw new Exception(string.Format("При текущей настройке кол-ва активных блобов ({0}) невозможно наложить даже одну блокировку, т.к. это будет больше доступного лимита блокировок для потоковой передачи ({1}%)",
                    this.DataAdapter.ActiveBlobsCount,
                    (this.DataAdapter.AllowedStreamingLocksLimit * 100)));
            }
        }

        private Blob GetRandomBlob(List<Blob> activeBlobs)
        {
            if (activeBlobs == null)
                throw new ArgumentNullException("activeBlobs");

            List<Blob> resultCollection = null;
            if (activeBlobs.Count > 1)
            {
                //выборка блоба без потоковой блокировки
                string pathLower = this.Path.ToLower();
                List<Blob> blobsWithoutStreamingLock = new List<Blob>();
                object containerStreamingLocker = this.GetContainerLocker(ContainerLockerType.Streaming);
                lock (containerStreamingLocker)
                {
                    if (_containerStreamingLocks.ContainsKey(pathLower))
                    {
                        ContainerLockInfo containerLockInfo = _containerStreamingLocks[pathLower];
                        foreach (Blob blob in activeBlobs)
                        {
                            if (!containerLockInfo.LockExists(blob))
                                blobsWithoutStreamingLock.Add(blob);
                        }
                    }
                }

                if (blobsWithoutStreamingLock.Count > 0)
                    resultCollection = blobsWithoutStreamingLock;
                else
                {
                    //нет свободных от потоковых блокировок блобов, выбираем из тех, что заблокированы
                    //какой из таких блобов освободится первее для записи невозможно, т.к. заренее неизвестен
                    //объем потоковых данных
                    resultCollection = activeBlobs;
                }
            }
            else
                resultCollection = activeBlobs;


            Blob randomBlob = null;
            if (resultCollection.Count == 0)
                throw new Exception(string.Format("Невозможно выбрать произвольный блоб из пустой коллекции"));
            else if (resultCollection.Count == 1)
            {
                //в коллекции 1 блоб, его и выбираем
                randomBlob = resultCollection.First();
            }
            else
            {
                //выбираем рандомный блоб для записи.
                //для равномерного распределения файлов используем хеш-код GUID
                int index = Math.Abs(Guid.NewGuid().GetHashCode() % resultCollection.Count);
                randomBlob = resultCollection[index];
            }

            return randomBlob;
        }

        /// <summary>
        /// Возвращает для текущейго контейнера кол-во блокировок, вызванных потоковой передачей.
        /// </summary>
        /// <returns></returns>
        private int GetContainerStreamingLocksCount(ICollection<Blob> activeBlobs)
        {
            if (activeBlobs == null)
                throw new ArgumentNullException("activeBlobs");

            int containerStreamingLocks = 0;
            if (activeBlobs.Count > 0)
            {
                string pathLower = this.Path.ToLower();
                lock (_commonLocker)
                {
                    if (_containerStreamingLocks.ContainsKey(pathLower))
                    {
                        ContainerLockInfo containerLockInfo = _containerStreamingLocks[pathLower];
                        foreach (Blob blob in activeBlobs)
                        {
                            if (containerLockInfo.LockExists(blob))
                                containerStreamingLocks++;
                        }
                    }
                }
            }

            return containerStreamingLocks;
        }

        /// <summary>
        /// Увеличивает на единицу кол-во блокировок для текущейго контейнера, вызванных потоковой передачей.
        /// </summary>
        /// <returns></returns>
        private void AddStreamingLock(Blob blob)
        {
            if (blob == null)
                throw new ArgumentNullException("blob");

            if (this.ID != blob.Container.ID)
                throw new Exception(string.Format("Передан блоб, который не принадлежит текущему контейнеру"));

            string pathLower = this.Path.ToLower();
            lock (_commonLocker)
            {
                ContainerLockInfo containerLockInfo;
                if (_containerStreamingLocks.ContainsKey(pathLower))
                    containerLockInfo = _containerStreamingLocks[pathLower];
                else
                {
                    containerLockInfo = new ContainerLockInfo();
                    _containerStreamingLocks.Add(pathLower, containerLockInfo);
                }

                containerLockInfo.AddStreamingLock(blob);
            }
        }

        /// <summary>
        /// Снимает потоковую блокировку с конкретного блоба текущего контейнера.
        /// </summary>
        /// <param name="blob">Блоб.</param>
        private void RemoveStreamingLock(Blob blob)
        {
            if (blob == null)
                throw new ArgumentNullException("blob");

            if (this.ID != blob.Container.ID)
                throw new Exception(string.Format("Передан блоб, который не принадлежит текущему контейнеру"));

            string pathLower = this.Path.ToLower();
            lock (_commonLocker)
            {
                if (_containerStreamingLocks.ContainsKey(pathLower))
                {
                    ContainerLockInfo containerLockInfo = _containerStreamingLocks[pathLower];
                    containerLockInfo.RemoveStreamingLock(blob);
                }
            }
        }
        #endregion

        /// <summary>
        /// Возвращает объект синхронизации для операций над единичным контейнером.
        /// </summary>
        /// <returns></returns>
        internal object GetContainerLocker(ContainerLockerType lockerType)
        {
            ContainerLocker containerLocker = null;
            string path = this.Path.ToLower();
            if (_containerLockers.ContainsKey(path))
                containerLocker = _containerLockers[path];
            else
            {
                lock (_commonLocker)
                {
                    if (_containerLockers.ContainsKey(path))
                        containerLocker = _containerLockers[path];
                    else
                    {
                        containerLocker = new ContainerLocker();
                        _containerLockers.Add(path, containerLocker);
                    }
                }
            }

            object lockTarget = null;
            switch (lockerType)
            {
                case ContainerLockerType.Directory:
                    lockTarget = containerLocker.DirectoryLocker;
                    break;
                case ContainerLockerType.Metadata:
                    lockTarget = containerLocker.MetadataLocker;
                    break;
                case ContainerLockerType.StreamingWaiting:
                    lockTarget = containerLocker.StreamingWaitingLocker;
                    break;
                case ContainerLockerType.Streaming:
                    lockTarget = containerLocker.StreamingLocker;
                    break;
                default:
                    throw new Exception(string.Format("Неизвестный тип объекта синхронизации: {0}",
                        lockerType));
            }

            return lockTarget;
        }

        /// <summary>
        /// Возвращает объект синхронизации для операции записи в физический файл блоба.
        /// </summary>
        /// <returns></returns>
        internal object GetBlobLocker(Blob blob)
        {
            if (blob == null)
                throw new ArgumentNullException("blob");

            object blobFileLocker = null;
            string path = blob.File.FullName.ToLower();

            if (_blobLockers.ContainsKey(path))
                blobFileLocker = _blobLockers[path];
            else
            {
                lock (_commonLocker)
                {
                    if (_blobLockers.ContainsKey(path))
                        blobFileLocker = _blobLockers[path];
                    else
                    {
                        blobFileLocker = new object();
                        _blobLockers.Add(path, blobFileLocker);
                    }
                }
            }

            return blobFileLocker;
        }

        /// <summary>
        /// Возвращает текущий блоб для записи.
        /// </summary>
        /// <returns></returns>
        private List<Blob> GetActiveBlobs()
        {
            this.Logger.WriteMessage("BlobContainer.GetActiveBlobs:Начало определения списка активных блобов.");

            List<Blob> blobs = new List<Blob>();

            ICollection<IBlobMetadata> activeBlobs = this.DataAdapter.BlobMetadataAdapter.GetActiveBlobs(this.ID);
            bool createNewBlobsRequired = this.DataAdapter.ActiveBlobsCount > activeBlobs.Count;

            if (createNewBlobsRequired)
            {
                object metadataLocker = this.GetContainerLocker(ContainerLockerType.Metadata);
                lock (metadataLocker)
                {
                    activeBlobs = this.DataAdapter.BlobMetadataAdapter.GetActiveBlobs(this.ID);

                    if (this.DataAdapter.ActiveBlobsCount > blobs.Count)
                    {
                        int count = this.DataAdapter.ActiveBlobsCount - activeBlobs.Count;
                        for (int i = 0; i < count; i++)
                        {
                            Blob newBlob = this.CreateBlob();
                            blobs.Add(newBlob);
                        }
                    }
                }
            }

            if (activeBlobs != null)
            {
                foreach (IBlobMetadata metadata in activeBlobs)
                {
                    Blob tmpBlob = new Blob(this, metadata);
                    blobs.Add(tmpBlob);
                }
            }

            this.Logger.WriteFormatMessage("BlobContainer.GetActiveBlobs:Окончание определения списка активных блобов.");

            return blobs;
        }

        private Blob CreateBlob()
        {
            //создание нового блоба
            string name = string.Format("{0}_{1}",
                BlobConsts.Blobs.Name,
                Guid.NewGuid().ToString(),
                BlobConsts.Blobs.Extension);

            IBlobMetadata blobMetadata = this.DataAdapter.BlobMetadataAdapter.CreateBlob(name, this.ID);
            this.DataAdapter.BlobMetadataAdapter.SaveBlob(blobMetadata);

            Blob newBlob = new Blob(this, blobMetadata);

            DateTime timeCreated = DateTime.Now;
            //создаем физический файл блоба
            //т.к. это первичное созадние файла, то "шарить" файл не нужно
            //пишем в него заголовок и закрываем поток
            using (FileStream fs = newBlob.File.Open(FileMode.Append, FileAccess.Write, FileShare.None))
            {
                if (fs.Position != 0)
                    throw new Exception(string.Format("Невозможно записать заголовок блоба в непустой блоб. Заголовок блоба должен быть записан в начало блоба!"));

                //записываем заголовок в начало блоба
                byte[] allBlobHeaderBytes = new byte[BlobConsts.Blobs.BlobHeaderSize];
                //фиксированные байты заголовка блоба
                byte[] fixedHeaderBytes = Encoding.UTF8.GetBytes(BlobConsts.Blobs.BlobSystemHeader);
                Array.Copy(fixedHeaderBytes, 0, allBlobHeaderBytes, 0, fixedHeaderBytes.Length);
                int destinationOffset = fixedHeaderBytes.Length;

                //версия блоба
                int blobHeaderVersion = BlobConsts.Blobs.BlobHeaderCurrentVersion;
                byte[] blobHeaderVersionBytes = BitConverter.GetBytes(blobHeaderVersion);
                Array.Copy(blobHeaderVersionBytes, 0, allBlobHeaderBytes, destinationOffset, blobHeaderVersionBytes.Length);
                destinationOffset += blobHeaderVersionBytes.Length;

                BlobeHeaderV1 blobHeader = new BlobeHeaderV1()
                {
                    ID = newBlob.ID,
                    Name = newBlob.File.Name,
                    FullName = newBlob.File.FullName,
                    ContainerID = newBlob.Metadata.ContainerID,
                    MachineName = Environment.MachineName,
                    TimeCreated = timeCreated
                };

                string blobHeaderJson = JsonDataSerializer.SerializeJson(blobHeader);
                byte[] blobHeaderBytes = Encoding.UTF8.GetBytes(blobHeaderJson);
                Array.Copy(blobHeaderBytes, 0, allBlobHeaderBytes, destinationOffset, blobHeaderBytes.Length);

                //запись всего заголовка блоба в файл блоба
                fs.Write(allBlobHeaderBytes, 0, allBlobHeaderBytes.Length);
            }

            return newBlob;
        }
    }
}