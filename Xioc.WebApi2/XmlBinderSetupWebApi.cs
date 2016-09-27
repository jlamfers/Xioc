using System;
using System.Xml.Linq;
using Xioc.Xml;

namespace Xioc.WebApi2
{
   [XmlConfigElement]
   public class XmlBinderSetupWebApi : XmlConfigElementBinder
   {
      public XmlBinderSetupWebApi()
         : base("setup-webapi", ";")
      {
      }

      protected override Action<IBinder> CreateBinder(XElement e)
      {
         return b => b.SetupWebApi(null,null);
      }
   }
}