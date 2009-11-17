using System;
using System.Collections.Generic;
using System.Text;

namespace IphoneComm
{
    class IphoneCommException : Exception
    {
        public IphoneCommException ()
        {
        }

        public IphoneCommException(String Message) : base(Message)
        {
        }
    }
}
