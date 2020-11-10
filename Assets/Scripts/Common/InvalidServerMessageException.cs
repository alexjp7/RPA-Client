using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts
{
    class InvalidServerMessageException:Exception
    {
        public InvalidServerMessageException(string message) : base(message)
        {
        }

        public InvalidServerMessageException(string message, Exception inner): base(message, inner)
        {

        }
    }
}
