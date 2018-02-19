using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml;
using SkyWebContextClass = Skychain.Models.Implementation.SkyWebContext;

namespace Skychain.Models.Runtime
{
    /// <summary>
    /// Содержит методы для работы с текущим http-контекстом.
    /// </summary>
    public static class SkyWebContext
    {
        /// <summary>
        /// Возвращает текущий контекст выполнения http-запроса в рамках веб-приложения Skychain.
        /// Генерирует исключение в случае отсутствия контекста.
        /// </summary>
        public static ISkyWebContext Current
        {
            get { return RuntimeContext.Current.Properties.GetSingleton<ISkyWebContext>("SkyWebContext.Current", CreateCurrent); }
        }

        private static ISkyWebContext CreateCurrent(RuntimeContext context)
        {
            if (context == null)
                throw new ArgumentNullException("context");
            context.CheckHttpContext();
            return new SkyWebContextClass(context.HttpContext);
        }


        #region EncodeHtml

        /// <summary>
        /// Кодирует текст для представления всех его символов в отображаемом виде в html.
        /// </summary>
        /// <param name="text">Кодируемый текст.</param>
        public static string EncodeHtml(string text)
        {
            //возвращаем исходный текст, если он пустой.
            if (string.IsNullOrEmpty(text))
                return text;

            //кодируем символы текста в отображаемом для html виде.
            string encodedText = SkyWebContext.EncodeWhiteSpaces(HttpUtility.HtmlEncode(text));

            //возвращаем закодированный текст.
            return encodedText;
        }


        /// <summary>
        /// Кодирует переносы строки, знаки табуляции, повторяющиеся пробелы для представления в отображаемом виде в html.
        /// </summary>
        /// <param name="text">Кодируемый текст.</param>
        private static string EncodeWhiteSpaces(string text)
        {
            //возвращаем исходный текст, если он пустой.
            if (string.IsNullOrEmpty(text))
                return text;

            //выполняем замену пустых символов для представляния в виде html.
            string encodedText = Regex.Replace(text, @"\r?\n([ \t]*)", delegate (Match breakMatch)
            {
                //если после переноса строки встретились пробелы или знаки табуляции, заменяем их на символ &nbsp;.
                if (breakMatch.Groups.Count > 1)
                    return string.Format("<br/>{0}", breakMatch.Groups[1].Value.Replace("\t", "&nbsp;&nbsp;&nbsp;&nbsp;").Replace(" ", "&nbsp;"));

                //если после переноса строки нет пробелов или знаков табуляций, заменяем перенос строки на <br/>.
                return "<br/>";
            });

            //заменяем повторяющиеся пробелы и знаки табуляции на &nbsp;.
            //заменить их нужно вместе, поскольку одиночный пробел рядом со знаком табуляции не отобразится.
            encodedText = Regex.Replace(encodedText, @"[ \t]{2,}", delegate (Match spaceMatch)
            {
                return spaceMatch.Value.Replace("\t", "&nbsp;&nbsp;&nbsp;&nbsp;").Replace(" ", "&nbsp;");
            });

            //заменяем все оставшиеся одиночные знаки табуляции.
            encodedText = encodedText.Replace("\t", "&nbsp;&nbsp;&nbsp;&nbsp;");

            //возвращаем закодированный текст.
            return encodedText;
        } 

        #endregion


        #region EncodeXml

