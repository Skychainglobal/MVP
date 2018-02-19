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
    /// Представляет запрос тренировки нейросети.
    /// </summary>
    public class SkyTrainRequest : SkyObject<SkyTrainRequest, SkyTrainRequestEntity, ISkyTrainRequest>, ISkyTrainRequest
    {
        internal SkyTrainRequest(SkyTrainRequestEntity entity, SkyObjectAdapter<SkyTrainRequest, SkyTrainRequestEntity, ISkyTrainRequest> adapter)
            : base(entity, adapter)
        {
        }


        /// <summary>
        /// Идентификатор нейросети, для которой выполняется тренировка.
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
        /// Нейросеть, для которой выполняется тренировка.
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
        /// Идентификатор тренируемой версии нейросети.
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
        /// Тренируемая версия нейросети.
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
        /// Идентификатор схемы тренировки.
        /// </summary>
        internal int TrainSchemeID
        {
            get { return this.Entity.TrainSchemeID; }
            set
            {
                this.Entity.TrainSchemeID = value;
                this.__init_TrainScheme = false;
            }
        }


        private bool __init_TrainScheme = false;
        private SkyTrainScheme _TrainScheme;
        /// <summary>
        /// Схема тренировки.
        /// Возвращает null в случае удаления схемы тренировки нейросети.
        /// </summary>
        public SkyTrainScheme TrainScheme
        {
            get
            {
                if (!__init_TrainScheme)
                {
                    _TrainScheme = this.Context.ObjectAdapters.TrainSchemes.GetObject(this.TrainSchemeID, false);
                    __init_TrainScheme = true;
                }
                return _TrainScheme;
            }
        }


        /// <summary>
        /// Идентификатор состояния нейросети, на основе которого выполняется тренировка.
        /// </summary>
        internal int InitialStateID
        {
            get { return this.Entity.InitialStateID; }
            set
            {
                this.Entity.InitialStateID = value;
                this.__init_InitialState = false;
            }
        }


        private bool __init_InitialState = false;
        private SkyNetworkState _InitialState;
        /// <summary>
        /// Состояние нейросети, на основе которого выполняется тренировка.
        /// Возвращает null в случае, если тренировка является первичной.
        /// </summary>
        public SkyNetworkState InitialState
        {
            get
            {
                if (!__init_InitialState)
                {
                    _InitialState = null;
                    if (this.InitialStateID > 0)
                        _InitialState = this.Context.ObjectAdapters.NetworkStates.GetObject(this.InitialStateID, false);
                    __init_InitialState = true;
                }
                return _InitialState;
            }
        }


        /// <summary>
        /// Состояние нейросети, сформированное в результате тренировки.
        /// </summary>
        internal int ResultStateID
        {
            get { return this.Entity.ResultStateID; }
            set
            {
                this.Entity.ResultStateID = value;
                this.__init_ResultState = false;
            }
        }

		/// <summary>
		/// Описание результирующего состояния.
		/// </summary>
		public string ResultStateDescription
		{
			get { return this.Entity.ResultStateDescription; }
			set { this.Entity.ResultStateDescription = value; }
		}


		private bool __init_ResultState = false;
        private SkyNetworkState _ResultState;
        /// <summary>
        /// Состояние нейросети, сформированное в результате тренировки.
        /// </summary>
        public SkyNetworkState ResultState
        {
            get
            {
                if (!__init_ResultState)
                {
                    _ResultState = this.Context.ObjectAdapters.NetworkStates.GetObject(this.ResultStateID, true);
                    __init_ResultState = true;
                }
                return _ResultState;
            }
        }


        /// <summary>
        /// Идентификатор пользователя, выполняющего тренировку нейросети.
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
        /// Пользователь, выполняющий тренировку нейросети.
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
        /// Статус запроса тренировки.
        /// </summary>
        public SkyTrainRequestStatus Status
        {
            get { return this.Entity.Status; }
            internal set { this.Entity.Status = value; }
        }


        /// <summary>
        /// Проверяет наличие сформированного состояния нейросети.
        /// </summary>
        private void CheckResultState()
        {
            if (this.ResultState == null)
                throw new Exception("Result state is empty.");
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
        /// Лог, записываемый во время выполнения тренировки.
        /// </summary>
        public string TrainLog
        {
            get { return this.Entity.TrainLog; }
            private set { this.Entity.TrainLog = value; }
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
            if (this.ResultStateID > 0)
                throw new Exception("The result state can only be set when the query is completed.");

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
        private void CheckStatus(SkyTrainRequestStatus status)
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
            this.CheckStatus(SkyTrainRequestStatus.Creating);

            //проверяем, что данные результата не заполнены.
            this.CheckCompletedPropertiesUndefined();

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
            this.CheckStatus(SkyTrainRequestStatus.Creating);

            //проверяем, что данные результата не заполнены.
            this.CheckCompletedPropertiesUndefined();

            //устанавливаем статус обработки.
            this.Status = SkyTrainRequestStatus.Processing;

            //устанавливаем время начала обработки.
            this.StartTime = DateTime.Now;

            //выполняем обновление объекта.
            using (OperationContext updateAllowedContext = this.ContextManager.BeginContext("UpdateAllowedContext"))
                this.Update();
        }


        /// <summary>
        /// Выполняет тренировку.
        /// </summary>
        public void Train()
        {
            //проверяем, что запрос существует.
            this.CheckExists();

            //проверяем, что тренировка ещё не была выполнена.
            if (this.ResultStateID != 0)
                throw new Exception(string.Format("The train request with ID={0} has already trained.", this.ID));

			//выполняем непосредственно тренировку сети.
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
			//var operationContext = new PublicAPI.TrainOperationContext(this.NetworkVersion, this.TrainScheme, this.InitialState, this.ResultStateDescription);
			var operationContext = new PublicAPI.TrainOperationContext(this);
			var trainScheme = new PublicAPI.TrainScheme(this.TrainScheme);
			neuralNetwork.Train(operationContext, trainScheme);
			
			//в качестве результата получаем состояние нейросети.
			byte[] networkStateData = operationContext.SavedState;
			if (networkStateData == null)
				throw new Exception("Training did not return the result. Call IOperationContext.SaveModel to save result.");

            //создаём состояние нейросети.
            SkyNetworkState networkState = this.NetworkVersion.CreateNetworkState(this.TrainScheme.ID, this.InitialStateID);
			networkState.Description = this.ResultStateDescription;
			using (MemoryStream networkStateStream = new MemoryStream(networkStateData))
			{
				networkStateStream.Position = 0;
				networkState.StateData.Upload("NetworkStateData", networkStateStream);
			}

            //сохраняем состояние.
            networkState.Update();

            //устанавливаем ссылку на сформированное состояние.
            this.ResultStateID = networkState.ID;

            //устанавливаем признак успешной обработки запроса.
            this.Succeed = true;
        }


        /// <summary>
        /// Записывает сообщение в лог тренировки нейросети.
        /// </summary>
        /// <param name="message">Текст записываемого в лог сообщения.</param>
        public void WriteTrainLog(string message)
        {
            if (string.IsNullOrEmpty(message))
                throw new ArgumentNullException("message");

            //проверяем статус запроса.
            this.CheckStatus(SkyTrainRequestStatus.Processing);

            //формируем итоговое сообщение.
            this.TrainLog = string.Concat(this.TrainLog, message);

            //выполняем обновление объекта.
            using (OperationContext updateAllowedContext = this.ContextManager.BeginContext("UpdateAllowedContext"))
                this.Update();
        }

        /// <summary>
        /// Устанавливает статус завершения запроса к нейросети.
        /// </summary>
        public void Complete()
        {
            //проверяем статус запроса.
            this.CheckStatus(SkyTrainRequestStatus.Processing);

            //проверяем наличие данных результата.
            if (this.Succeed)
                this.CheckResultState();

            //изменяем статус запроса.
            this.Status = SkyTrainRequestStatus.Completed;

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
            this.TrainScheme.CheckExists();
            object checkObj = this.User;

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


        ISkyNetwork ISkyTrainRequest.Network => this.Network;

        ISkyNetworkVersion ISkyTrainRequest.NetworkVersion => this.NetworkVersion;

        ISkyTrainScheme ISkyTrainRequest.TrainScheme => this.TrainScheme;

        ISkyNetworkState ISkyTrainRequest.InitialState => this.InitialState;

        ISkyNetworkState ISkyTrainRequest.ResultState => this.ResultState;

        ISkyUser ISkyTrainRequest.User => this.User;
    }
}
