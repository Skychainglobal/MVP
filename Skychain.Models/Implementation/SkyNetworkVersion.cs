using Skychain.Models.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Storage.Lib;

namespace Skychain.Models.Implementation
{
    /// <summary>
    /// Представляет версию нейросети.
    /// </summary>
    public class SkyNetworkVersion : SkyObject<SkyNetworkVersion, SkyNetworkVersionEntity, ISkyNetworkVersion>, ISkyNetworkVersion
    {
        internal SkyNetworkVersion(SkyNetworkVersionEntity entity, SkyObjectAdapter<SkyNetworkVersion, SkyNetworkVersionEntity, ISkyNetworkVersion> adapter)
            : base(entity, adapter)
        {
        }

        /// <summary>
        /// Идентификатор нейросети.
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
        /// Нейросеть.
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
        /// Идентификатор версии в рамках нейросети.
        /// </summary>
        public int InternalID
        {
            get { return this.Entity.InternalID; }
            private set
            {
                if (value == 0)
                    throw new ArgumentNullException("value");

                this.Entity.InternalID = value;
            }
        }


        /// <summary>
        /// Проверяет наличие идентификатора версии в рамках нейросети.
        /// Генерирует исключение в случае отсутствия.
        /// </summary>
        public void CheckInternalID()
        {
            if (this.InternalID == 0)
                throw new Exception("Network version has no InternalID.");
        }


        /// <summary>
        /// Название набора данных.
        /// </summary>
        public string Name
        {
            get { return this.Entity.Name; }
            set { this.Entity.Name = value; }
        }


        /// <summary>
        /// Описание набора данных.
        /// </summary>
        public string Description
        {
            get { return this.Entity.Description; }
            set { this.Entity.Description = value; }
        }


        /// <summary>
        /// Возвращает true, если версия нейросети является активной.
        /// При сохранении установленного значения true в базу данных (при вызове Update), 
        /// данный флаг устанавливается в false у текущей активной версии нейросети, отличающейся от данной.
        /// </summary>
        public bool IsActive
        {
            get { return this.Entity.IsActive; }
            set
            {
                this.ChangeInfo.SetPropertyChange<bool>("IsActive", this.IsActive, value);
                this.Entity.IsActive = value;
            }
        }


        /// <summary>
        /// Проверяет, является ли версия нейросети активной, в ином случае генерирует исключение.
        /// </summary>
        public void CheckIsActive()
        {
            if (!this.IsActive)
                throw new Exception(string.Format("Network version [{0}] is not active.", this.Name));
        }


        /// <summary>
        /// Манифест введённых пользователем данных и данных результата.
        /// </summary>
        public string ManifestData
        {
            get { return this.Entity.ManifestData; }
            set
            {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentNullException("value");

                //ругаемся при попытке изменения манифеста существующей версии.
                if (!this.IsNew)
                    throw new Exception("Unable to modify the manifest data of the existing network version.");

                //устанавливаем значение манифеста.
                this.Entity.ManifestData = value;
            }
        }


        private bool __init_TensorLibrary = false;
        private SkyFile _TensorLibrary;
        /// <summary>
        /// Библиотека тензора нейросети.
        /// Свойство всегда возвращает существующий экземпляр класса.
        /// </summary>
        public SkyFile TensorLibrary
        {
            get
            {
                if (!__init_TensorLibrary)
                {
                    _TensorLibrary = new SkyFile(this.Entity.ExecutableLibraryID, "TensorLibraries", true, this.Context);
                    __init_TensorLibrary = true;
                }
                return _TensorLibrary;
            }
        }


        private bool __init_NetworkStates = false;
        private IEnumerable<SkyNetworkState> _NetworkStates;
        /// <summary>
        /// Состояния нейросети, соответствующие данной версии.
        /// </summary>
        public IEnumerable<SkyNetworkState> NetworkStates
        {
            get
            {
                if (!__init_NetworkStates)
                {
                    _NetworkStates = this.Context.ObjectAdapters.NetworkStates.GetObjects(dbSet => dbSet
                        .Where(x => x.NetworkVersionID == this.ID)
                        .OrderBy(x => x.ID));

                    __init_NetworkStates = true;
                }
                return _NetworkStates;
            }
        }

