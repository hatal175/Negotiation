using Negotiation.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DonaStrategy
{
    public class ExtendedTimePassedEventArgs : TimePassedEventArgs
    {
        public Boolean HasRoundPassed { get; set; }

        public ExtendedTimePassedEventArgs(TimeSpan remainingTime, Boolean roundPassed)
            : base(remainingTime)
        {
            HasRoundPassed = roundPassed;
        }
    }
}
