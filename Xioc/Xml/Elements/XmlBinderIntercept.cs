using System;
using System.Xml.Linq;

namespace Xioc.Xml.Elements
{
   [XmlConfigElement]
   public class XmlBinderIntercept : XmlConfigElementBinder
   {
      public XmlBinderIntercept()
         : base("intercept", "service-type,interceptor-type")
      {
      }

      protected override Action<IBinder> CreateBinder(XElement e)
      {
         var serviceType = e.GetAttributeValue<Type>(RequiredAttributes[0]);
         var interceptorType = e.GetAttributeValue<Type>(RequiredAttributes[1]);
         return b => b.Intercept(serviceType, interceptorType);
      }
   }
}