namespace Assets.Scripts
{
    using System;

    class InvalidServerMessageException :Exception
    {
        public InvalidServerMessageException(string message) : base(message)
        {
        }

        public InvalidServerMessageException(string message, Exception inner): base(message, inner)
        {

        }
    }
}
