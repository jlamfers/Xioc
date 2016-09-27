using System;
using Xioc.Core;

namespace Xioc.Wcf.Host
{
   public class DefaultServiceHostIocInitializer : IServiceHostIocInitializer
   {
      public void BindTypes(IBinder binder)
      {
         binder.BindXiocExports(AppDomain.CurrentDomain.GetAvailableAssemblies());
      }
   }
}