        internal void ResetNetworkStates()
        {
            this.__init_NetworkStates = false;
        }

        /// <summary>
        /// Создаёт новый экземпляр состояния нейросети для заданной схемы тренировок.
        /// Состояние нейросети может быть создано на основе другого состояния.
        /// </summary>
        /// <param name="trainSchemeID">Идентификатор схемы тренировок, для которого создаётся состояние нейросети.</param>
        /// <param name="initialStateID">Идентификатор состояния нейросети, на основе которого создаётся состояние. 
        /// Если состояние является первичным, необходимо передать значение 0.</param>
        public SkyNetworkState CreateNetworkState(int trainSchemeID, int initialStateID)
        {
            if (trainSchemeID == 0)
                throw new ArgumentNullException("trainSchemeID");

            this.CheckExists();

            //получаем схему тренировок.
            SkyTrainScheme trainScheme = this.Context.ObjectAdapters.TrainSchemes.GetObject(trainSchemeID, true);

            //проверяем, что схема тренировок принадлежит данной сети.
            if (trainScheme.Network.ID != this.Network.ID)
                throw new Exception(string.Format("The training scheme with ID={0} does not belong to the neural network with ID={1} to which this version of the neural network belongs.",
                    trainScheme.ID, this.Network.ID));

            //получаем инициализационное состояние.
            SkyNetworkState initialState = null;
            if (initialStateID > 0)
            {
                initialState = this.Context.ObjectAdapters.NetworkStates.GetObject(initialStateID, true);

                //проверяем, что инициализационное состояние принадлежит данной версии.
                if (initialState.NetworkVersion.ID != this.ID)
                    throw new Exception(string.Format("The network state with ID={0} does not belong to the version of the neural network with ID={1}.",
                        initialState.ID, this.ID));
            }

            //создаём экземпляр состояния нейросети.
            SkyNetworkState networkState = this.Context.ObjectAdapters.NetworkStates.CreateObject();
            networkState.NetworkID = this.Network.ID;
            networkState.NetworkVersionID = this.ID;
            networkState.TrainSchemeID = trainScheme.ID;
            networkState.InitialStateID = initialState != null ? initialState.ID : 0;

            //возвращаем созданный экземпляр.
            return networkState;
        }


        /// <summary>
        /// Возвращает новый идентификатор состояния нейросети в рамках версии нейросети.
        /// </summary>
        internal int GetStateInternalID()
        {
            this.CheckExists();
            this.Entity.StatesInternalIDCounter++;
            this.Update();
            return this.Entity.StatesInternalIDCounter;
        }


        /// <summary>
        /// Идентификатор активного состояния нейросети, используемого для выполнения запросов к данной версии нейросети.
        /// </summary>
        private int ActiveNetworkStateID
        {
            get { return this.Entity.ActiveNetworkStateID; }
            set { this.Entity.ActiveNetworkStateID = value; }
        }


