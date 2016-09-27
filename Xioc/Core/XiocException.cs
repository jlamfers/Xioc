using System;
using System.Runtime.Serialization;

namespace Xioc.Core
{
   [Serializable]
   public class XiocException : Exception
   {

      public XiocException()
      {
      }

      public XiocException(string message)
         : base(message)
      {
      }

      public XiocException(string message, Exception inner)
         : base(message, inner)
      {
      }

      protected XiocException(
          SerializationInfo info,
          StreamingContext context)
         : base(info, context)
      {
      }
   }
}
