using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace Xioc.Wcf.Host
{

   public abstract class XiocServiceHostFactory<TIocInitializer> : ServiceHostFactory
           where TIocInitializer : IServiceHostIocInitializer, new()
   {

      private static class Nested
      {
         public static readonly XiocContainer Container = new XiocContainer(b => new TIocInitializer().BindTypes(b));
      }

      private class NiocServiceHost : ServiceHost
      {
         private readonly XiocContainer _baseContainer;

         public NiocServiceHost(XiocContainer baseContainer, Type serviceType, params Uri[] baseAddresses)
            : base(EnsureClassType(baseContainer, serviceType), baseAddresses)
         {
            _baseContainer = baseContainer;
         }

         protected override void OnOpening()
         {
            Description.Behaviors.Add(new NiocServiceBehavior(_baseContainer));

            base.OnOpening();
         }

         private static Type EnsureClassType(XiocContainer baseContainer, Type serviceType)
         {
            if (!serviceType.IsInterface)
            {
               return serviceType;
            }
            using (var scope = baseContainer.BeginScope())
            {
               return scope.Resolve(serviceType).GetType();
            }

         }
      }

      private class NiocInstanceProvider : IInstanceProvider
      {
         private readonly XiocContainer _baseContainer;

         private readonly Type
            _serviceType;

         public NiocInstanceProvider(XiocContainer baseContainer, Type serviceType)
         {
            if (baseContainer == null) throw new ArgumentNullException("baseContainer");
            if (serviceType == null) throw new ArgumentNullException("serviceType");
            _baseContainer = baseContainer;
            _serviceType = serviceType;
         }

         public object GetInstance(InstanceContext instanceContext, Message message)
         {
            return GetInstance(instanceContext);
         }

         public object GetInstance(InstanceContext instanceContext)
         {
            return _baseContainer.ResolveScoped(_serviceType);
         }

         public void ReleaseInstance(InstanceContext instanceContext, object instance)
         {
            _baseContainer.ReleaseScoped(instance);
         }
      }

      private class NiocServiceBehavior : IServiceBehavior
      {
         private readonly XiocContainer _baseContainer;

         public NiocServiceBehavior(XiocContainer baseContainer)
         {
            if (baseContainer == null) throw new ArgumentNullException("baseContainer");
            _baseContainer = baseContainer;
         }

         public void AddBindingParameters(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase,
            Collection<ServiceEndpoint> endpoints, BindingParameterCollection bindingParameters)
         {
         }

         public void ApplyDispatchBehavior(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
         {
            InitializeInstanceProviders(serviceDescription, serviceHostBase);
         }

         public void Validate(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
         {
         }

         private void InitializeInstanceProviders(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
         {
            var endpoints =
               serviceDescription.Endpoints.Where(
                  serviceEndpoint => serviceEndpoint.Contract.ContractType.IsAssignableFrom(serviceDescription.ServiceType))
                  .ToList();

            foreach (var channelDispatcher in serviceHostBase.ChannelDispatchers.OfType<ChannelDispatcher>())
            {
               foreach (var endpointDispatcher in channelDispatcher.Endpoints)
               {
                  var endpoint = endpoints.FirstOrDefault(ep => ep.Contract.Name == endpointDispatcher.ContractName);
                  if (endpoint == null) continue;
                  endpointDispatcher.DispatchRuntime.InstanceProvider = new NiocInstanceProvider(_baseContainer,
                     endpoint.Contract.ContractType);
               }
            }
         }

      }


      protected override ServiceHost CreateServiceHost(Type serviceType, Uri[] baseAddresses)
      {
         return new NiocServiceHost(Nested.Container, serviceType, baseAddresses);
      }
   }
}