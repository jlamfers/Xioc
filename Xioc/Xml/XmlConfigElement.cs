using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace Xioc.Xml
{
   public abstract class XmlConfigElement : IXmlConfigElement
   {
      protected XmlConfigElement(string elementName, string attributes)
         : this(elementName,null,null)
      {
         if (String.IsNullOrWhiteSpace(attributes))
         {
            return;
         }
         var parts = attributes.Split(';');
         RequiredAttributes = parts[0].Split(',').Select(x => x.Trim()).Where(x => x.Length > 0).ToArray();
         OptionalAttributes = parts.Length > 1 ? parts[1].Split(',').Select(x => x.Trim()).Where(x => x.Length > 0).ToArray() : null;
      }

      protected XmlConfigElement(string elementName, IList<string> requiredAttributes, IList<string> optionalAttributes)
      {
         if (elementName == null) throw new ArgumentNullException("elementName");
         ElementName = elementName;
         RequiredAttributes = requiredAttributes;
         OptionalAttributes = optionalAttributes; 
      }

      public abstract bool IsPredicate { get; }
      public string ElementName { get; protected set; }
      public IList<string> RequiredAttributes { get; protected set; }
      public IList<string> OptionalAttributes { get; protected set; }
      Action<IBinder> IXmlConfigElement.CreateBinder(XElement e)
      {
         Validate(e);
         return CreateBinder(e);
      }
      Func<bool> IXmlConfigElement.CreatePredicate(XElement e)
      {
         Validate(e);
         return CreatePredicate(e);

      }
      public virtual void Validate(XElement configElement)
      {
         if (configElement == null) throw new ArgumentNullException("configElement");
         if (RequiredAttributes == null && OptionalAttributes == null)
         {
            return;
         }
         var attNames = configElement.Attributes().Select(a => a.Name.LocalName).ToArray();
         foreach (var n in attNames)
         {
            if (!(RequiredAttributes == null || RequiredAttributes.Contains(n)) &&
                !(OptionalAttributes == null || OptionalAttributes.Contains(n)))
            {
               throw new XmlException(String.Format("Unknown attribute \"{0}\" at element <{1}>", n, configElement.Name));
            }
         }
         if (RequiredAttributes != null)
         {
            foreach (var n in RequiredAttributes)
            {
               if (!attNames.Contains(n))
               {
                  throw new XmlException(String.Format("Missing required attribute \"{0}\" at element <{1}>", n, configElement.Name));
               }
            }
         }
      }

      protected abstract Action<IBinder> CreateBinder(XElement e);
      protected abstract Func<bool> CreatePredicate(XElement e);
   }
}