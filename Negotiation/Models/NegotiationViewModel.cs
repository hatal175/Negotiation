using Negotiation.App_Start;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Negotiation.Models
{
    public class NegotiationViewModel
    {
        public NegotiationViewModel()
        {
            Actions = new List<NegotiationActionModel>();
            Offer = new NegotiationOffer();
            OpponentOffer = new NegotiationOffer();
        }

        public String Id { get; set; }

        public SideConfig HumanConfig { get; set; }
        public String AiSide { get; set; }
        public NegotiationDomain Domain { get; set; }

        public NegotiationOffer Offer { get; set; }
        public NegotiationOffer OpponentOffer { get; set; }

        public TimeSpan RemainingTime { get; set; }

        public List<NegotiationActionModel> Actions { get; set; }
    }
}