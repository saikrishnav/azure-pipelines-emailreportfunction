using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace EmailReportFunction.Utils
{
    public class XmlTextWriterWithoutNamespace : XmlTextWriter
    {
        public XmlTextWriterWithoutNamespace(Stream w, Encoding encoding) : base(w, encoding)
        {
        }

        public XmlTextWriterWithoutNamespace(string filename, Encoding encoding) : base(filename, encoding)
        {
        }

        public XmlTextWriterWithoutNamespace(TextWriter w) : base(w)
        {
        }

        public override void WriteStartElement(string prefix, string localName, string ns)
        {
            base.WriteStartElement(string.Empty, localName, string.Empty);
        }

        public override void WriteValue(string value)
        {
            base.WriteValue(StringUtils.GetXmlValidString(value));
        }
    }
}
