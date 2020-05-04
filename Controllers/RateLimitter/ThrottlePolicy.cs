using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core_all.Controllers.RateLimitter
{
    public class ThrottlePolicy
    {
        public long? perSecond = 1;
        public long? perMinute = 30;
        public long? perHour = 1000;
        public long? perDay = 10000;
        public long? perWeek = 10000;

        public ThrottlePolicy()
        {

        }

    }
}
