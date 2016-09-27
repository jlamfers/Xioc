using System;
using System.Xml.Linq;

namespace Xioc.Xml.Elements
{
   [XmlConfigElement]
   public class XmlBinderIfDebug : XmlConfigElementBinder
   {
      public XmlBinderIfDebug() : base("if-debug", ";")
      {
      }

      protected override Action<IBinder> CreateBinder(XElement e)
      {
         if (!BuildHelper.IsDebug()) return b => { };
         var binders = e.Elements().GetBinders();
         return binders.Bind;
      }
   }
}