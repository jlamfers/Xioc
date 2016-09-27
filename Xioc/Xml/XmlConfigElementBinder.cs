using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace Xioc.Xml
{
   public abstract class XmlConfigElementBinder : XmlConfigElement
   {
      protected XmlConfigElementBinder(string elementName, string attributes) : base(elementName, attributes)
      {
      }

      protected XmlConfigElementBinder(string elementName, IList<string> requiredAttributes, IList<string> optionalAttributes)
         : base(elementName, requiredAttributes, optionalAttributes)
      {
      }

      public sealed override bool IsPredicate { get { return false; } }

      protected sealed override Func<bool> CreatePredicate(XElement e)
      {
         throw new NotImplementedException("This element is not a predicate");
      }
   }
}