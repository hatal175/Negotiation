using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Negotiation.Models
{
    public class TimePassedEventArgs : EventArgs
    {
        public TimePassedEventArgs(TimeSpan remainingTime)
        {
            RemainingTime = remainingTime;
        }

        public TimeSpan RemainingTime { get; set; }
    }
}
