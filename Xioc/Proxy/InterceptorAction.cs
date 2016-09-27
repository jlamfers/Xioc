using System;

namespace Xioc.Proxy
{
    public class InterceptorAction : IInterceptor
    {
        private readonly Action<IInvocation> _action;

        public InterceptorAction(Action<IInvocation> action)
        {
            _action = action;
        }

        public void Intercept(IInvocation invocation)
        {
            _action(invocation);
        }
    }
}