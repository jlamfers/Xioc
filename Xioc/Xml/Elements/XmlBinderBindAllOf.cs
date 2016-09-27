using System;
using System.Xml.Linq;
using Xioc.Core;

namespace Xioc.Xml.Elements
{
   [XmlConfigElement]
   public class XmlBinderBindAllOf : XmlConfigElementBinder
   {
      public XmlBinderBindAllOf() : base("bind-all-of", "service-type;lifestyle,if-not-can-be-resolved")
      {
      }

      protected override Action<IBinder> CreateBinder(XElement e)
      {
         var serviceType = e.GetAttributeValue<Type>(RequiredAttributes[0]);
         var lifestyle = e.GetAttributeValue(OptionalAttributes[0], Lifestyle.Transient);
         var ifNotCanBeResolved = e.GetAttributeValue(OptionalAttributes[1], true);
         var assemblies = AppDomain.CurrentDomain.GetAvailableAssemblies();
         return b => b.BindAllOf(serviceType, assemblies, null, lifestyle, ifNotCanBeResolved);
      }
   }
}