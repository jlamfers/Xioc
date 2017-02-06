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
using System.Collections.Generic;
using System.Reflection;
using Xioc.Core;

namespace Xioc.Proxy.Core
{
    /// <summary>
    /// The InvocationContext is an invocation that ensures all interceptors to be processed.
    /// </summary>
    internal class InvocationContext : IInvocation
    {
        private readonly IInvocation _invocation;
        private readonly IList<IInterceptor> _interceptors;
        private int _currentIndex;

        private bool _allProceedsCompleted;


        public InvocationContext(IInvocation invocation, IList<IInterceptor> interceptors)
        {
            _invocation = invocation;
            _interceptors = interceptors;
        }

        public object Target
        {
            get { return _invocation.Target; }
        }
        public IDictionary<object, object> Context
        {
            get { return _invocation.Context; }
        }

        public object[] Arguments
        {
            get { return _invocation.Arguments; }
        }
        public MethodInfo Method
        {
            get { return _invocation.Method; }
        }
        public object ReturnValue
        {
            get { return _invocation.ReturnValue; }
            set { _invocation.ReturnValue = value; }
        }

        public void Proceed()
        {
            if (AllIntercepted())
            {
                if (_allProceedsCompleted)
                {
                    throw new XiocException("Any interceptor made a Proceed() call more than once. All intercepters MUST call Proceed() exactly once inside the Intercept(invocation) implementation, or otherwise throw an exception.");
                }
                _allProceedsCompleted = true;
                // we are done with all interceptors
                // => invoke the "real" invocation
                _invocation.Proceed();
            }
            else
            {
                InterceptNext();
            }
        }

        #region Private
        private bool AllIntercepted()
        {
            return _interceptors == null || _currentIndex >= _interceptors.Count;
        }

        private void InterceptNext()
        {
            _interceptors[_currentIndex++].Intercept(this);
        }
        #endregion
    }
}