namespace Assets.Scripts.Common
{
    using log4net;
    using System;

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
