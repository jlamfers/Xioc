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

   public abstract class NiocServiceHostFactory<TIocInitializer> : ServiceHostFactory
           where TIocInitializer : IServiceHostIocInitializer, new()
   {

      protected static class Container
      {
         // singleton container in application domain, initialized once per application domain
         public static readonly XiocContainer Instance = new XiocContainer(b => new TIocInitializer().BindTypes(b));
      }

      protected override ServiceHost CreateServiceHost(Type serviceType, Uri[] baseAddresses)
      {
         return new XiocServiceHost(Container.Instance, serviceType, baseAddresses);
      }
   }

   public class XiocServiceHost : ServiceHost
   {
      protected readonly XiocContainer Container;

      public XiocServiceHost(XiocContainer container, Type serviceType, params Uri[] baseAddresses)
         : base(EnsureClassType(container, serviceType), baseAddresses)
      {
         Container = container;
      }

      protected override void OnOpening()
      {
         Description.Behaviors.Add(new NiocServiceBehavior(Container));

         base.OnOpening();
      }

      internal protected virtual void ApplyDispatchBehavior(ServiceDescription serviceDescription)
      {
         foreach (var channelDispatcher in ChannelDispatchers.OfType<ChannelDispatcher>())
         {
            foreach (var faultHandler in Scope.ResolveAll<IErrorHandler>())
            {
               channelDispatcher.ErrorHandlers.Add(faultHandler);
            }

            foreach (var endpoint in channelDispatcher.Endpoints)
            {
               foreach (var inspector in Scope.ResolveAll<IDispatchMessageInspector>())
               {
                  endpoint.DispatchRuntime.MessageInspectors.Add(inspector);
               }
            }

         }

      }

      IScope _scope;
      private readonly object _synclock = new object();
      protected IScope Scope
      {
         get
         {
            lock (_synclock)
            {
               return _scope ?? (_scope = Container.BeginScope());
            }
         }
      }

      protected override void OnClose(TimeSpan timeout)
      {
         base.OnClose(timeout);

         lock (_synclock)
         {
            if (_scope != null)
            {
               _scope.Dispose();
            }
            _scope = null;
         }
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

   public class NiocInstanceProvider : IInstanceProvider
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

   public class NiocServiceBehavior : IServiceBehavior
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
         var host = serviceHostBase as XiocServiceHost;
         if (host != null)
         {
            host.ApplyDispatchBehavior(serviceDescription);
         }
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


}