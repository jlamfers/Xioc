using System;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Xioc.Core;

namespace Xioc.Xml.Elements
{
   [XmlConfigElement]
   public class XmlBinderBindIf : XmlConfigElementBinder
   {
      public XmlBinderBindIf() : base("if", ";")
      {
      }

      protected override Action<IBinder> CreateBinder(XElement e)
      {
         var conditionElement = e.Elements("condition").SingleOrDefault();
         if (conditionElement == null)
         {
            throw new XmlException("No <condition> element found.");
         }
         var condition = new XmlPredicateAll().CastTo<IXmlConfigElement>().CreatePredicate(conditionElement);
         var thenElement = e.Elements("then").SingleOrDefault();
         if (thenElement == null)
         {
            throw new XmlException("No <then> element found.");
         }
         var then = thenElement.Elements().GetBinders();
         var elseElement = e.Elements("else").SingleOrDefault();
         var @else = elseElement != null ? elseElement.Elements().GetBinders() : null;
         return b =>
         {
            if (condition())
            {
               then.Bind(b);
            }
            else
            {
               if (@else != null)
               {
                  @else.Bind(b);
               }
            }
         };
      }
   }
}