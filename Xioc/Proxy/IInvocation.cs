using System.Collections.Generic;
using System.Reflection;

namespace Xioc.Proxy
{
    public interface IInvocation
    {
        object Target { get; }
        object[] Arguments { get; }
        MethodInfo Method { get; }
        object ReturnValue { get; set; }
        IDictionary<object, object> Context { get; }
        void Proceed();
    }
}