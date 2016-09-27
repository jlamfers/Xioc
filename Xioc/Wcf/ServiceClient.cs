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
            catch (CommunicationException e)
            {
               target.Abort();
            }
            catch (TimeoutException e)
            {
               target.Abort();
            }
            catch (Exception e)
            {

               target.Abort();
               throw;
            }
         }
      }
   }
}
