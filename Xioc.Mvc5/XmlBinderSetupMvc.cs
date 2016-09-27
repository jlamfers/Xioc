using System;
using System.Xml.Linq;
using Xioc;
using Xioc.Core;
using Xioc.Xml;

namespace Sioc.Mvc5
{
   [XmlConfigElement]
   public class XmlBinderSetupMvc : XmlConfigElementBinder
   {
      public XmlBinderSetupMvc() : base("setup-mvc", ";")
      {
      }

      protected override Action<IBinder> CreateBinder(XElement e)
      {
         var assemblies = AppDomain.CurrentDomain.GetAvailableAssemblies();
         return b => b.SetupMvc(assemblies);
      }
   }
}