using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfRadar.Services.Exceptions
{
    public class ConfRadarAuthenticationException : Exception
    {
        public ConfRadarAuthenticationException(string message)
            : base(message)
        {
        }
    }
}
