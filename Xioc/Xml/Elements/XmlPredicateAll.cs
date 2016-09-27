using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace Xioc.Xml.Elements
{
   [XmlConfigElement]
   public class XmlPredicateAll : XmlConfigElementPredicate
   {
      public XmlPredicateAll()
         : base("all", ";")
      {
      }

      protected override Func<bool> CreatePredicate(XElement e)
      {
         var predicates = e.Elements().GetPredicates();
         return () => predicates.All(f => f());
      }

   }
}