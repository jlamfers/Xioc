using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace Xioc.Xml
{
   public abstract class XmlConfigElementPredicate : XmlConfigElement
   {
      protected XmlConfigElementPredicate(string elementName, string attributes) : base(elementName, attributes)
      {
      }

      protected XmlConfigElementPredicate(string elementName, IList<string> requiredAttributes, IList<string> optionalAttributes)
         : base(elementName, requiredAttributes, optionalAttributes)
      {
      }

      public sealed override bool IsPredicate { get { return true; } }

      protected sealed override Action<IBinder> CreateBinder(XElement e)
      {
         throw new NotImplementedException("this element is not a binder, it is a " + GetType().Name);
      }
   }
}