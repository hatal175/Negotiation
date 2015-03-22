using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Negotiation.Models
{
    public class NegotiationSideDescription :
        NegotiationDescription<NegotiationTopic<ScoredNegotiationOption>,
        ScoredNegotiationOption>
    {
        public int Reservation { get; set; }
        public int Optout { get; set; }
        public int TimeEffect { get; set; }
    }
}