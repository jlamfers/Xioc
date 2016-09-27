using System;

namespace Xioc.Wcf
{
   public static class BinderExtension
   {

      public static IBinder BindWcfService(this IBinder self, Type serviceType, Type implementationType)
      {
         // implementationType is not disposed by the scope, dependencies can be resolved and ARE managed by scope
         self.Bind(serviceType, implementationType, Lifestyle.TransientNoDispose);
         var b = self.GetBinding(serviceType);

         // ServiceClient<> takes care of closing (disposing) the proxy from the previous binding, 
         // by its Dispose method, as soon as the lifetime scope ends

         //NOTE: never bind service clients as transient, because this could lead into a large number of open clients, 
         //      since the corresponding client instances only would be closed on scope exit.
         var serviceClientType = typeof(ServiceClient<>).MakeGenericType(serviceType);
         self.Bind(serviceClientType, c => serviceClientType.CreateInstance<IServiceClient>().Initialize(b.GetInstance(c)), Lifestyle.PerScope);

         // Resolving serviceType results into a scope level ServiceClient target instance
         self.Bind(serviceType, c => c.Scope.Resolve(serviceClientType).CastTo<IServiceClient>().Target, Lifestyle.UnManaged);

         return self;
      }

      public static IBinder BindWcfService<TService, TImplementation>(this IBinder self)
          where TImplementation : TService
      {
         return self.BindWcfService(typeof(TService), typeof(TImplementation));
      }

   }
}
