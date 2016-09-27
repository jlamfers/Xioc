using System;
using System.Linq;
using System.Xml.Linq;

namespace Xioc.Xml.Elements
{
   [XmlConfigElement]
   public class XmlPredicateNot : XmlConfigElementPredicate
   {
      public XmlPredicateNot()
         : base("not", ";")
      {
      }

      protected override Func<bool> CreatePredicate(XElement e)
      {
         var predicates = e.Elements().GetPredicates();
         return () => !predicates.All(f => f());
      }

   }
}