        /// <summary>
        /// Кодирует текст формата Xml для представления всех его символов в отображаемом виде в html.
        /// </summary>
        /// <param name="xmlData">Кодируемый текст в формате Xml.</param>
        public static string EncodeXml(string xmlData)
        {
            //возвращаем исходный текст, если он пустой.
            if (string.IsNullOrEmpty(xmlData))
                return xmlData;

            //получаем форматированный Xml.
            string formattedXmlData = IndentXml(xmlData);

            //выполняем подстветку тэга документа.
            string coloredXml = Regex.Replace(formattedXmlData, @"(?<DocStart><\?)(?<TagName>\w[\w:-]*)(?<TagBody>[^>]*)(?<DocEnd>\?>)", ReplaceDocumentTag);

            //выполняем подсветку открывающих тэгов.
            coloredXml = Regex.Replace(coloredXml, @"(?<TagStart><)(?<TagName>\w[\w:-]*)(?<TagBody>[^>]*?)(?<TagEnd>\/?>)", ReplaceBeginingTag);

            //выполняем подсветку закрывающих тэгов.
            coloredXml = Regex.Replace(coloredXml, @"(?<TagStart><\/)(?<TagName>\w[\w:-]*)(?<TagBody>\s*)(?<TagEnd>>)", ReplaceClosingTag);

            //выполняем подсветку комментария.
            coloredXml = Regex.Replace(coloredXml, @"(?<TagStart><!--)(?<CommentBody>[\s\S]*?)(?<TagEnd>-->)", ReplaceComment);

            //выполняем кодировку всего содержимого.
            string resultHtml = EncodeHtml(coloredXml);

            //выполняем замену цветов.
            resultHtml = ReplaceColor(resultHtml, "DocStart", "xml_tag");
            resultHtml = ReplaceColor(resultHtml, "DocEnd", "xml_tag");
            resultHtml = ReplaceColor(resultHtml, "TagStart", "xml_tag");
            resultHtml = ReplaceColor(resultHtml, "TagEnd", "xml_tag");
            resultHtml = ReplaceColor(resultHtml, "TagName", "xml_tagName");
            resultHtml = ReplaceColor(resultHtml, "CommentBody", "xml_comment");

            resultHtml = ReplaceColor(resultHtml, "AttributeName", "xml_attr");
            resultHtml = ReplaceColor(resultHtml, "Equals", "xml_equals");
            resultHtml = ReplaceColor(resultHtml, "AttributeStart", "xml_quote");
            resultHtml = ReplaceColor(resultHtml, "AttributeBody", "xml_attrValue");
            resultHtml = ReplaceColor(resultHtml, "AttributeEnd", "xml_quote");

            return resultHtml;
        }


        /// <summary>
        /// Удаляет пробелы между тэгами.
        /// </summary>
        /// <param name="text">Строка в формате Xml или Html.</param>
        private static string RemoveTagWhitespaces(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;
            text = Regex.Replace(text, @">\s+<", "><");
            text = Regex.Replace(text, @">\s+$", ">");
            text = Regex.Replace(text, @"^\s+<", "<");
            return text;
        }


        /// <summary>
        /// Приводит текст в формате Xml к виду, включающему отступы вложенных тэгов.
        /// </summary>
        /// <param name="xmlText">Строка в формате Xml.</param>
        private static string IndentXml(string xmlText)
        {
            if (string.IsNullOrEmpty(xmlText))
                return xmlText;

            //удаляем лишние пробелы.
            xmlText = RemoveTagWhitespaces(xmlText);

            //форматируем xml через XmlTextWriter.
            XmlDocument xDoc = new XmlDocument();
            xDoc.LoadXml(xmlText);
            using (StringWriter textWriter = new StringWriter())
            {
                using (XmlTextWriter xmlWriter = new XmlTextWriter(textWriter))
                {
                    //задаем читабельный тип форматирования для текста Xml.
                    xmlWriter.Formatting = Formatting.Indented;

                    //записываем документ во writer.
                    xDoc.WriteTo(xmlWriter);
                    xmlWriter.Flush();

                    //формируем форматированный результат
                    xmlText = textWriter.ToString();
                }
            }

            //возвращаем результат.
            return xmlText;
        }


