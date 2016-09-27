using System;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Xioc.Core;

namespace Xioc.Test
{
   [TestClass]
   public class AsyncTest
   {
      public class Foo : IDisposable
      {
         public IASyncManager ASync { get; private set; }

         public Foo(IASyncManager @async)
         {
            ASync = @async;
         }

         public void Dispose()
         {
            Disposed = true;
         }

         public bool Disposed { get; private set; }
      }

      [TestMethod]
      public void ScopeIsNotDisposedBeforeTaskCompletes()
      {
         var c = new XiocContainer(b => b.Bind<Foo>());
         var stop1 = false;
         var stop2 = false;
         Foo fooInner, fooOuter;
         using (var outerScope = c.BeginScope())
         {
            fooOuter = outerScope.Resolve<Foo>();

            using (var innerScope = outerScope.BeginScope())
            {
               fooInner = innerScope.Resolve<Foo>();
               var s1 = innerScope;
               innerScope.ExecuteAsync(() =>
               {
                  while (!stop1)
                  {
                     s1.Resolve<Foo>();
                     Thread.Sleep(0);
                  }
               });
               fooInner.ASync.ExecuteAsync(() =>
               {
                  while (!stop2)
                  {
                     Thread.Sleep(0);
                  }
               });
            }
         }

         Assert.IsTrue(fooOuter.Disposed); // no task dependency

         Assert.IsFalse(fooInner.Disposed); // tasks still running
         stop1 = true; // cancel task 1
         Thread.Sleep(100);
         Assert.IsFalse(fooInner.Disposed); // tasks still running

         stop2 = true; // cancel task 2
         Thread.Sleep(100);
         Assert.IsTrue(fooInner.Disposed);
      }
   }
}
