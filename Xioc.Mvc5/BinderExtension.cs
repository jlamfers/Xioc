using System.Collections.Generic;
using System.Reflection;
using System.Web.Mvc;
using Xioc;

namespace Sioc.Mvc5
{
   public static class BinderExtension
   {
      public static IBinder SetupMvc(this IBinder self, IEnumerable<Assembly> controllerAssemblies)
      {
         if (controllerAssemblies != null)
         {
            self.BindAllOf<Controller>(controllerAssemblies);
         }
         XiocHttpModule.SetContainer(self.Container);
         DependencyResolver.SetResolver(new XiocMvcDependencyResolver(DependencyResolver.Current));
         return self;
      }

   }
}