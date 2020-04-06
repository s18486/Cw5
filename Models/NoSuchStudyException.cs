using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cw5.Models
{
    public class NoSuchStudyException : Exception
    {
        public override string Message => "No such study";
    }
}
