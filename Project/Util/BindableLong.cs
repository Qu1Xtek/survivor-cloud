using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudLogic.Util
{
    public class BindableLong
    {
        public long Value { get; set; }

        private BindableLong(long value)
        {
            Value = value;
        }

        public static BindableLong Of(long value)
        {
            return new BindableLong(value);
        }
    }
}
