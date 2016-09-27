using System;
using System.Xml.Linq;

namespace Xioc.Xml.Elements
{
   [XmlConfigElement]
   public class XmlBinderBindXml : XmlConfigElementBinder
   {
      public XmlBinderBindXml() : base("bind-xml", "uri")
      {
      }

      protected override Action<IBinder> CreateBinder(XElement e)
      {
         var uri = e.GetAttributeValue(RequiredAttributes[0]);
         return b => b.BindXml(uri);
      }
   }
}