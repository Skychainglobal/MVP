using Skychain.Models.Entity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Skychain.Models.Implementation
{
    /// <summary>
    /// Представляет запрос пользователя к нейросети.
    /// </summary>
    public class SkyNetworkRequest : SkyObject<SkyNetworkRequest, SkyNetworkRequestEntity, ISkyNetworkRequest>, ISkyNetworkRequest
    {
        internal SkyNetworkRequest(SkyNetworkRequestEntity entity, SkyObjectAdapter<SkyNetworkRequest, SkyNetworkRequestEntity, ISkyNetworkRequest> adapter)
            : base(entity, adapter)
        {
        }


        /// <summary>
        /// Идентификатор нейросети, к которой выполняется запрос.
        /// </summary>
        internal int NetworkID
        {
            get { return this.Entity.NetworkID; }
            set
            {
                this.Entity.NetworkID = value;
                this.__init_Network = false;
            }
        }


        private bool __init_Network = false;
        private SkyNetwork _Network;
        /// <summary>
        /// Нейросеть, к которой выполняется запрос.
        /// </summary>
        public SkyNetwork Network
        {
            get
            {
                if (!__init_Network)
                {
                    _Network = this.Context.ObjectAdapters.Networks.GetObject(this.NetworkID, true);
                    __init_Network = true;
                }
                return _Network;
            }
        }



        /// <summary>
        /// Идентификатор используемой версии нейросети.
        /// </summary>
        internal int NetworkVersionID
        {
            get { return this.Entity.NetworkVersionID; }
            set
            {
                this.Entity.NetworkVersionID = value;
                this.__init_NetworkVersion = false;
            }
        }


        private bool __init_NetworkVersion = false;
        private SkyNetworkVersion _NetworkVersion;
        /// <summary>
        /// Используемая версия нейросети.
        /// </summary>
        public SkyNetworkVersion NetworkVersion
        {
            get
            {
                if (!__init_NetworkVersion)
                {
                    _NetworkVersion = this.Context.ObjectAdapters.NetworkVersions.GetObject(this.NetworkVersionID, true);
                    __init_NetworkVersion = true;
                }
                return _NetworkVersion;
            }
        }


        /// <summary>
        /// Состояние нейросети, на основе которого формируется результат.
        /// </summary>
        internal int NetworkStateID
        {
            get { return this.Entity.NetworkStateID; }
            set
            {
                this.Entity.NetworkStateID = value;
                this.__init_NetworkState = false;
            }
        }


        private bool __init_NetworkState = false;
        private SkyNetworkState _NetworkState;
        /// <summary>
        /// Состояние нейросети, на основе которого формируется результат.
        /// </summary>
        public SkyNetworkState NetworkState
        {
            get
            {
                if (!__init_NetworkState)
                {
                    _NetworkState = this.Context.ObjectAdapters.NetworkStates.GetObject(this.NetworkStateID, true);
                    __init_NetworkState = true;
                }
                return _NetworkState;
            }
        }


        /// <summary>
        /// Идентификатор пользователя, формирующего запрос к нейросети.
        /// </summary>
        internal Guid UserID
        {
            get { return this.Entity.UserID; }
            set
            {
                this.Entity.UserID = value;
                this.__init_User = false;
            }
        }


        private bool __init_User = false;
        private SkyUser _User;
        /// <summary>
        /// Пользователь, формирующий запрос к нейросети.
        /// </summary>
        public SkyUser User
        {
            get
            {
                if (!__init_User)
                {
                    _User = this.Context.GetUser(this.UserID);
                    __init_User = true;
                }
                return _User;
            }
        }


        /// <summary>
        /// Статус запроса к нейросети.
        /// </summary>
        public SkyNetworkRequestStatus Status
        {
            get { return this.Entity.Status; }
            internal set { this.Entity.Status = value; }
        }


        /// <summary>
        /// Стоимость использования нейросети на момент создания запроса.
        /// </summary>
        public decimal NetworkCost
        {
            get { return this.Entity.NetworkCost; }
            internal set { this.Entity.NetworkCost = value; }
        }


        /// <summary>
        /// Данные, введённые пользователем, в соответствии с форматом манифеста.
        /// </summary>
        public string InputData
        {
            get { return this.Entity.InputData; }
            set
            {
                this.CheckIsNew();
                this.Entity.InputData = value;
            }
        }


        /// <summary>
        /// Проверяет наличие данных, введённых пользователем, при формировании запроса.
        /// </summary>
        public void CheckInputData()
        {
            if (string.IsNullOrEmpty(this.InputData))
                throw new Exception("Request input data is empty.");
        }


        /// <summary>
        /// Создаёт экземпляр для загрузки файла, выбранного пользователем на форме входных данных запроса к нейросети.
        /// </summary>
        public SkyFile CreateInputFile()
        {
            this.CheckIsNew();
            SkyFile inputFile = new SkyFile(Guid.Empty, "NetworkRequestInputData", true, this.Context);
            this.InputFiles.Add(inputFile);
            return inputFile;
        }


        private bool __init_InputFiles = false;
        private List<SkyFile> _InputFiles;
        /// <summary>
        /// Коллекция файлов, загруженных пользователем при формировании запроса.
        /// </summary>
        private List<SkyFile> InputFiles
        {
            get
            {
                if (!__init_InputFiles)
                {
                    _InputFiles = new List<SkyFile>();
                    if (!this.IsNew && !string.IsNullOrEmpty(this.Entity.InputFiles))
                        _InputFiles.AddRange(this.Entity.InputFiles.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                                .Select(x => new SkyFile(new Guid(x), "NetworkRequestInputData", true, this.Context)));

                    __init_InputFiles = true;
                }
                return _InputFiles;
            }
        }


        /// <summary>
        /// Возвращает экземпляр файла, выбранного пользователем на форме входных данных запроса к нейросети.
        /// Метод всегда возвращает значение, отличающееся от null.
        /// </summary>
        /// <param name="fileID">Идентификатор файла.</param>
        public SkyFile GetInputFile(Guid fileID)
        {
            if (fileID == null)
                throw new ArgumentNullException("fileID");

            this.CheckExists();
            return new SkyFile(fileID, "NetworkRequestInputData", true, this.Context);
        }


        /// <summary>
        /// Данные результата запроса, в соответствии с форматом манифеста.
        /// </summary>
        public string ResultData
        {
            get { return this.Entity.ResultData; }
            set { this.Entity.ResultData = value; }
        }


        /// <summary>
        /// Проверяет наличие данных результата запроса.
        /// </summary>
        public void CheckResultData()
        {
            if (string.IsNullOrEmpty(this.ResultData))
                throw new Exception("Request result data is empty.");
        }


        /// <summary>
        /// Время начала запроса.
        /// </summary>
        public DateTime StartTime
        {
            get
            {
                return this.Entity.StartTime.HasValue ?
                  this.Entity.StartTime.Value : DateTime.MinValue;
            }
            private set
            {
                if (value != DateTime.MinValue)
                    this.Entity.StartTime = value;
                else
                    this.Entity.StartTime = null;
            }
        }


        /// <summary>
        /// Время начала окончания.
        /// </summary>
        public DateTime EndTime
        {
            get
            {
                return this.Entity.EndTime.HasValue ?
                  this.Entity.EndTime.Value : DateTime.MinValue;
            }
            private set
            {
                if (value != DateTime.MinValue)
                    this.Entity.EndTime = value;
                else
                    this.Entity.EndTime = null;
            }
        }


        /// <summary>
        /// Сообщение об ошибке, возникшей при выполнении запроса.
        /// </summary>
        public string ErrorMessage
        {
            get { return this.Entity.ErrorMessage; }
            set { this.Entity.ErrorMessage = value; }
        }


        /// <summary>
        /// Возвращает true, если обработка запроса была выполнена без ошибок.
        /// </summary>
        public bool Succeed
        {
            get { return this.Entity.Succeed; }
            private set { this.Entity.Succeed = value; }
        }


        /// <summary>
        /// Проверяет, что незаполнены свойства, которые должны быть заполнены при завершении запроса.
        /// </summary>
        private void CheckCompletedPropertiesUndefined()
        {
            //ругаемся при попытке изменения данных результата.
            if (!string.IsNullOrEmpty(this.ResultData))
                throw new Exception("The result data can only be set when the query is completed.");

            //ругаемся при попытке изменения сообщения об ошибке.
            if (!string.IsNullOrEmpty(this.ErrorMessage))
                throw new Exception("The error message can only be set when the query is completed.");

            //ругаемся, если установлен признак Succeed.
            if (this.Succeed)
                throw new Exception("The succeed flag can only be set when the query is completed.");
        }


        /// <summary>
        /// Проверет установленный статус запроса.
        /// Генерирует исключение, если статус не соответствует переданному статусу.
        /// </summary>
        /// <param name="status">Проверяемый статус.</param>
        private void CheckStatus(SkyNetworkRequestStatus status)
        {
            if (this.Status != status)
                throw new Exception(string.Format("Request status [{0}] is not equals to expected status [{1}].",
                    this.Status, status));
        }


        /// <summary>
        /// Создаёт запрос к нейросети.
        /// </summary>
        public void Create()
        {
            //проверяем, что объект является новым.
            this.CheckIsNew();

            //проверяем статус запроса.
            this.CheckStatus(SkyNetworkRequestStatus.Creating);

            //проверяем, что данные результата не заполнены.
            this.CheckCompletedPropertiesUndefined();

            //сохраняем стоимость запроса сети на момент его создания.
            this.NetworkCost = this.Network.Cost;

            //сохраняем идентификаторы загруженных файлов.
            this.Entity.InputFiles = string.Join(";", this.InputFiles
                .Where(x => x.Exists)
                .Select(x => x.ID.ToString()).ToArray());
            if (string.IsNullOrEmpty(this.Entity.InputFiles))
                this.Entity.InputFiles = null;

            //выполняем обновление объекта.
            using (OperationContext updateAllowedContext = this.ContextManager.BeginContext("UpdateAllowedContext"))
                this.Update();
        }


        /// <summary>
        /// Устаналивает статус обработки запроса нейросетью в данный момент времени.
        /// </summary>
        public void Process()
        {
            //проверяем, что запрос существует.
            this.CheckExists();

            //проверяем статус запроса.
            this.CheckStatus(SkyNetworkRequestStatus.Creating);

            //проверяем, что данные результата не заполнены.
            this.CheckCompletedPropertiesUndefined();

            //устанавливаем статус обработки.
            this.Status = SkyNetworkRequestStatus.Processing;

            //устанавливаем время начала обработки.
            this.StartTime = DateTime.Now;

            //выполняем обновление объекта.
            using (OperationContext updateAllowedContext = this.ContextManager.BeginContext("UpdateAllowedContext"))
                this.Update();
        }


        /// <summary>
        /// Вычисляет результат обработки запроса к нейросети.
        /// </summary>
        public void ComputeResult()
        {
			//формируем объект с входными данными
			var inputFormData = PublicAPI.InferenceOperationContext.CreateInputData(this.InputData);

			//подгружаем библиотеку
			var library = this.NetworkVersion.TensorLibrary.Content;

			//загружаем
			var assembly = Assembly.Load(library);

			//тип интерфейса сети
			var neuralNetworkInterfaceType = typeof(SkychainAPI.INeuralNetwork);

			//ищем реализацию интерфейса
			var neuralNetworkImplementationType = assembly.GetTypes().FirstOrDefault(type => neuralNetworkInterfaceType.IsAssignableFrom(type));
			if (neuralNetworkImplementationType == null)
				throw new Exception($"Implementation of interface '{neuralNetworkInterfaceType.AssemblyQualifiedName}' not found in assembly '{assembly.FullName}'");

			//создаём реализацию
			var neuralNetwork = Activator.CreateInstance(neuralNetworkImplementationType) as SkychainAPI.INeuralNetwork;

			//вызываем реализацию
			var operationContext = new PublicAPI.InferenceOperationContext(this);

			this.ResultData = neuralNetwork.Inference(operationContext, inputFormData);

            //устанавливаем признак успешной обработки запроса.
            this.Succeed = true;
        }
		
		/// <summary>
		/// Устанавливает статус завершения запроса к нейросети.
		/// </summary>
		public void Complete()
        {
            //проверяем статус запроса.
            this.CheckStatus(SkyNetworkRequestStatus.Processing);

            //проверяем наличие данных результата.
            if (this.Succeed)
                this.CheckResultData();

            //изменяем статус запроса.
            this.Status = SkyNetworkRequestStatus.Completed;

            //устанавливаем время окончания обработки.
            this.EndTime = DateTime.Now;

            //выполняем обновление объекта.
            using (OperationContext updateAllowedContext = this.ContextManager.BeginContext("UpdateAllowedContext"))
                this.Update();
        }


        /// <summary>
        /// Обновляет объект в базе данных.
        /// </summary>
        public override void Update()
        {
            //провряем доступность обновления экземпляра.
            this.ContextManager.CheckContext("UpdateAllowedContext");

            //проверяем наличие связанных объектов.
            this.Network.CheckExists();
            this.NetworkVersion.CheckExists();
            this.NetworkState.CheckExists();
            object checkObj = this.User;

            //всегда проверяем наличие входных данных.
            this.CheckInputData();

            //обновляем объект.
            base.Update();
        }


        /// <summary>
        /// Удаляет объект и все связанные с ним дочерние объекты.
        /// </summary>
        public override void Delete()
        {
            throw new NotSupportedException("The network request can only be deleted in the context of deleting the network state.");
        }


        ISkyNetwork ISkyNetworkRequest.Network => this.Network;

        ISkyNetworkVersion ISkyNetworkRequest.NetworkVersion => this.NetworkVersion;

        ISkyNetworkState ISkyNetworkRequest.NetworkState => this.NetworkState;

        ISkyUser ISkyNetworkRequest.User => this.User;

        ISkyFile ISkyNetworkRequest.CreateInputFile()
        {
            return this.CreateInputFile();
        }

        ISkyFile ISkyNetworkRequest.GetInputFile(Guid fileID)
        {
            return this.GetInputFile(fileID);
        }
    }
}
