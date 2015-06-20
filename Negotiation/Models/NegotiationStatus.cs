using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Negotiation.Models
{
    public class SideStatus
    {
        public SideStatus()
        {
            Signed = false;
        }

        public NegotiationOffer Offer { get; set; }
        public int Score { get; set; }
        public Boolean Signed { get; set; }
    }

    public class NegotiationStatus
    {
        public SideStatus HumanStatus { get; set; }
        public SideStatus AiStatus { get; set; }
        public TimeSpan RemainingTime { get; set; }
        public NegotiationState State {get; set;}
        public NegotiationOffer LastAcceptedOffer { get; set; }
        public INegotiationChannel AcceptedOfferSide { get; set; }
    }
}