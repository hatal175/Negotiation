using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Negotiation.Models
{
    public class NegotiationStatus
    {
        public NegotiationOffer[] OpenOffers { get; set; }
        public TimeSpan RemainingTime { get; set; }
    }
}