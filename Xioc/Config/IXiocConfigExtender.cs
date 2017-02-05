using System;
using System.Collections.Generic;
using System.Reflection;

namespace Xioc.Config
{
   public interface IXiocConfigExtender
   {
      void ExtendSyntax(IList<Tuple<string,MemberInfo>> tuples);
   }
}
