using System;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Xioc.Test
{
   [TestClass]
   public class DependenciesTest
   {
      public class Boo { }

      public class Foo
      {
         public string Aap { get; set; }
         public int Noot { get; set; }
         public Boo Boo { get; set; }
         public DateTime? Mies { get; set; }

         public Foo()
         {
            
         }

         public Foo(string aap)
         {
            Aap = aap;
         }
         public Foo(string aap, int noot, Boo boo)
         {
            Aap = aap;
            Noot = noot;
            Boo = boo;
         }
         public Foo(string aap, int noot, DateTime? mies)
         {
            Aap = aap;
            Noot = noot;
            Mies = mies;
         }
      }

      [TestMethod]
      public void DependenciesAreAssigned()
      {
         IContainer c = new XiocContainer(b => b.Bind<Boo>());

         using (var s = c.BeginScope(b => b
            .Bind<Foo>(dependencies: WithDependencies
               .AddParam("aap", "yep")
               .AddParam("noot", 11)
               .AddParam("mies", DateTime.Today))))
         {
            var f = s.Resolve<Foo>();
            Assert.AreEqual("yep",f.Aap);
            Assert.AreEqual(11, f.Noot);
            Assert.AreEqual(DateTime.Today, f.Mies);
            Assert.IsNull(f.Boo);
        }

         using (var s = c.BeginScope(b => b
            .Bind<Foo>(dependencies: WithDependencies
               .AddParam("aap", "aaa")
               .AddParam("noot", 12)
               )))
         {
            var f = s.Resolve<Foo>();
            Assert.AreEqual("aaa", f.Aap);
            Assert.AreEqual(12, f.Noot);
            Assert.AreEqual(null, f.Mies);
            Assert.IsNotNull(f.Boo);

            var sw = new Stopwatch();
            sw.Start();
            for (var i = 0; i < 100000; i++)
            {
               s.Resolve<Foo>();
            }
            sw.Stop();
            Debug.WriteLine(sw.ElapsedMilliseconds);
         }

         using (var s = c.BeginScope(b => b
            .Bind<Foo>(dependencies: WithDependencies
               .AddParam("aap", () => "aaa")
               .AddParam("noot", () => 12)
               )))
         {
            var f = s.Resolve<Foo>();
            Assert.AreEqual("aaa", f.Aap);
            Assert.AreEqual(12, f.Noot);
            Assert.IsNull(f.Mies);
            Assert.IsNotNull(f.Boo);
            var sw = new Stopwatch();
            sw.Start();
            for (var i = 0; i < 100000; i++)
            {
               s.Resolve<Foo>();
            }
            sw.Stop();
            Debug.WriteLine(sw.ElapsedMilliseconds);
         }

         using (var s = c.BeginScope(b => b
            .Bind<Foo>(dependencies: WithDependencies
               .AddParam("aap", x => "aaa")
               .AddParam("noot", x => 12)
               .AddParam("mies", x => x.Scope.TryResolve<DateTime?>())
               )))
         {
            var f = s.Resolve<Foo>();
            Assert.AreEqual("aaa", f.Aap);
            Assert.AreEqual(12, f.Noot);
            Assert.IsNull(f.Mies);
            Assert.IsNull(f.Boo);
         }

      }
   }
}
