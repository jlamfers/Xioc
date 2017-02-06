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
using System.ServiceModel;

namespace Xioc.Wcf
{
   public sealed class ServiceClient<TService> : IDisposable, IServiceClient
   {
      private TService _target;

      public TService Target
      {
         get
         {
            if (Equals(_target, default(TService)))
            {
               throw new ObjectDisposedException(GetType().Name, "WCF client has been disposed");
            }
            return _target;
         }
      }

      public IServiceClient Initialize(object target)
      {
         _target = (TService)target;
         return this;
      }

      object IServiceClient.Target
      {
         get
         {
            return Target;
         }
      }

      public void Dispose()
      {
         var target = _target as ICommunicationObject;
         _target = default(TService);
         if (target == null) return;
         if (target.State == CommunicationState.Faulted)
         {
            target.Abort();
         }
         else if (target.State != CommunicationState.Closed)
         {
            try
            {
               target.Close();
            }
            catch (CommunicationException)
            {
               target.Abort();
            }
            catch (TimeoutException)
            {
               target.Abort();
            }
            catch (Exception)
            {

               target.Abort();
               throw;
            }
         }
      }
   }
}
