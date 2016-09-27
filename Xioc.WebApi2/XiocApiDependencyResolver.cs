using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Dependencies;

namespace Xioc.WebApi2
{
    // This class fully replaces (decorates) the web api DependencyResolver.
    // All resolve attempts are probed at the NiocScope first. If the NiocScope cannot resolve the request, the corresponding
    // request is forwarded to the original DependencyResolver
    public sealed class XiocApiDependencyResolver : IDependencyResolver
    {
        private IDependencyResolver _dependencyResolver;
        private IDependencyScope _dependencyScope;
        private IContainer _container;
        private IScope _containerScope;
        private readonly object _syncroot = new object();


        public XiocApiDependencyResolver(IDependencyResolver dependencyResolver, IContainer container)
        {
            if (dependencyResolver == null) throw new ArgumentNullException("dependencyResolver");
            if (container == null) throw new ArgumentNullException("container");
            _dependencyResolver = dependencyResolver;
            _container = container;
            // start a scope anyway, for the non-scoped resolve requests
            // normally such requests won't result into real instances unless you decide to bind 
            // your own custom managing types from the MVC framework in the NiocContainer
            _containerScope = container.BeginScope();
        }
        protected XiocApiDependencyResolver(IDependencyScope dependencyScope, IScope containerScope)
        {
            _dependencyScope = dependencyScope;
            _containerScope = containerScope;
        }

        public object GetService(Type serviceType)
        {
            EnsureNotDisposed();
            return _containerScope.TryResolve(serviceType) ?? (_dependencyScope ?? _dependencyResolver).GetService(serviceType);
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            EnsureNotDisposed();
            var result = _containerScope.ResolveAll(serviceType).ToArray();
            return result.Any() ? result : (_dependencyScope ?? _dependencyResolver).GetServices(serviceType);
        }

        public IDependencyScope BeginScope()
        {
            EnsureNotDisposed();
            return new XiocApiDependencyResolver(_dependencyResolver.BeginScope(), _container.BeginScope());
        }

        private bool _disposed;
        public void Dispose()
        {
            lock (_syncroot)
            {
                if (_disposed) return;
                _disposed = true;
            }

            // there always is a container scope
            _containerScope.Dispose();

            if (_dependencyScope != null)
            {
                // if there is a scope, i.e., BeginScope was called, then dispose the dependency scope
                _dependencyScope.Dispose();
            }
            else
            {
                // no scope, we are at the root level
                _dependencyResolver.Dispose();
                _container.Dispose();
            }
            _containerScope = null;
            _dependencyScope = null;
            _container = null;
            _dependencyResolver = null;
        }


        private void EnsureNotDisposed()
        {
            if (_disposed) throw new ObjectDisposedException(GetType().FullName);
        }

    }
}