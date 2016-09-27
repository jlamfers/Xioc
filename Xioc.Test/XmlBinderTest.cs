using System;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Xioc.Core;
using Xioc.Test.Shared;
using Xioc.Xml;

namespace Xioc.Test
{
   public interface IFoo { }
   public class Foo : IFoo { }

   [XmlConfigElement]
   public class TestXmlConfigElement : XmlConfigElementBinder
   {
      public TestXmlConfigElement() : base("debug-write", "text")
      {
      }

      protected override Action<IBinder> CreateBinder(XElement e)
      {
         var text = e.GetAttributeValue(RequiredAttributes[0]);
         return b => Debug.Write(text);
      }

   }


   [TestClass]
   public class XmlBinderTest
   {
      [TestMethod]
      public void MonkeyTest()
      {
         var c = new XiocContainer(b => b.BindXml("xioc.xml").BindMefExports(AppDomain.CurrentDomain.GetAssembliesFromDirectory("Plugins")));
         var sw = new Stopwatch();
         sw.Start();
         for (var i = 0; i < 100; i++)
         {
            using (var s = c.BeginScope(b => b.BindXml("xioc.xml")))
            {
               var list = s.ResolveAll<IFoo>();
               var plugins = s.ResolveAll<IMyPlugin>().ToList();
               Assert.AreEqual(2,list.Count());
            }
         }
         sw.Stop();
         Debug.WriteLine(sw.ElapsedMilliseconds);

      }

      [TestMethod]
      public void RoleTest()
      {
         var xml = @"
<xioc>
   <if>
      <condition>
         <not>
            <role any-of='oops' type='windows'/>
         </not>
      </condition>
      <then>
         <debug-write text='NO!'/>
      </then>
   </if>
   <if>
      <condition>
         <not>
            <role any-of='oops' type='windows'/>
         </not>
      </condition>
      <then>
         <debug-write text='NO Again!'/>
      </then>
   </if>

   <if>
      <condition>
         <not>
            <not>
               <role any-of='Administrators' type='windows'/>
            </not>
         </not>
      </condition>
      <then>
         <debug-write text='Yes, Administrator!'/>
      </then>
   </if>

</xioc>
";
         var c = new XiocContainer(b => b.BindXml(xml));
         var sw = new Stopwatch();
         sw.Start();
         for (var i = 0; i < 1; i++)
         {
            using (var s = c.BeginScope(b => b.BindXml(xml)))
            {
               var list = s.ResolveAll<IFoo>();
            }
         }
         sw.Stop();
         Debug.WriteLine(sw.ElapsedMilliseconds);

      }
   }
}
