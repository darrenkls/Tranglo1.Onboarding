using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;

namespace Tranglo1.Onboarding.Application.Common
{
    public class TransformHelper
    {
        internal static string TransformXML(string xsl, string xml, XsltArgumentList parameters = null)
        {
            XPathDocument xDoc = new XPathDocument(new StringReader(xml));
            XslCompiledTransform transform = new XslCompiledTransform(true);
            transform.Load(new XmlTextReader(xsl, XmlNodeType.Document, null));

            var sr = StringWriterWithEncoding.GetUTF8();
            transform.Transform(xDoc.CreateNavigator(), parameters, sr);
            return sr.ToString();
        }
    }

    public class StringWriterWithEncoding : System.IO.StringWriter
    {
        System.Text.Encoding encoding;
        public static StringWriterWithEncoding GetUTF8()
        {
            return new StringWriterWithEncoding(System.Text.Encoding.UTF8);
        }

        public StringWriterWithEncoding(System.Text.StringBuilder builder, System.Text.Encoding encoding)
            : base(builder)
        {
            this.encoding = encoding;
        }

        public StringWriterWithEncoding(System.Text.Encoding encoding)
            : base()
        {
            this.encoding = encoding;
        }

        public StringWriterWithEncoding(IFormatProvider formatProvider, System.Text.Encoding encoding)
            : base(formatProvider)
        {
            this.encoding = encoding;
        }

        public StringWriterWithEncoding(System.Text.StringBuilder builder, IFormatProvider formatProvider, System.Text.Encoding encoding)
            : base(builder, formatProvider)
        {
            this.encoding = encoding;
        }

        public override System.Text.Encoding Encoding
        {
            get { return encoding; }
        }
    }

    public static partial class Extensions
    {
        /// <summary>
        /// Save the document contents to XML with UTF-8 encoding
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static string ToUTF8(this System.Xml.Linq.XDocument self)
        {
            using (var writer = StringWriterWithEncoding.GetUTF8())
            {
                self.Save(writer);
                writer.Flush();
                return writer.ToString();
            }

        }
    }
}
