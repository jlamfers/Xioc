using System;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;
using Xioc.Proxy.Core;

namespace Xioc.Proxy
{
    public static class ProxyBuilder
    {

        private class InnerProxy : RealProxy
        {
            private readonly object _instance;
            private readonly IInterceptor _interceptor;

            public InnerProxy(Type proxyType, object instance, IInterceptor interceptor) : base(proxyType)
            {
                _instance = instance;
                _interceptor = interceptor;
            }

            public override IMessage Invoke(IMessage msg)
            {
                var methodCall = (IMethodCallMessage) msg;
                var method = (MethodInfo) methodCall.MethodBase;
                try
                {
                    var invocation = new Invocation
                    {
                        Arguments = methodCall.Args,
                        Method = method,
                        Target = _instance
                    };

                    _interceptor.Intercept(invocation);

                    return methodCall.HasVarArgs
                        ? new ReturnMessage(invocation.ReturnValue, invocation.Arguments,invocation.Arguments.Length, methodCall.LogicalCallContext, methodCall)
                        : new ReturnMessage(invocation.ReturnValue, null, 0, methodCall.LogicalCallContext, methodCall);
                }
                catch (Exception ex)
                {
                    return new ReturnMessage(ex is TargetInvocationException && ex.InnerException != null ? ex.InnerException : ex, msg as IMethodCallMessage);
                }
            }

        }

        public static T CreateProxy<T>(T instance, IInterceptor interceptor)
            where T : class
        {
            return (T) CreateProxy(typeof (T), instance, interceptor);
        }
        public static T CreateProxy<T>(T instance, IInterceptor[] interceptors)
            where T : class
        {
            return (T)CreateProxy(typeof(T), instance, interceptors);
        }

        public static object CreateProxy(Type serviceType, object instance, IInterceptor interceptor)
        {
            if (serviceType == null) throw new ArgumentNullException("serviceType");
            if (instance == null) throw new ArgumentNullException("instance");
            if (interceptor == null) throw new ArgumentNullException("interceptor");
            return new InnerProxy(serviceType, instance, interceptor).GetTransparentProxy();
        }

        public static object CreateProxy(Type serviceType, object instance, IInterceptor[] interceptors)
        {
            if (serviceType == null) throw new ArgumentNullException("serviceType");
            if (instance == null) throw new ArgumentNullException("instance");
            if (interceptors == null) throw new ArgumentNullException("interceptors");
            return interceptors.Length == 1 
                ? new InnerProxy(serviceType, instance, interceptors[0]).GetTransparentProxy() 
                : new InnerProxy(serviceType, instance, new InterceptorCollection(interceptors)).GetTransparentProxy();
        }
    }
}
