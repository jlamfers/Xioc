using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Xioc.Config;
using Xioc.Proxy;
using Xioc.Test.Shared;

namespace Xioc.Test
{

   public interface IFoo
   {
      void Doit();
   }

   public class Foo : IFoo
   {
      public void Doit()
      {
         Debug.WriteLine(GetType().Name+".Doit");
      }
   }
   public class MyFoo2 : Foo
   {
      public DateTime CreatedAt { get; private set; }

      public MyFoo2(DateTime createdAt)
      {
         CreatedAt = createdAt;
      }
   }

   public class FooDecorator : IFoo
   {
      private readonly IFoo _foo;

      public FooDecorator(IFoo foo)
      {
         _foo = foo;
      }

      public void Doit()
      {
         Debug.WriteLine("FooDecorator.Doit");
         _foo.Doit();
      }
   }

   public class MyInterceptor : IInterceptor
   {
      public void Intercept(IInvocation invocation)
      {
         Debug.WriteLine("Intercepting: " + invocation.Method);
         invocation.Proceed();
      }
   }


   [TestClass]
   public class ConfigBinderTest
   {


      [TestMethod]
      public void MonkeyTest()
      {
         var c = new XiocContainer(b => b.BindFromConfiguration("xioc.setup"));
         using (var s = c.BeginScope())
         {
            var list = s.ResolveAll<IFoo>().ToList();
            var plugins = s.ResolveAll<IMyPlugin>().ToList();
            //Assert.AreEqual(2, list.Count());
            list[0].Doit();
            list[1].Doit();
         }
         var sw = new Stopwatch();
         sw.Start();
         for (var i = 0; i < 1; i++)
         {
            using (var s = c.BeginScope())
            {
               var list = s.ResolveAll<IFoo>();
               var plugins = s.ResolveAll<IMyPlugin>().ToList();
            }
         }
         sw.Stop();
         Debug.WriteLine(sw.ElapsedMilliseconds);

      }

   }
}
