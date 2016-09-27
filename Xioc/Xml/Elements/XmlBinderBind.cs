using System;
using System.Xml.Linq;

namespace Xioc.Xml.Elements
{
   [XmlConfigElement]
   public class XmlBinderBind : XmlConfigElementBinder
   {
      public XmlBinderBind()
         : base("bind", "service-type;implementation-type,lifestyle")
      {
      }

      protected override Action<IBinder> CreateBinder(XElement e)
      {
         var serviceType = e.GetAttributeValue<Type>(RequiredAttributes[0]);
         var implementationType = e.GetAttributeValue<Type>(OptionalAttributes[0], serviceType);
         var lifestyle = e.GetAttributeValue(OptionalAttributes[1], Lifestyle.Transient);
         var dependencies = e.GetDependencies(implementationType);
         return b => b.Bind(serviceType, implementationType, lifestyle, dependencies);
      }
   }
}