using System;
using System.Collections.Generic;
using System.Reflection;
using System.Web.Http;
using Xioc.Core;

namespace Xioc.WebApi2
{
   public static class BinderExtension
    {
        public static IBinder SetupWebApi(this IBinder self, IEnumerable<Assembly> apiControllerAssemblies, HttpConfiguration config)
        {
            apiControllerAssemblies = apiControllerAssemblies ?? AppDomain.CurrentDomain.GetAvailableAssemblies();
            config = config ?? GlobalConfiguration.Configuration;
            self.BindAllOf<ApiController>(apiControllerAssemblies);
            config.DependencyResolver = new XiocApiDependencyResolver(config.DependencyResolver, self.Container);
            return self;
        }

    }
}