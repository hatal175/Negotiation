using Negotiation.App_Start;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Negotiation.Models
{
    public class NegotiationViewModel
    {
        public String Id { get; set; }

        public SideConfig HumanConfig { get; set; }
        public NegotiationDomain Domain { get; set; }

        public NegotiationOffer Offer { get; set; }

        public TimeSpan RemainingTime { get; set; }

        public List<NegotiationAction> Actions { get; set; }
    }
}