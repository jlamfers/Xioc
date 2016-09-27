using System;
using System.Xml.Linq;

namespace Xioc.Xml.Elements
{
   [XmlConfigElement]
   public class XmlPredicateDebug : XmlConfigElementPredicate
   {
      public XmlPredicateDebug()
         : base("debug", ";")
      {
      }

      protected override Func<bool> CreatePredicate(XElement e)
      {
         var value = BuildHelper.IsDebug();
         return () => value;
      }

   }
}