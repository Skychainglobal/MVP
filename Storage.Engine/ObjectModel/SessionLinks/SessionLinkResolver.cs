using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Storage.Lib;

namespace Storage.Engine
{
    /// <summary>
    /// Адаптер сессионных ссылок.
    /// </summary>
    internal class SessionLinkResolver : ISessionResolver
    {
        private readonly int _guidLength = 16;

        /// <summary>
        /// Возвращает сессионную ссылку по токену.
        /// </summary>
        /// <param name="token">Токен файла.</param>
        /// <returns></returns>
        public string GetSessionLink(IFileToken token)
        {
            if (token == null)
                throw new ArgumentNullException("token");

            int destinationIndex = 0;
            if (string.IsNullOrEmpty(token.FolderUrl))
                throw new Exception(string.Format("sessionToken.FolderUrl"));

            if (token.UniqueID == Guid.Empty)
                throw new ArgumentNullException("sessionToken.UniqueID");

            if (token.FileUniqueID == Guid.Empty)
                throw new ArgumentNullException("sessionToken.FileUniqueID");

            if (token.VersionUniqueID == Guid.Empty)
                throw new ArgumentNullException("sessionToken.VersionUniqueID");

            byte[] folderUrlBytes = Encoding.UTF8.GetBytes(token.FolderUrl);
            byte[] tokenBytes = new byte[(_guidLength * 3) + folderUrlBytes.Length];

            Array.Copy(token.UniqueID.ToByteArray(), 0, tokenBytes, destinationIndex, _guidLength);
            destinationIndex += _guidLength;
            Array.Copy(token.FileUniqueID.ToByteArray(), 0, tokenBytes, destinationIndex, _guidLength);
            destinationIndex += _guidLength;
            Array.Copy(token.VersionUniqueID.ToByteArray(), 0, tokenBytes, destinationIndex, _guidLength);
            destinationIndex += _guidLength;
            Array.Copy(folderUrlBytes, 0, tokenBytes, destinationIndex, folderUrlBytes.Length);

            string tokenString = Convert.ToBase64String(tokenBytes);

            string contentDeliveryHost = ConfigReader.GetStringValue(EngineConsts.CongfigParams.ContentDeliveryHost);
            contentDeliveryHost = contentDeliveryHost.TrimEnd('/');
            string link = string.Format("{0}/{1}",
                contentDeliveryHost,
                tokenString.Replace("/", "-"));

            return link;
        }

        /// <summary>
        /// Возвращает токен файла по ссылке.
        /// </summary>
        /// <param name="sessionLink">Сессионная ссылка.</param>
        /// <returns></returns>
        public IFileToken Resolve(string sessionLink)
        {
            if (string.IsNullOrEmpty(sessionLink))
                throw new ArgumentNullException("sessionLink");

            Uri uri = new Uri(sessionLink);
            //обрезаем первый слеш разделителя параметров
            string token = uri.PathAndQuery.TrimStart('/');
            //заменяем спецсимвол на слеш (при генерации ссылки делалась обратная операция)
            token = token.Replace("-", "/");

            Guid uniqueID;
            Guid fileUniqueID;
            Guid versionUniqueID;
            byte[] tmpGuidBytes = new byte[16];


            byte[] allBytes = Convert.FromBase64String(token);
            int tokenGuidsLen = _guidLength * 3;
            int folderBytesLength = allBytes.Length - tokenGuidsLen;
            if (folderBytesLength < 1)
                throw new Exception(string.Format("Некорректный токен"));

            int sourceIndex = 0;
            string folderUrl = Encoding.UTF8.GetString(allBytes, tokenGuidsLen, folderBytesLength);

            Array.Copy(allBytes, sourceIndex, tmpGuidBytes, 0, _guidLength);
            sourceIndex += _guidLength;
            uniqueID = new Guid(tmpGuidBytes);

            Array.Copy(allBytes, sourceIndex, tmpGuidBytes, 0, _guidLength);
            sourceIndex += _guidLength;
            fileUniqueID = new Guid(tmpGuidBytes);

            Array.Copy(allBytes, sourceIndex, tmpGuidBytes, 0, _guidLength);
            versionUniqueID = new Guid(tmpGuidBytes);

            SessionLinkToken sessionToken = new SessionLinkToken(uniqueID, fileUniqueID, versionUniqueID, folderUrl);
            return sessionToken;
        }
    }
}