        private bool __init_ActiveNetworkState = false;
        private SkyNetworkState _ActiveNetworkState;
        /// <summary>
        /// Активное состояние нейросети, используемое для выполнения запросов к данной версии нейросети.
        /// Возвращает null, если для активное состояние не задано для данной версии нейросети.
        /// </summary>
        public SkyNetworkState ActiveNetworkState
        {
            get
            {
                if (!__init_ActiveNetworkState)
                {
                    _ActiveNetworkState = null;
                    if (this.ActiveNetworkStateID > 0)
                        _ActiveNetworkState = this.Context.ObjectAdapters.NetworkStates.GetObject(this.ActiveNetworkStateID, false);
                    __init_ActiveNetworkState = true;
                }
                return _ActiveNetworkState;
            }
            private set
            {
                //обрабатываем состояние.
                SkyNetworkState networkState = value;
                if (networkState != null)
                {
                    //проверяем, что состояние принадлежит данной версии.
                    if (networkState.NetworkVersion.ID != this.ID)
                        throw new Exception(string.Format("The network state with ID={0} does not belong to the version of the neural network with ID={1}.",
                            networkState.ID, this.ID));

                    //проверяем наличие данных состояния.
                    networkState.CheckStateData();

                    //устанавливаем идентификатор активного состояния.
                    this.ActiveNetworkStateID = networkState.ID;
                }
                //сбрасываем идентификатор активного состояния.
                else
                    this.ActiveNetworkStateID = 0;

                _ActiveNetworkState = networkState;
                __init_ActiveNetworkState = true;
            }
        }

        /// <summary>
        /// Устанавливает активное состояние нейросети, используемое для выполнения запросов к данной версии нейросети.
        /// </summary>
        /// <param name="networkStateID">Идентификатор состояния нейросети, устанавливаемого в качестве активного для версии нейросети.</param>
        public void ChangeActiveNetworkState(int networkStateID)
        {
            if (networkStateID == 0)
                throw new ArgumentNullException("networkStateID");

            this.ActiveNetworkState = this.Context.ObjectAdapters.NetworkStates.GetObject(networkStateID, true);
        }

        /// <summary>
        /// Проверяет наличие активного состояния версии нейросети.
        /// Генерирует исключение в случае отсутствия активного состояния версии нейросети.
        /// </summary>
        public void CheckActiveNetworkState()
        {
            if (this.ActiveNetworkState == null)
                throw new Exception(string.Format("There is no active state for the neural network version with ID={0}.", this.ID));
        }


        /// <summary>
        /// Создаёт новый экземляр запроса к активному состоянию данной версии нейросети.
        /// </summary>
        /// <param name="user">Пользователь, формирующий запрос.</param>
        public SkyNetworkRequest CreateRequest(ISkyUser user)
        {
            this.CheckActiveNetworkState();
            return this.ActiveNetworkState.CreateRequest(user);
        }


        /// <summary>
        /// Создаёт новый запрос тренировки нейросети для заданной схемы тренировок.
        /// Тренировка нейросети может быть инициализирована на основе другого состояния нейросети.
        /// </summary>
        /// <param name="trainSchemeID">Идентификатор схемы тренировок.</param>
        /// <param name="initialStateID">Идентификатор состояния нейросети, на основе которого будет выполнена тренировка. 
        /// Если тренировка является первичной, необходимо передать значение 0.</param>
        /// <param name="user">Идентификатор схемы тренировок.</param>
        public SkyTrainRequest CreateTrainRequest(int trainSchemeID, int initialStateID, ISkyUser user)
        {
            if (trainSchemeID == 0)
                throw new ArgumentNullException("trainSchemeID");
            if (user == null)
                throw new ArgumentNullException("user");

            this.CheckExists();

            //получаем схему тренировок.
            SkyTrainScheme trainScheme = this.Context.ObjectAdapters.TrainSchemes.GetObject(trainSchemeID, true);

            //проверяем, что схема тренировок принадлежит данной сети.
            if (trainScheme.Network.ID != this.Network.ID)
                throw new Exception(string.Format("The training scheme with ID={0} does not belong to the neural network with ID={1} to which this version of the neural network belongs.",
                    trainScheme.ID, this.Network.ID));

            //получаем инициализационное состояние.
            SkyNetworkState initialState = null;
            if (initialStateID > 0)
            {
                initialState = this.Context.ObjectAdapters.NetworkStates.GetObject(initialStateID, true);

                //проверяем, что инициализационное состояние принадлежит данной версии.
                if (initialState.NetworkVersion.ID != this.ID)
                    throw new Exception(string.Format("The network state with ID={0} does not belong to the version of the neural network with ID={1}.",
                        initialState.ID, this.ID));
            }

            //создаём запрос тренировки.
            SkyTrainRequest trainRequest = this.Context.ObjectAdapters.TrainRequests.CreateObject();
            trainRequest.NetworkID = this.Network.ID;
            trainRequest.NetworkVersionID = this.ID;
            trainRequest.TrainSchemeID = trainScheme.ID;
            trainRequest.InitialStateID = initialState != null ? initialState.ID : 0;
            trainRequest.UserID = user.ID;
            trainRequest.Status = SkyTrainRequestStatus.Creating;

            //возвращаем созданный экземпляр.
            return trainRequest;
        }


