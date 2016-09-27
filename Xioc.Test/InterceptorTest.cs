using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Xioc.Proxy;

namespace Xioc.Test
{
   [TestClass]
   public class InterceptorTest
   {
      public interface IFoo
      {
         bool Doit();
      }

      public class Foo : MarshalByRefObject, IFoo
      {
         public bool Doit()
         {
            return true;
         }
      }

      [TestMethod]
      public void MonkeyTest()
      {
         var c = new XiocContainer(b => b.Bind<IFoo, Foo>().Bind<IFoo, Foo>().Bind<IFoo, Foo>().Bind<Foo>().Bind<Foo>().Bind<Foo>().Bind<Foo>().Bind<Foo>());
         using (var s = c.BeginScope(b => b.Bind<IFoo, Foo>().Intercept<IFoo>(i =>
         {
            Debug.Write(i.Method + " => ");
            i.Proceed();
            Debug.WriteLine(i.ReturnValue);
            i.ReturnValue = false;
         })))
         {
            s.ResolveAll<IFoo>().ToList().ForEach(i => i.Doit());
            Debug.WriteLine("interfaces intercepted A.");
            using (var s2 = s.BeginScope(b => b.Intercept<IFoo>(i =>
            {
               Debug.Write(i.Method + " => ");
               i.Proceed();
               Debug.WriteLine(i.ReturnValue);
            })))
            {
               s2.ResolveAll<IFoo>().ToList().ForEach(i => i.Doit());
            }
         }
         Debug.WriteLine("interfaces intercepted B.");
         using (var s = c.BeginScope(b => b.Intercept<Foo>(i =>
         {
            Debug.Write(i.Method + " => ");
            i.Proceed();
            Debug.WriteLine(i.ReturnValue);
         })))
         {
            s.ResolveAll<Foo>().ToList().ForEach(i => i.Doit());
         }

      }
   }
}
