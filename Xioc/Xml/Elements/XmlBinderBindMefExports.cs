using System;
using System.Linq;
using System.Xml.Linq;
using Xioc.Core;

namespace Xioc.Xml.Elements
{
   [XmlConfigElement]
   public class XmlBinderBindMefExports : XmlConfigElementBinder
   {
      public XmlBinderBindMefExports() : base("bind-mef-exports", ";default-lifestyle,path")
      {
      }

      protected override Action<IBinder> CreateBinder(XElement e)
      {
         var defaultLifestyle = e.GetAttributeValue(OptionalAttributes[0], Lifestyle.Transient);
         var path = e.GetAttributeValue(OptionalAttributes[1],(string)null);
         var assemblies = path != null 
            ? AppDomain.CurrentDomain.GetAssembliesFromDirectory(path) 
            : AppDomain.CurrentDomain.GetAvailableAssemblies().ToList();
         return b => b.BindMefExports(assemblies, defaultLifestyle);
      }
   }
}