using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudLogic.Util
{
    public class BindableDateTime
    {
        public DateTime Value { get; set; }

        private BindableDateTime(DateTime value)
        {
            Value = value;
        }

        public static BindableDateTime Of(DateTime value)
        {
            return new BindableDateTime(value);
        }
    }
}
