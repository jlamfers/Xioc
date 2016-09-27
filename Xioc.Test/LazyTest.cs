using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Xioc.Test
{
    [TestClass]
    public class LazyTest
    {
        public class Aap : IDisposable {
            public Noot Noot { get; private set; }
            public Aap() { }
            public Aap(Noot noot)
            {
                Noot = noot;
            }

            public void Dispose()
            {
                this.Disposed = true;
            }

            public bool Disposed { get; set; }
        }

        public class Noot { }

        [TestMethod]
        public void LazyCanBeResolved()
        {
            var c = new XiocContainer(b => b.Bind<Aap>());
            Lazy<Aap> value;
            using (var s = c.BeginScope())
            {
                value = s.Resolve<Lazy<Aap>>();
                var aap = value.Value;
                Assert.IsFalse(aap.Disposed);
               var sw = new Stopwatch();
               sw.Start();
               for (var i = 0; i < 10000; i++)
               {
                  value = s.Resolve<Lazy<Aap>>();
               }
               sw.Stop();
               Debug.WriteLine(sw.ElapsedMilliseconds);
               var x = value.Value;

            }
            // should still work, because value was resolved
            var aap2 = value.Value;
            Assert.IsTrue(aap2.Disposed);

            using (var s = c.BeginScope())
            {
                value = s.Resolve<Lazy<Aap>>();
            }
            try
            {
                // should fail => out of scope => scope is disposed
                aap2 = value.Value;
            }
            catch (ObjectDisposedException)
            {
                return;
            }
            throw new Exception("Test failed: expected ObjectDisposedException");

        }

        [TestMethod]
        public void CollectionsCanBeResolved()
        {
            var c = new XiocContainer(b => b.Bind<Aap>(Lifestyle.PerScope).Bind<Aap>());
            IEnumerable<Aap> value;
            using (var s = c.BeginScope(b => b.Bind<Aap>()))
            {
                value = s.ResolveAll<Aap>().ToList();
            }
            // should still work, because value was resolved
            var aap2 = value.ToList();
            Assert.IsTrue(aap2.All(a => a.Disposed));
            Assert.AreEqual(3,aap2.Count);

            using (var s = c.BeginScope())
            {
                value = s.ResolveAll<Aap>();
            }
            try
            {
                // should fail => out of scope => scope is disposed
                aap2 = value.ToList();
            }
            catch (ObjectDisposedException)
            {
                return;
            }
            throw new Exception("Test failed: expected ObjectDisposedException");

        }

        [TestMethod]
        public void CollectionsCanBeResolvedFromDifferentScopesWithDifferentDependencies()
        {
            var c = new XiocContainer(b => b.Bind<Aap>(Lifestyle.PerScope).Bind<Aap>());
            IEnumerable<Aap> value;
            using (var s = c.BeginScope())
            {
                Assert.AreEqual(2, s.ResolveAll<Aap>().Count());
            }
            using (var s = c.BeginScope(b => b.Bind<Aap>().Bind<Noot>()))
            {
                value = s.ResolveAll<Aap>().ToList();
            }
            using (var s = c.BeginScope())
            {
                Assert.AreEqual(2, s.ResolveAll<Aap>().Count());
            }
            var aaps = value.ToList();
            Assert.IsTrue(aaps.All(a => a.Disposed));
            Assert.AreEqual(3, aaps.Count);
            Assert.AreEqual(2, aaps.Count(a => a.Noot == null));
            Assert.AreEqual(1, aaps.Count(a => a.Noot != null));

        }

    }
}
