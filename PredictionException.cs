using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HouseProphecy
{    
    /// <summary>
    /// generate this exception, when some troubles with prediction happens
    /// </summary>
    public class PredictionException : Exception
    {
        public PredictionException()
        {
        }

        public PredictionException(string message) : base(message)
        {
        }

        public PredictionException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}
