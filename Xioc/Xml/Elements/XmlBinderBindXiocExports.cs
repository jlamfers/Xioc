using System;
using System.Linq;
using System.Xml.Linq;
using Xioc.Core;

namespace Xioc.Xml.Elements
{
   [XmlConfigElement]
   public class XmlBinderBindXiocExports : XmlConfigElementBinder
   {
      public XmlBinderBindXiocExports() : base("bind-xioc-exports", "path")
      {
      }

      protected override Action<IBinder> CreateBinder(XElement e)
      {
         var path = e.GetAttributeValue<string>(OptionalAttributes[0],null);
         var assemblies = path != null
            ? AppDomain.CurrentDomain.GetAssembliesFromDirectory(path)
            : AppDomain.CurrentDomain.GetAvailableAssemblies().ToList();
         return b => b.BindXiocExports(assemblies);
      }
   }
}