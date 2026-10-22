using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudLogic.Util
{
    public class BindableDouble
    {
        public double Value { get; set; }

        private BindableDouble(double value)
        {
            Value = value;
        }

        public static BindableDouble Of(double value)
        {
            return new BindableDouble(value);
        }
    }
}
