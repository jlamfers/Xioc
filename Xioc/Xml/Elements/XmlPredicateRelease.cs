using System;
using System.Xml.Linq;

namespace Xioc.Xml.Elements
{
   [XmlConfigElement]
   public class XmlPredicateRelease : XmlConfigElementPredicate
   {
      public XmlPredicateRelease()
         : base("release", ";")
      {
      }

      protected override Func<bool> CreatePredicate(XElement e)
      {
         var value = !BuildHelper.IsDebug();
         return () => value;
      }

   }

}
