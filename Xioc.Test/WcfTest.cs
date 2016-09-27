using System;
using System.Diagnostics;
using System.ServiceModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Xioc.Wcf;

namespace Xioc.Test
{
    [TestClass]
    public class WcfTest
    {
        public interface IWcfContract
        {
            void Foo();
        }

        public class WcfService : IWcfContract, IDisposable, ICommunicationObject
        {
            public void Foo()
            {
                
            }

            public void Dispose()
            {
                Disposed = true;
            }

            public bool Disposed { get; set; }
            public void Abort()
            {
                Aborted = true;
            }

            public bool Aborted { get; set; }

            public virtual void Close()
            {
                IsClosed = true;
            }

            public bool IsClosed { get; set; }

            public void Close(TimeSpan timeout)
            {
                throw new NotImplementedException();
            }

            public IAsyncResult BeginClose(AsyncCallback callback, object state)
            {
                throw new NotImplementedException();
            }

            public IAsyncResult BeginClose(TimeSpan timeout, AsyncCallback callback, object state)
            {
                throw new NotImplementedException();
            }

            public void EndClose(IAsyncResult result)
            {
                throw new NotImplementedException();
            }

            public void Open()
            {
                throw new NotImplementedException();
            }

            public void Open(TimeSpan timeout)
            {
                throw new NotImplementedException();
            }

            public IAsyncResult BeginOpen(AsyncCallback callback, object state)
            {
                throw new NotImplementedException();
            }

            public IAsyncResult BeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
            {
                throw new NotImplementedException();
            }

            public void EndOpen(IAsyncResult result)
            {
                throw new NotImplementedException();
            }

            public CommunicationState State { get; private set; }
            public event EventHandler Closed;
            public event EventHandler Closing;
            public event EventHandler Faulted;
            public event EventHandler Opened;
            public event EventHandler Opening;
        }
        public class WcfServiceWithError : WcfService
        {
            public override void Close()
            {
                throw new TimeoutException();
            }
        }

        [TestMethod]
        public void WcfBindingWorks()
        {
            var c = new XiocContainer(b => b.BindWcfService<IWcfContract, WcfService>());
            IWcfContract srv;
            using (var s = c.BeginScope())
            {
                srv = s.Resolve<IWcfContract>();
                Assert.IsNotNull(srv);
                var srv2 = s.Resolve<IWcfContract>();
                Assert.AreSame(srv,srv2);
            }
            var service = (WcfService) srv;
            Assert.IsFalse(service.Disposed);
            Assert.IsFalse(service.Aborted);
            Assert.IsTrue(service.IsClosed);

            using (var s = c.BeginScope(b => b.BindWcfService<IWcfContract, WcfServiceWithError>()))
            {
                srv = s.Resolve<IWcfContract>();
                Assert.IsNotNull(srv);
                var srv2 = s.Resolve<IWcfContract>();
                Assert.AreSame(srv, srv2);
            }
            service = (WcfService)srv;
            Assert.IsFalse(service.Disposed);
            Assert.IsTrue(service.Aborted);
            Assert.IsFalse(service.IsClosed);
        }

       [TestMethod]
       public void TestValueTYpe()
       {
          Debug.WriteLine(typeof(float).IsValueType);
       }


    }
}
