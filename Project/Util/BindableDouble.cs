using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudLogic.Util
{
    public class BindableInt
    {
        public int Value { get; set; }

        private BindableInt(int value)
        {
            Value = value;
        }

        public static BindableInt Of(int value)
        {
            return new BindableInt(value);
        }
    }
}
