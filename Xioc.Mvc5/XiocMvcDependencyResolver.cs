using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Xioc;

namespace Sioc.Mvc5
{
    public class XiocMvcDependencyResolver : IDependencyResolver
    {

        private readonly IDependencyResolver
            _decorated;


        public XiocMvcDependencyResolver(IDependencyResolver decorated)
        {
            if (decorated == null) throw new ArgumentNullException("decorated");
            _decorated = decorated;
        }

        public object GetService(Type serviceType)
        {
            return XiocHttpModule.GetRequestScope().TryResolve(serviceType) ?? _decorated.GetService(serviceType);
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            var result = XiocHttpModule.GetRequestScope().ResolveAll(serviceType).ToArray();
            return result.Any() ? result : _decorated.GetServices(serviceType);
        }
    }
}