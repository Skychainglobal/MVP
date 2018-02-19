using Skychain.Models.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skychain.Models.Implementation
{
    /// <summary>
    /// Представляет параметры прохождения тренировочного сета для набора эпох.
    /// </summary>
    public class SkyTrainEpochParams : SkyObject<SkyTrainEpochParams, SkyTrainEpochParamsEntity, ISkyTrainEpochParams>, ISkyTrainEpochParams
    {
        internal SkyTrainEpochParams(SkyTrainEpochParamsEntity entity, SkyObjectAdapter<SkyTrainEpochParams, SkyTrainEpochParamsEntity, ISkyTrainEpochParams> adapter)
            : base(entity, adapter)
        {
        }


        /// <summary>
        /// Идентификатор схемы тренировок.
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
        /// Схема тренировок, к которой относятся параметры прохождения.
        /// </summary>
        public SkyTrainScheme TrainScheme
        {
            get
            {
                if (!__init_TrainScheme)
                {
                    _TrainScheme = this.Context.ObjectAdapters.TrainSchemes.GetObject(this.TrainSchemeID, true);
                    __init_TrainScheme = true;
                }
                return _TrainScheme;
            }
        }


        /// <summary>
        /// Начальная эпоха, которой соответствуют параметры прохождения тренировочного сета.
        /// </summary>
        public int StartEpochNumber
        {
            get { return this.Entity.StartEpochNumber; }
            set
            {
                if (value <= 0)
                    throw new ArgumentException("Value cannot be less or equals than zero.");
                this.ChangeInfo.SetPropertyChange<int>("StartEpochNumber", this.Entity.StartEpochNumber, value);
                this.Entity.StartEpochNumber = value;
            }
        }


        /// <summary>
        /// Возвращает true, если значение свойства StartEpochNumber было изменено.
        /// </summary>
        private bool StartEpochNumberChanged => this.ChangeInfo.IsPropertyChanged("StartEpochNumber");


        /// <summary>
        /// Конечная эпоха, которой соответствуют параметры прохождения тренировочного сета.
        /// </summary>
        public int EndEpochNumber
        {
            get { return this.Entity.EndEpochNumber; }
            set
            {
                if (value <= 0)
                    throw new ArgumentException("Value cannot be less or equals than zero.");
                this.ChangeInfo.SetPropertyChange<int>("EndEpochNumber", this.Entity.EndEpochNumber, value);
                this.Entity.EndEpochNumber = value;
            }
        }


        /// <summary>
        /// Возвращает true, если значение свойства EndEpochNumber было изменено.
        /// </summary>
        private bool EndEpochNumberChanged => this.ChangeInfo.IsPropertyChanged("EndEpochNumber");


        private bool __init_DataSetInternal = false;
        private SkyDataSet _DataSetInternal;
        /// <summary>
        /// Набор данных для тренировки.
        /// Генерирует исключение в случае отсутствия набора данных.
        /// </summary>
        private SkyDataSet DataSetInternal
        {
            get
            {
                if (!__init_DataSetInternal)
                {
                    _DataSetInternal = null;
                    if (this.Entity.DataSetID > 0)
                        _DataSetInternal = this.Context.ObjectAdapters.DataSets.GetObject(this.Entity.DataSetID, false);
                    __init_DataSetInternal = true;
                }
                return _DataSetInternal;
            }
            set
            {
                SkyDataSet dataSet = value;
                if (dataSet == null)
                    throw new ArgumentNullException("dataSet");

                //проверяем и устанавливаем идентификатор датасета.
                dataSet.CheckExists();
                this._DataSetInternal = dataSet;
                this.__init_DataSetInternal = true;

                //устанавливаем идентификатор.
                this.ChangeInfo.SetPropertyChange<int>("DataSetID", this.Entity.DataSetID, dataSet.ID);
                this.Entity.DataSetID = dataSet.ID;
            }
        }

        /// <summary>
        /// Набор данных для тренировки.
        /// Генерирует исключение в случае отсутствия набора данных.
        /// </summary>
        public SkyDataSet DataSet
        {
            get
            {
                if (this.DataSetInternal == null)
                    throw new Exception(string.Format("There is no dataset with ID={0}.", this.Entity.DataSetID));

                return this.DataSetInternal;
            }
        }

        /// <summary>
        /// Устанавливает набор данных для тренировки сети.
        /// </summary>
        /// <param name="dataSetID">Идентификатор набора данных.</param>
        public void ChangeDataSet(int dataSetID)
        {
            if (dataSetID == 0)
                throw new ArgumentNullException("dataSetID");

            //проверяем наличие дата сета.
            this.DataSetInternal = this.Context.ObjectAdapters.DataSets.GetObject(dataSetID, true);
        }

        /// <summary>
        /// Возвращает true, если в параметрах тренировки указан набор данных.
        /// </summary>
        public bool HasDataSet => this.DataSetInternal != null;

        /// <summary>
        /// Произвольные параметры тренировки.
        /// </summary>
        public string CustomParameters
        {
            get { return this.Entity.CustomParameters; }
            set
            {
                this.ChangeInfo.SetPropertyChange<string>("CustomParameters", this.Entity.CustomParameters, value);
                this.Entity.CustomParameters = value;
            }
        }


        /// <summary>
        /// Обновляет объект в базе данных.
        /// </summary>
        public override void Update()
        {
            //проверяем наличие схемы тренировок.
            this.TrainScheme.CheckExists();

            //проверяем, что обновление эпох выполняется в контексте выполнения обновления всех эпох.
            if (this.StartEpochNumberChanged || this.EndEpochNumberChanged)
                this.TrainScheme.CheckUpdateEpochsContext();

            //получаем признак изменения объекта.
            bool hasChanges =
                this.IsNew ||
                this.StartEpochNumberChanged ||
                this.EndEpochNumberChanged ||
                this.ChangeInfo.IsPropertyChanged("DataSetID") ||
                this.ChangeInfo.IsPropertyChanged("CustomParameters");

            //выходим при отсутствии изменений.
            if (!hasChanges)
                return;

            //обновляем объект.
            base.Update();

            //сбрасываем флаг инициализации коллекции параметров прохождения у схемы тренировок.
            if (this.JustCreated || this.StartEpochNumberChanged || this.EndEpochNumberChanged)
                this.TrainScheme.ResetEpochParamsSet();
        }


        /// <summary>
        /// Удаляет объект и все связанные с ним дочерние объекты.
        /// </summary>
        public override void Delete()
        {
            //удаляем объект из базы данных.
            base.Delete();

            //сбрасываем флаг инициализации коллекции параметров прохождения у схемы тренировок.
            this.TrainScheme.ResetEpochParamsSet();
        }


        ISkyTrainScheme ISkyTrainEpochParams.TrainScheme => this.TrainScheme;

        ISkyDataSet ISkyTrainEpochParams.DataSet => this.DataSet;
    }
}
