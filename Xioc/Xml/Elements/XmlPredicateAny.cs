using System;
using System.Linq;
using System.Xml.Linq;

namespace Xioc.Xml.Elements
{
   [XmlConfigElement]
   public class XmlPredicateAny : XmlConfigElementPredicate
   {
      public XmlPredicateAny()
         : base("any", ";")
      {
      }

      protected override Func<bool> CreatePredicate(XElement e)
      {
         var predicates = e.Elements().GetPredicates();
         return () => predicates.Any(f => f());
      }

   }
}