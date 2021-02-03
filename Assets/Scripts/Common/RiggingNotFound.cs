using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Common
{
    class RiggingNotFound : Exception
    {

        public RiggingNotFound(string message) : base(message)
        {
        }

        public RiggingNotFound(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected RiggingNotFound(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
