using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Xioc.Test
{
    [TestClass]
    public class UnmanagedTest
    {
        public class Disposable : IDisposable {
            public void Dispose()
            {
                Disposed = true;
            }

            public bool Disposed { get; set; }
        }
        public class Aap : Disposable
        {
            public Noot Noot { get; set; }
            public Mies Mies { get; set; }
            public Aap() { }
            public Aap(Noot noot)
            {
                Noot = noot;
            }

            public Aap(Noot noot, Mies mies)
            {
                Noot = noot;
                Mies = mies;
            }
        }

        public class Noot : Disposable { }
        public class Mies : Disposable { }
        public class NootEx : Noot { }

        [TestMethod]
        public void UnmanagedWorks()
        {
            var c = new XiocContainer(b => b.Bind<Noot>(Lifestyle.PerContainer).Bind<Aap>());
            Aap aap;
            Noot noot;
            using (var s = c.BeginScope())
            {
                aap = s.Resolve<Aap>();
                noot = aap.Noot;
                Assert.AreSame(aap.Noot,s.Resolve<Aap>().Noot);
            }
            Assert.IsTrue(aap.Disposed);
            Assert.IsFalse(noot.Disposed);

            using (var s = c.BeginScope(b => b.Bind<Aap>(Lifestyle.UnManaged)))
            {
                aap = s.Resolve<Aap>();
                noot = aap.Noot;
                Assert.AreNotSame(aap.Noot, s.Resolve<Aap>().Noot);
            }
            Assert.IsFalse(aap.Disposed);
            Assert.IsFalse(noot.Disposed);

            using (var s = c.BeginScope(b => b.Bind<Aap>(Lifestyle.TransientNoDispose)))
            {
                aap = s.Resolve<Aap>();
                noot = aap.Noot;
                Assert.AreSame(aap.Noot, s.Resolve<Aap>().Noot);
            }
            Assert.IsFalse(aap.Disposed);
            Assert.IsFalse(noot.Disposed);
        }
    }
}
