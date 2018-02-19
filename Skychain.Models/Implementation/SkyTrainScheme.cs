using Skychain.Models.Entity;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skychain.Models.Implementation
{
    /// <summary>
    /// Представляет схему тренировки нейросети.
    /// </summary>
    public class SkyTrainScheme : SkyObject<SkyTrainScheme, SkyTrainSchemeEntity, ISkyTrainScheme>, ISkyTrainScheme
    {
        internal SkyTrainScheme(SkyTrainSchemeEntity entity, SkyObjectAdapter<SkyTrainScheme, SkyTrainSchemeEntity, ISkyTrainScheme> adapter)
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
        /// Нейросеть, для которой соответствует данная схема тренировки.
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
        /// Название схемы тренировки нейросети.
        /// </summary>
        public string Name
        {
            get { return this.Entity.Name; }
            set { this.Entity.Name = value; }
        }


        private bool __init_Epochs = false;
        private IEnumerable<SkyTrainEpoch> _Epochs;
        /// <summary>
        /// Эпохи прохождения тренировочного сета.
        /// </summary>
        public IEnumerable<SkyTrainEpoch> Epochs
        {
            get
            {
                if (!__init_Epochs)
                {
                    //проверяем корректность номеров эпох.
                    this.CheckEpochNumbers();

                    //формируем коллекцию эпох.
                    List<SkyTrainEpoch> epochs = new List<SkyTrainEpoch>();
                    foreach (SkyTrainEpochParams epochParams in this.EpochParamsSet)
                    {
                        for (int epochNumber = epochParams.StartEpochNumber; epochNumber <= epochParams.EndEpochNumber; epochNumber++)
                        {
                            SkyTrainEpoch epoch = new SkyTrainEpoch(epochNumber, epochParams, this);
                            epochs.Add(epoch);
                        }
                    }
                    _Epochs = new ReadOnlyCollection<SkyTrainEpoch>(epochs);
                    __init_Epochs = true;
                }
                return _Epochs;
            }
        }


        private bool __init_EpochParamsSet = false;
        private IEnumerable<SkyTrainEpochParams> _EpochParamsSet;
        /// <summary>
        /// Коллекция параметров прохождения тренировочного сета для наборов эпох.
        /// </summary>
        public IEnumerable<SkyTrainEpochParams> EpochParamsSet
        {
            get
            {
                if (!__init_EpochParamsSet)
                {
                    this.CheckExists();
                    _EpochParamsSet = this.Context.ObjectAdapters.TrainEpochParams.GetObjects(dbSet => dbSet
                        .Where(x => x.TrainSchemeID == this.ID)
                        .OrderBy(x => x.ID));

                    __init_EpochParamsSet = true;
                }
                return _EpochParamsSet;
            }
        }


        private bool __init_CreatingEpochParams = false;
        private List<SkyTrainEpochParams> _CreatingEpochParams;
        private List<SkyTrainEpochParams> CreatingEpochParams
        {
            get
            {
                if (!__init_CreatingEpochParams)
                {
                    _CreatingEpochParams = new List<SkyTrainEpochParams>();
                    __init_CreatingEpochParams = true;
                }
                return _CreatingEpochParams;
            }
        }


        internal void ResetEpochParamsSet()
        {
            this.__init_Epochs = false;
            this.__init_EpochParamsSet = false;
        }

        /// <summary>
        /// Создаёт экземпляр параметров прохождения тренировочного сета для набора эпох, без сохранения в базу данных.
        /// </summary>
        public SkyTrainEpochParams CreateEpochParams()
        {
            this.CheckExists();
            SkyTrainEpochParams epochParams = this.Context.ObjectAdapters.TrainEpochParams.CreateObject();
            epochParams.TrainSchemeID = this.ID;
            this.CreatingEpochParams.Add(epochParams);
            return epochParams;
        }


        /// <summary>
        /// Запускает контекст операции обновления номеров эпох.
        /// </summary>
        private OperationContext RunUpdateEpochsContext()
        {
            return this.ContextManager.BeginContext("UpdateEpochsContext");
        }

        /// <summary>
        /// Проверяет, что код выполняется в контексте операции обновления номеров эпох.
        /// </summary>
        internal void CheckUpdateEpochsContext()
        {
            if (!this.ContextManager.IsContext("UpdateEpochsContext"))
                throw new Exception("Changing epoch numbers should be executed at the UpdateEpochs() method.");
        }

        /// <summary>
        /// Проверяет корректность и обновляет номера эпох, а также обновляет остальные свойства для всех параметров эпох.
        /// </summary>
        public void UpdateEpochs()
        {
            //выходим при отсутствии параметров эпох.
            if (!(this.EpochParamsSet.Any() || this.CreatingEpochParams.Any(x => x.IsNew)))
                return;

            using (OperationContext context = this.RunUpdateEpochsContext())
            {
                //проверяем эпохи.
                this.CheckEpochNumbers();

                //обновляем эпохи в случае успешной проверки.
                //эпохи будут обновлены только при наличии изменений хотя бы одно из свойств.
                foreach (SkyTrainEpochParams epochParams in this.EpochParamsSet)
                    epochParams.Update();

                //обновляем новые эпохи.
                foreach (SkyTrainEpochParams epochParams in this.CreatingEpochParams)
                {
                    if (epochParams.IsNew)
                        epochParams.Update();
                }
            }
        }

        /// <summary>
        /// Проверяет корректность номеров эпох в параметрах эпох.
        /// Генерирует исключение в случае непоследовательной нумерации эпох.
        /// </summary>
        private void CheckEpochNumbers()
        {
            //конечный номер предыдущей проверяемой эпохи.
            int previousEndNumber = 0;

            IEnumerable<SkyTrainEpochParams> epochsToCheck = null;
            if (this.CreatingEpochParams.Any(x => x.IsNew))
            {
                List<SkyTrainEpochParams> allEpochs = new List<SkyTrainEpochParams>(this.EpochParamsSet);
                allEpochs.AddRange(this.CreatingEpochParams.Where(x => x.IsNew));
                epochsToCheck = allEpochs;
            }
            else
                epochsToCheck = this.EpochParamsSet;

            //проверяем параметры эпох.
            foreach (SkyTrainEpochParams epochParams in epochsToCheck)
            {
                //проверяем, что начальная эпоха на единицу отличается от предыдущей.
                if (epochParams.StartEpochNumber != previousEndNumber + 1)
                {
                    if (previousEndNumber == 0)
                        throw new Exception("First epoch number must be 1.");
                    else
                        throw new Exception(string.Format("The number of the initial epoch {0} must be one greater than the number of the finite epoch {1} of the previous set of epochs.",
                            epochParams.StartEpochNumber, previousEndNumber));
                }

                //проверяем, что конечная эпоха больше или равна начальной.
                if (epochParams.EndEpochNumber < epochParams.StartEpochNumber)
                    throw new Exception(string.Format("The number of the finite epoch {0} must be greater than or equal to the number of the initial epoch {1}.",
                        epochParams.EndEpochNumber, epochParams.StartEpochNumber));

				previousEndNumber = epochParams.EndEpochNumber;
			}
        }

        /// <summary>
        /// Обновляет объект в базе данных.
        /// </summary>
        public override void Update()
        {
            //проверяем наличие нейросети.
            this.Network.CheckExists();

            //обновляем объект.
            base.Update();

            //сбрасываем флаг инициализации коллекции схемы тренировок у нейросети.
            if (this.JustCreated)
                this.Network.ResetTrainSchemes();
        }


        /// <summary>
        /// Удаляет объект и все связанные с ним дочерние объекты.
        /// </summary>
        public override void Delete()
        {
            //удаляем схемы параметры прохождения эпох.
            this.DeleteChildren(this.EpochParamsSet);

            //удаляем объект из базы данных.
            base.Delete();

            //сбрасываем флаг инициализации коллекции схемы тренировок у нейросети.
            this.Network.ResetTrainSchemes();
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


        ISkyNetwork ISkyTrainScheme.Network => this.Network;

        IEnumerable<ISkyTrainEpoch> ISkyTrainScheme.Epochs => this.Epochs;

        IEnumerable<ISkyTrainEpochParams> ISkyTrainScheme.EpochParamsSet => this.EpochParamsSet;

        ISkyTrainEpochParams ISkyTrainScheme.CreateEpochParams()
        {
            return this.CreateEpochParams();
        }
    }
}
