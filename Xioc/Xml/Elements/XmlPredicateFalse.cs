using System;
using System.Xml.Linq;

namespace Xioc.Xml.Elements
{
   [XmlConfigElement]
   public class XmlPredicateFalse : XmlConfigElementPredicate
   {
      public XmlPredicateFalse()
         : base("false", ";")
      {
      }

      protected override Func<bool> CreatePredicate(XElement e)
      {
         return () => false;
      }
   }
}