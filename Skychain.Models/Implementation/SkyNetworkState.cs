using Skychain.Models.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skychain.Models.Implementation
{
    /// <summary>
    /// Представляет состояние нейросети, сформированное в результате тренировки.
    /// Используется для выполнения запросов к нейросети.
    /// </summary>
    public class SkyNetworkState : SkyObject<SkyNetworkState, SkyNetworkStateEntity, ISkyNetworkState>, ISkyNetworkState
    {
        internal SkyNetworkState(SkyNetworkStateEntity entity, SkyObjectAdapter<SkyNetworkState, SkyNetworkStateEntity, ISkyNetworkState> adapter)
            : base(entity, adapter)
        {
        }


        /// <summary>
        /// Идентификатор нейросети, которой соответствует состояние.
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
        /// Версия нейросети, которой соответствует состояние.
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
        /// Идентификатор версии нейросети, которой соответствует состояние.
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
        /// Версия нейросети, которой соответствует состояние.
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
        /// Идентификатор состояния в рамках версии нейросети.
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
        /// Проверяет наличие идентификатора состояния в рамках версии нейросети.
        /// Генерирует исключение в случае отсутствия.
        /// </summary>
        public void CheckInternalID()
        {
            if (this.InternalID == 0)
                throw new Exception("Network version has no InternalID.");
        }


        /// <summary>
        /// Идентификатор схемы тренировки, по которой было сформировано состояние нейросети.
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
        /// Схема тренировки, по которой было сформировано состояние нейросети.
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
        /// Идентификатор состояния нейросети, на основе которого было инициализировано данное состояние.
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
        /// Состояние нейросети, на основе которого было инициализировано данное состояние.
        /// Возвращает null в случае, если данное состояние является первичным.
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
        /// Описание состояния нейросети.
        /// </summary>
        public string Description
        {
            get { return this.Entity.Description; }
            set { this.Entity.Description = value; }
        }


        /// <summary>
        /// Отображаемое название состояния нейросети.
        /// </summary>
        public string DisplayName
        {
            get
            {
                return !string.IsNullOrEmpty(this.Description) ?
                    this.Description :
                    string.Format("{0} - {1}", this.InternalID.ToString("D2"), this.TrainScheme.Name);
            }
        }


        private bool __init_DerivedStates = false;
        private IEnumerable<SkyNetworkState> _DerivedStates;
        /// <summary>
        /// Коллекция состояний сети, инициализированных на основе данного состояния.
        /// </summary>
        public IEnumerable<SkyNetworkState> DerivedStates
        {
            get
            {
                if (!__init_DerivedStates)
                {
                    this.CheckExists();
                    _DerivedStates = this.Context.ObjectAdapters.NetworkStates.GetObjects(dbSet =>
                        dbSet.Where(x => x.InitialStateID == this.ID));
                    __init_DerivedStates = true;
                }
                return _DerivedStates;
            }
        }


        private bool __init_StateData = false;
        private SkyFile _StateData;
        /// <summary>
        /// Данные состояния нейросети, через которое будут выполняться запросы к нейросети.
        /// Свойство всегда возвращает существующий экземпляр класса.
        /// </summary>
        public SkyFile StateData
        {
            get
            {
                if (!__init_StateData)
                {
                    _StateData = new SkyFile(this.Entity.StateDataID, "NetworkStates", true, this.Context);
                    __init_StateData = true;
                }
                return _StateData;
            }
        }


        /// <summary>
        /// Проверяет наличие данных состояния нейросети.
        /// Генерирует исключение в случае отсутствия.
        /// </summary>
        public void CheckStateData()
        {
            if (!this.StateData.Exists)
                throw new Exception(string.Format("Missing neural network state data with StateID={0}.", this.ID));
        }


        /// <summary>
        /// Параметры состояния нейросети.
        /// Может содержать ошибку прохождения тренировочной схему и другие параметры.
        /// </summary>
        public string StateParameters
        {
            get { return this.Entity.StateParameters; }
            set { this.Entity.StateParameters = value; }
        }


        /// <summary>
        /// Создаёт новый экземляр запроса к данному состоянию нейросети.
        /// </summary>
        /// <param name="user">Пользователь, формирующий запрос.</param>
        public SkyNetworkRequest CreateRequest(ISkyUser user)
        {
            if (user == null)
                throw new ArgumentNullException("user");

            this.CheckExists();

            SkyNetworkRequest request = this.Context.ObjectAdapters.NetworkRequests.CreateObject();
            request.NetworkID = this.Network.ID;
            request.NetworkVersionID = this.NetworkVersion.ID;
            request.NetworkStateID = this.ID;
            request.UserID = user.ID;
            request.Status = SkyNetworkRequestStatus.Creating;
            return request;
        }


        /// <summary>
        /// Обновляет объект в базе данных.
        /// </summary>
        public override void Update()
        {
            //проверяем наличие связанных объектов.
            this.Network.CheckExists();
            this.NetworkVersion.CheckExists();
            this.TrainScheme.CheckExists();

            //проставляем идентификатор версии в рамках нейросети.
            if (this.IsNew)
                this.InternalID = this.NetworkVersion.GetStateInternalID();

            //проставляем ссылку на состояние сети, если оно инициализировано.
            if (this.StateData.Exists)
                this.Entity.StateDataID = this.StateData.ID;

            //обновляем объект.
            base.Update();

            //сбрасываем флаг инициализации коллекции версий у нейросети.
            if (this.JustCreated)
                this.NetworkVersion.ResetNetworkStates();
        }


        /// <summary>
        /// Удаляет объект и все связанные с ним дочерние объекты.
        /// </summary>
        public override void Delete()
        {
            //удаляем тензор нейросети.
            using (OperationContext deletingContext = this.Context.RunDeletingContext())
            {
                if (this.StateData.Exists)
                    this.StateData.Delete();
            }

            //выполняем удаление запросов и загруженных вместе с запросами файлов.
            this.Context.ObjectAdapters.NetworkRequests.ExecuteQuery((dbSet, context) =>
            {
                //удаляем загруженные пользователями файлы всех запросов.
                IEnumerable<string> allFileIdentities = dbSet
                    .Where(x => x.NetworkStateID == this.ID && !string.IsNullOrEmpty(x.InputFiles))
                    .Select(x => x.InputFiles);

                using (OperationContext deletingContext = this.Context.RunDeletingContext())
                {
                    foreach (string requestFileIdentities in allFileIdentities)
                    {
                        requestFileIdentities.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                            .Select(x => new SkyFile(new Guid(x), "NetworkRequestInputData", true, this.Context))
                            .ToList()
                            .ForEach(x =>
                            {
                                if (x.Exists)
                                    x.Delete();
                            });
                    }
                }

                //удаляем запросы, сфорированные по данному состоянию нейросети.
                dbSet.RemoveRange(dbSet.Where(x => x.NetworkStateID == this.ID));
                context.SaveChanges();
            });

            //удаляем объект из базы данных.
            base.Delete();

            //сбрасываем флаг инициализации коллекции версий у нейросети.
            this.NetworkVersion.ResetNetworkStates();
        }

        
        /// <summary>
        /// Текстовое представление экземпляра класса.
        /// </summary>
        public override string ToString()
        {
            return this.DisplayName;
        }

        ISkyNetwork ISkyNetworkState.Network => this.Network;

        ISkyNetworkVersion ISkyNetworkState.NetworkVersion => this.NetworkVersion;

        ISkyTrainScheme ISkyNetworkState.TrainScheme => this.TrainScheme;

        ISkyNetworkState ISkyNetworkState.InitialState => this.InitialState;

        IEnumerable<ISkyNetworkState> ISkyNetworkState.DerivedStates => this.DerivedStates;

        ISkyFile ISkyNetworkState.StateData => this.StateData;

        ISkyNetworkRequest ISkyNetworkState.CreateRequest(ISkyUser user)
        {
            return this.CreateRequest(user);
        }

    }

}
