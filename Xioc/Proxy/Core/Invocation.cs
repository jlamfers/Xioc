using System.Collections.Generic;
using System.Reflection;

namespace Xioc.Proxy.Core
{
    internal class Invocation : IInvocation
    {
        private IDictionary<object, object> _context;

        public object Target { get; set; }
        public object[] Arguments { get; set; }
        public MethodInfo Method { get; set; }
        public object ReturnValue { get; set; }

        public IDictionary<object, object> Context
        {
            get { return _context ?? (_context = new Dictionary<object, object>()); }
        }

        public void Proceed()
        {
            ReturnValue = Method.Invoke(Target,Arguments);
        }
    }
}