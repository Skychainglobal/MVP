using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Storage.Lib;
using Storage.Engine;

namespace Storage.Metadata.MSSQL
{
    /// <summary>
    /// Адаптер работы с токенами.
    /// </summary>
    public class TokenAdapter : SingleTableObjectAdapter<TokenMetadata>
    {
        /// <summary>
        /// К-тор.
        /// </summary>
        /// <param name="metadataAdapter">Адаптер работы с метаданными хранилища.</param>
        internal TokenAdapter(MetadataAdapter metadataAdapter)
            : base(metadataAdapter) { }

        private bool __init_Logger;
        private ILogProvider _Logger;
        /// <summary>
        /// Логгер.
        /// </summary>
        private ILogProvider Logger
        {
            get
            {
                if (!__init_Logger)
                {
                    _Logger = ConfigFactory.Instance.Create<ILogProvider>(MetadataConsts.Scopes.TokenAdapter);
                    __init_Logger = true;
                }
                return _Logger;
            }
        }

        /// <summary>
        /// Возвращает токен из хранилища метаданных.
        /// </summary>
        /// <param name="tokenUniqueID">Уникальный идентификатор токена.</param>
        /// <returns></returns>
        public TokenMetadata GetToken(Guid tokenUniqueID)
        {
            if (tokenUniqueID == Guid.Empty)
                throw new ArgumentNullException("tokenUniqueID");

            TokenMetadata token = null;
            string condition = string.Format("UniqueID = '{0}'", tokenUniqueID);
            string resultQuery = @"{SelectQuery} WHERE {Condition}"
                .ReplaceKey("SelectQuery", this.SelectQuery)
                .ReplaceKey("Condition", condition);

            DataRow row = this.DataAdapter.GetDataRow(resultQuery);
            if (row != null)
                token = new TokenMetadata(row);

            return token;
        }

        /// <summary>
        /// Удаляет токен из хранилища метаданных.
        /// </summary>
        /// <param name="tokenUniqueID"></param>
        public void RemoveToken(Guid tokenUniqueID)
        {
            if (tokenUniqueID == Guid.Empty)
                throw new ArgumentNullException("tokenUniqueID");


            string resultQuery = @"DELETE 
FROM {TableName} 
WHERE [UniqueID] = '{TokenID}'"
                .ReplaceKey("TableName", this.DBSchemaAdapter.TableName)
                .ReplaceKey("TokenID", tokenUniqueID);

            this.Logger.WriteFormatMessage("GenerateToken: Начало удаления токена. Запрос:{0}", resultQuery);
            this.DataAdapter.ExecuteQuery(resultQuery);
            this.Logger.WriteFormatMessage("GenerateToken: Окончание удаления токена. Запрос:{0}", resultQuery);

        }

        /// <summary>
        /// Генерирует новый токен в хранилище метаданных.
        /// </summary>
        /// <returns></returns>
        public TokenMetadata GenerateToken()
        {
            this.Logger.WriteMessage("GenerateToken: Начало запроса в БД.");

            string resultQuery = @"DELETE 
FROM {TableName} 
WHERE [Expired] < GETDATE()"
                .ReplaceKey("TableName", this.DBSchemaAdapter.TableName);

            this.Logger.WriteFormatMessage("GenerateToken: Начало удаления протухших токенов. Запрос:{0}", resultQuery);
            this.DataAdapter.ExecuteQuery(resultQuery);
            this.Logger.WriteFormatMessage("GenerateToken: Окончание удаления протухших токенов. Запрос:{0}", resultQuery);


            TokenMetadata token = new TokenMetadata();
            token.Expired = DateTime.Now.AddDays(1);
            token.UniqueID = Guid.NewGuid();

            this.Logger.WriteMessage("GenerateToken: Сохранение токена в БД.");
            token.ID = this.InsertObject(token);
            this.Logger.WriteMessage("GenerateToken: Сохранение токена в БД завершено.");

            return token;
        }
    }
}