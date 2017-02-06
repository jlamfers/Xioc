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
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Xioc.Mvc5
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