        private static string ReplaceDocumentTag(Match documentTag)
        {
            if (documentTag == null)
                throw new ArgumentNullException("documentTag");

            return
                SetColor(documentTag.Groups["DocStart"].Value, "DocStart") +
                SetColor(documentTag.Groups["TagName"].Value, "TagName") +
                Regex.Replace(documentTag.Groups["TagBody"].Value, @"(?<AttributeName>\w[\w:-]*)(?<Equals>\s*=\s*)((?<AttributeStart>"")(?<AttributeBody>[^""]*)(?<AttributeEnd>"")|(?<AttributeStart>')(?<AttributeBody>[^']*)(?<AttributeEnd>'))", ReplaceAttribute) +
                SetColor(documentTag.Groups["DocEnd"].Value, "DocEnd");
        }

        private static string ReplaceBeginingTag(Match beginingTag)
        {
            if (beginingTag == null)
                throw new ArgumentNullException("beginingTag");

            return
                SetColor(beginingTag.Groups["TagStart"].Value, "TagStart") +
                SetColor(beginingTag.Groups["TagName"].Value, "TagName") +
                Regex.Replace(beginingTag.Groups["TagBody"].Value, @"(?<AttributeName>\w[\w:-]*)(?<Equals>\s*=\s*)((?<AttributeStart>"")(?<AttributeBody>[^""]*)(?<AttributeEnd>"")|(?<AttributeStart>')(?<AttributeBody>[^']*)(?<AttributeEnd>'))", ReplaceAttribute) +
                SetColor(beginingTag.Groups["TagEnd"].Value, "TagEnd");
        }

        private static string ReplaceAttribute(Match tagBody)
        {
            if (tagBody == null)
                throw new ArgumentNullException("tagBody");

            return
                SetColor(tagBody.Groups["AttributeName"].Value, "AttributeName") +
                SetColor(tagBody.Groups["Equals"].Value, "Equals") +
                SetColor(tagBody.Groups["AttributeStart"].Value, "AttributeStart") +
                SetColor(tagBody.Groups["AttributeBody"].Value, "AttributeBody") +
                SetColor(tagBody.Groups["AttributeEnd"].Value, "AttributeEnd");
        }

        private static string ReplaceClosingTag(Match closingTag)
        {
            if (closingTag == null)
                throw new ArgumentNullException("closingTag");

            return
                SetColor(closingTag.Groups["TagStart"].Value, "TagStart") +
                SetColor(closingTag.Groups["TagName"].Value, "TagName") +
                closingTag.Groups["TagBody"].Value +
                SetColor(closingTag.Groups["TagEnd"].Value, "TagEnd");
        }

        private static string ReplaceComment(Match commentNode)
        {
            if (commentNode == null)
                throw new ArgumentNullException("commentNode");

            return
                SetColor(commentNode.Groups["TagStart"].Value, "TagStart") +
                SetColor(commentNode.Groups["CommentBody"].Value, "CommentBody") +
                SetColor(commentNode.Groups["TagEnd"].Value, "TagEnd");
        }

        private static string SetColor(string stringValue, string colorTagName)
        {
            if (string.IsNullOrEmpty(colorTagName))
                throw new ArgumentNullException("colorTagName");

            return string.Format("[Color:{0}]{1}[/Color:{0}]", colorTagName, stringValue);
        }

        private static string ReplaceColor(string value, string colorTagName, string cssClassName)
        {
            if (value == null)
                throw new ArgumentNullException("value");
            if (string.IsNullOrEmpty(cssClassName))
                throw new ArgumentNullException("cssClassName");


            string cssClassNameLocal = cssClassName;
            return Regex.Replace(value, string.Format(@"(?<TagStart>\[Color:{0}\])(?<Body>[\s\S]*?)(?<TagEnd>\[\/Color:{0}\])", colorTagName), (Match colorTag) =>
            {
                return ReplaceColorTags(colorTag, cssClassNameLocal);
            });
        }

        private static string ReplaceColorTags(Match colorTag, string cssClassName)
        {
            if (colorTag == null)
                throw new ArgumentNullException("colorTag");

            if (string.IsNullOrEmpty(cssClassName))
                throw new ArgumentNullException("cssClassName");

            return
                string.Format("<span class=\"{0}\">", cssClassName) +
                colorTag.Groups["Body"].Value +
                "</span>";
        }

        #endregion
    }
}