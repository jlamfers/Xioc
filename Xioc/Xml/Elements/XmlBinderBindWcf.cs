using System;
using System.Xml.Linq;
using Xioc.Wcf;

namespace Xioc.Xml.Elements
{
   [XmlConfigElement]
   public class XmlBinderBindWcf : XmlConfigElementBinder
   {
      public XmlBinderBindWcf() : base("bind-wcf", "service-type,implementation-type")
      {
      }

      protected override Action<IBinder> CreateBinder(XElement e)
      {
         var serviceType = e.GetAttributeValue<Type>(RequiredAttributes[0]);
         var implementationType = e.GetAttributeValue<Type>(RequiredAttributes[1]);
         return b => b.BindWcfService(serviceType, implementationType);
      }
   }
}