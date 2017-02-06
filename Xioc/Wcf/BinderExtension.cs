#region  License
/*
Copyright 2017 - Jaap Lamfers - jlamfers@xipton.net

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
 * */
#endregion
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
