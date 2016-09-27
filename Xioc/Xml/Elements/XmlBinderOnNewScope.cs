using System;
using System.Xml.Linq;

namespace Xioc.Xml.Elements
{
   [XmlConfigElement]
   public class XmlBinderOnNewScope : XmlConfigElementBinder
   {
      public XmlBinderOnNewScope() : base("on-new-scope", ";level")
      {
      }

      protected override Action<IBinder> CreateBinder(XElement e)
      {
         var binders = e.Elements().GetBinders();
         var level = e.GetAttributeValue(OptionalAttributes[0], 0);
         return b => b.OnNewScope(binders.Bind, level);
      }
   }
}