using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace Xioc.Xml
{
   public interface IXmlConfigElement
   {
      bool IsPredicate { get; }
      string ElementName { get; }
      IList<string> RequiredAttributes { get; }
      IList<string> OptionalAttributes { get; }
      Action<IBinder> CreateBinder(XElement e);
      Func<bool> CreatePredicate(XElement e);
      void Validate(XElement e);
   }
}