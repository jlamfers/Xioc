using System.Collections.Generic;

namespace Xioc.Proxy.Core
{
    internal class InterceptorCollection : IInterceptor
    {
        private readonly IList<IInterceptor> _interceptors;

        public InterceptorCollection(IList<IInterceptor> interceptors)
        {
            _interceptors = interceptors;
        }

        public void Intercept(IInvocation invocation)
        {
            // thread safe: each invocation gets its own context
            new InvocationContext(invocation, _interceptors).Proceed();
        }

    }
}