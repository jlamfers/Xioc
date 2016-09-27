using System;
using System.Xml.Linq;

namespace Xioc.Xml.Elements
{
   [XmlConfigElement]
   public class XmlPredicateTrue : XmlConfigElementPredicate
   {
      public XmlPredicateTrue()
         : base("true", ";")
      {
      }

      protected override Func<bool> CreatePredicate(XElement e)
      {
         return () => true;
      }
   }
}