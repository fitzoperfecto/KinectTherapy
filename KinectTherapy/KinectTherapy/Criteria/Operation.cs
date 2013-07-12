using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SWENG.Criteria
{
    [Serializable()]
    public enum Operation
    {
        Equals = 0,
        NotEquals = 1,
        LessThan = 2,
        GreaterThan = 3,
    }
}