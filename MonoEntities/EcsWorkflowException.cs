using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoEntities
{
    public class EcsWorkflowException : Exception
    {
        public EcsWorkflowException(string message) : base(message)
        {

        }
    }
}