        /// <summary>
        /// Обновляет объект в базе данных.
        /// </summary>
        public override void Update()
        {
            //проверяем наличие сети.
            this.Network.CheckExists();

            //обрабатываем создание новой версии нейросети.
            if (this.IsNew)
            {
                //проставляем идентификатор версии в рамках нейросети.
                this.InternalID = this.Network.GetVersionsInternalID();

                //проставляем ссылку на библиотеку тензора.
                this.TensorLibrary.CheckExists();
                this.Entity.ExecutableLibraryID = this.TensorLibrary.ID;
            }

            //сбрасываем признак активной версии у версии отличающейся от данной.
            if (this.ChangeInfo.IsPropertyChanged("IsActive"))
            {
                SkyNetworkVersion currentActiveVersion = this.Network.ActiveVersion;
                if (this.IsActive &&
                    currentActiveVersion != null &&
                    currentActiveVersion.ID != this.ID)
                {
                    currentActiveVersion.IsActive = false;
                    currentActiveVersion.Update();
                }
                this.Network.ResetActiveVersion();
            }

            //обновляем объект.
            base.Update();

            //сбрасываем флаг инициализации коллекции версий у нейросети.
            if (this.JustCreated)
            {
                this.Network.ResetVersions();
                this.Network.AddCreatedVersion(this);
            }
        }


        /// <summary>
        /// Удаляет объект и все связанные с ним дочерние объекты.
        /// </summary>
        public override void Delete()
        {
            //удаляем тензор нейросети.
            using (OperationContext deletingContext = this.Context.RunDeletingContext())
            {
                if (this.TensorLibrary.Exists)
                    this.TensorLibrary.Delete();
            }

            //удаляем состояния нейросети.
            this.DeleteChildren(this.NetworkStates);
            this.__init_ActiveNetworkState = false;

            //удаляем объект из базы данных.
            base.Delete();

            //сбрасываем флаг инициализации коллекции версий у нейросети.
            this.Network.ResetVersions();
        }


        /// <summary>
        /// Текстовое представление экземпляра класса.
        /// </summary>
        public override string ToString()
        {
            if (!string.IsNullOrEmpty(this.Name))
                return this.Name;
            return base.ToString();
        }


        ISkyNetwork ISkyNetworkVersion.Network => this.Network;

        ISkyFile ISkyNetworkVersion.TensorLibrary => this.TensorLibrary;

        IEnumerable<ISkyNetworkState> ISkyNetworkVersion.NetworkStates => this.NetworkStates;

        ISkyNetworkState ISkyNetworkVersion.CreateNetworkState(int trainSchemeID, int initialStateID)
        {
            return this.CreateNetworkState(trainSchemeID, initialStateID);
        }

        ISkyNetworkState ISkyNetworkVersion.ActiveNetworkState => this.ActiveNetworkState;

        ISkyNetworkRequest ISkyNetworkVersion.CreateRequest(ISkyUser user)
        {
            return this.CreateRequest(user);
        }

        ISkyTrainRequest ISkyNetworkVersion.CreateTrainRequest(int trainSchemeID, int initialStateID, ISkyUser user)
        {
            return this.CreateTrainRequest(trainSchemeID, initialStateID, user);
        }
    }
}
