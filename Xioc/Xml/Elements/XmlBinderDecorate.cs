using System;
using System.Xml.Linq;

namespace Xioc.Xml.Elements
{
   [XmlConfigElement]
   public class XmlBinderDecorate : XmlConfigElementBinder
   {
      public XmlBinderDecorate()
         : base("decorate", "service-type,decorator-type;lifestyle")
      {
      }

      protected override Action<IBinder> CreateBinder(XElement e)
      {
         var serviceType = e.GetAttributeValue<Type>(RequiredAttributes[0]);
         var decoratorType = e.GetAttributeValue<Type>(RequiredAttributes[1]);
         var lifestyle = e.GetAttributeValue(OptionalAttributes[0], Lifestyle.Transient);
         var dependencies = e.GetDependencies(decoratorType);
         return b => b.Decorate(serviceType, decoratorType, lifestyle, dependencies);
      }
   }
}