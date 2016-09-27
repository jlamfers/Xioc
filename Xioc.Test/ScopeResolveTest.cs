using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Xioc.Core;

namespace Xioc.Test
{

    [TestClass]
    public class ScopeResolveTest
    {
        public class Aap
        {
            public Noot Noot { get; private set; }
            public Mies Mies { get; private set; }

            public Aap()
            {
            }
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

        public class Noot { }

        public class Mies { }

        public class NootEx : Noot { }

        [TestMethod]
        public void DependenciesAreResolvedPerScope()
        {
            var c = new XiocContainer(b => b.Bind<Aap>().Bind<Noot>());

            using (var s = c.BeginScope())
            {
                var aap = s.Resolve<Aap>();
                Assert.AreSame(typeof(Noot),aap.Noot.GetType());
            }
            using (var s = c.BeginScope(b => b.Bind<Noot,NootEx>()))
            {
                var aap = s.Resolve<Aap>();
                Assert.AreSame(typeof(NootEx), aap.Noot.GetType());
            }
            using (var s = c.BeginScope())
            {
                var aap = s.Resolve<Aap>();
                Assert.AreSame(typeof(Noot), aap.Noot.GetType());
            }
        }

        [TestMethod]
        public void FactoriesAreResolvedByOriginalScope()
        {
            var c = new XiocContainer(b => b.Bind<Aap>().Bind<Noot>());

            using (var s = c.BeginScope(b => b.Bind<Mies>()))
            {
                // the original Aap-registration is used to resolve dependencies
                var aap = s.Resolve<Aap>();
                Assert.AreSame(typeof(Noot), aap.Noot.GetType());
                Assert.IsNull(aap.Mies);
            }
            using (var s = c.BeginScope(b => b.Bind<Mies>().Bind<Aap>()))
            {
                // now Aap is (re)registered as well, dependency Mies must be resolved as well, at same scope level
                var aap = s.Resolve<Aap>();
                Assert.AreSame(typeof(Noot), aap.Noot.GetType());
                Assert.AreSame(typeof(Mies), aap.Mies.GetType());
            }
        }

        [TestMethod]
        [ExpectedException(typeof(XiocException))]
        public void PerContainerInstancesCannotBeRegisteredAtScopeLevel()
        {
            var c = new XiocContainer(b => b.Bind<Aap>(Lifestyle.PerContainer));
            using (var s = c.BeginScope(b => b.Bind<Aap>(Lifestyle.PerContainer)))
            {

            }
        }

        [TestMethod]
        public void PerContainerInstancesAlwaysAreResolvedAtRootLevel()
        {
            var c = new XiocContainer(b => b.Bind<Aap>(Lifestyle.PerContainer));
            using (var s = c.BeginScope())
            {
                var aap = s.Resolve<Aap>();
                Assert.IsNull(aap.Noot);
            }
            using (var s = c.BeginScope(b => b.Bind<Noot>()))
            {
                var aap = s.Resolve<Aap>();
                // singletons must be resolved at the container root level
                Assert.IsNull(aap.Noot);
            }

            c = new XiocContainer(b => b.Bind<Aap>(Lifestyle.PerContainer).Bind<Noot>());
            using (var s = c.BeginScope())
            {
                var aap = s.Resolve<Aap>();
                Assert.IsNotNull(aap.Noot);
            }
            using (var s = c.BeginScope(b => b.Bind<Noot,NootEx>()))
            {
                var aap = s.Resolve<Aap>();
                // singletons must be resolved at the container root level
                Assert.IsNotNull(aap.Noot);
                Assert.AreSame(typeof(Noot),aap.Noot.GetType());
            }
            using (var s = c.BeginScope(b => b.Bind<Aap>().Bind<Noot, NootEx>()))
            {
                var aap = s.Resolve<Aap>();
                // App is re-registered at scope level
                Assert.IsNotNull(aap.Noot);
                Assert.AreSame(typeof(NootEx), aap.Noot.GetType());
            }

            c = new XiocContainer(b => b.Bind<Aap>(Lifestyle.PerScope).Bind<Noot>());
            using (var s = c.BeginScope())
            {
                var aap = s.Resolve<Aap>();
                Assert.IsNotNull(aap.Noot);
            }
            using (var s = c.BeginScope(b => b.Bind<Noot, NootEx>()))
            {
                var aap = s.Resolve<Aap>();
                // singletons must be resolved at the container root level
                Assert.IsNotNull(aap.Noot);
                Assert.AreSame(typeof(NootEx), aap.Noot.GetType());
            }


        }

        [TestMethod]
        public void ScopeStackWorks()
        {
            var c = new XiocContainer(b => b.Bind<Aap>().Bind<Noot>());
            using (var s = c.BeginScope())
            {
                using (var s2 = s.BeginScope())
                {
                    using (var s3 = s2.BeginScope(b => b.Bind<Mies>()))
                    {
                        using (var s4 = s3.BeginScope(b => b.Bind<Noot,NootEx>()))
                        {
                            var aap = s4.Resolve<Aap>();
                            Assert.AreEqual(typeof(NootEx),aap.Noot.GetType());
                            Assert.AreEqual(2,s4.ResolveAll<Noot>().Count());
                            Assert.IsNull(aap.Mies);// => because Aap is bound by binding at root-level, where Mies was not registered
                            using (var s5 = s4.BeginScope(b => b.Bind<Aap>()))
                            {
                                aap = s5.Resolve<Aap>();
                                Assert.AreEqual(typeof(NootEx), aap.Noot.GetType());
                                Assert.AreEqual(2, s5.ResolveAll<Noot>().Count());
                                Assert.AreEqual(2, s5.ResolveAll<Aap>().Count());
                                Assert.IsNotNull(aap.Mies); // => since Aap is re-bound
                            }
                        }
                    }
                    
                }
            }
        }
    }

}
