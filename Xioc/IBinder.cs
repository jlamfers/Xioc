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
using Xioc.Core;

namespace Xioc
{
   public interface IBinder : IContainerBase
   {
      IBinder Bind(Type serviceType, Type implementationType, Lifestyle lifestyle = Lifestyle.Transient, IDictionary<string, object> dependencies = null);
      IBinder Bind(Type serviceType, Func<Context, object> factory, Lifestyle lifestyle = Lifestyle.Transient);
      IBinder Decorate(Type serviceType, Type decoratorType, Lifestyle lifestyle = Lifestyle.Transient, IDictionary<string, object> dependencies = null);
      IBinder Decorate(Type serviceType, Func<Context, object> factory, Lifestyle lifestyle = Lifestyle.Transient, IDictionary<string, object> dependencies = null);
      IBinder OnNewScope(Action<IBinder> binder, int level = 0);
      IContainer Container { get; }
      IScope Scope { get; }
   }
}