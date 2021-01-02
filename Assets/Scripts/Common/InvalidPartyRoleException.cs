using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Common
{
    class InvalidPartyRoleException : Exception
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(InvalidPartyRoleException));

        public InvalidPartyRoleException(string message) : base(message)
        {
        }

        public InvalidPartyRoleException(string message, Exception innerException) : base(message, innerException)
        {
        }

    }
}
