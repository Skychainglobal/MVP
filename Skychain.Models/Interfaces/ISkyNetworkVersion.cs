using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skychain.Models
{
    /// <summary>
    /// Представляет версию нейросети.
    /// </summary>
    public interface ISkyNetworkVersion : ISkyObject
    {
        /// <summary>
        /// Нейросеть.
        /// </summary>
        ISkyNetwork Network { get; }

        /// <summary>
        /// Идентификатор версии в рамках нейросети.
        /// </summary>
        int InternalID { get; }

        /// <summary>
        /// Проверяет наличие идентификатора версии в рамках нейросети.
        /// Генерирует исключение в случае отсутствия.
        /// </summary>
        void CheckInternalID();

        /// <summary>
        /// Название нейросети.
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Описание нейросети.
        /// </summary>
        string Description { get; set; }

        /// <summary>
        /// Возвращает true, если версия нейросети является активной.
        /// При сохранении установленного значения true в базу данных (при вызове Update), 
        /// данный флаг устанавливается в false у текущей активной версии нейросети, отличающейся от данной.
        /// </summary>
        bool IsActive { get; set; }

        /// <summary>
        /// Проверяет, является ли версия нейросети активной, в ином случае генерирует исключение.
        /// </summary>
        void CheckIsActive();

        /// <summary>
        /// Манифест введённых пользователем данных и данных результата.
        /// </summary>
        string ManifestData { get; set; }

        /// <summary>
        /// Библиотека тензора нейросети.
        /// Свойство всегда возвращает существующий экземпляр класса.
        /// </summary>
        ISkyFile TensorLibrary { get; }

        /// <summary>
        /// Создаёт новый экземпляр состояния нейросети для заданной схемы тренировок.
        /// Состояние нейросети может быть создано на основе другого состояния.
        /// </summary>
        /// <param name="trainSchemeID">Идентификатор схемы тренировок, для которого создаётся состояние нейросети.</param>
        /// <param name="initialStateID">Идентификатор состояния нейросети, на основе которого создаётся состояние. 
        /// Если состояние является первичным, необходимо передать значение 0.</param>
        ISkyNetworkState CreateNetworkState(int trainSchemeID, int initialStateID);

        /// <summary>
        /// Состояния нейросети, соответствующие данной версии.
        /// </summary>
        IEnumerable<ISkyNetworkState> NetworkStates { get; }

        /// <summary>
        /// Активное состояние нейросети, используемое для выполнения запросов к данной версии нейросети.
        /// Возвращает null, если для активное состояние не задано для данной версии нейросети.
        /// </summary>
        ISkyNetworkState ActiveNetworkState { get; }

        /// <summary>
        /// Проверяет наличие активного состояния версии нейросети.
        /// Генерирует исключение в случае отсутствия активного состояния версии нейросети.
        /// </summary>
        void CheckActiveNetworkState();

        /// <summary>
        /// Устанавливает активное состояние нейросети, используемое для выполнения запросов к данной версии нейросети.
        /// </summary>
        /// <param name="networkStateID">Идентификатор состояния нейросети, устанавливаемого в качестве активного для версии нейросети.</param>
        void ChangeActiveNetworkState(int networkStateID);

        /// <summary>
        /// Создаёт новый экземляр запроса к активному состоянию данной версии нейросети.
        /// </summary>
        /// <param name="user">Пользователь, формирующий запрос.</param>
        ISkyNetworkRequest CreateRequest(ISkyUser user);

        /// <summary>
        /// Создаёт новый запрос тренировки нейросети для заданной схемы тренировок.
        /// Тренировка нейросети может быть инициализирована на основе другого состояния нейросети.
        /// </summary>
        /// <param name="trainSchemeID">Идентификатор схемы тренировок.</param>
        /// <param name="initialStateID">Идентификатор состояния нейросети, на основе которого будет выполнена тренировка. 
        /// Если тренировка является первичной, необходимо передать значение 0.</param>
        /// <param name="user">Пользователь, выполняющий тренировку нейросети.</param>
        ISkyTrainRequest CreateTrainRequest(int trainSchemeID, int initialStateID, ISkyUser user);
    }
}
