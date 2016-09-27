using System;
using System.Xml.Linq;

namespace Xioc.Xml.Elements
{
   [XmlConfigElement]
   public class XmlBinderReturn : XmlConfigElementBinder
   {
      public XmlBinderReturn() : base("return", ";")
      {
      }

      protected override Action<IBinder> CreateBinder(XElement e)
      {
         return b => { throw new BinderExtension.XmlReturnException(); };
      }
   }
}