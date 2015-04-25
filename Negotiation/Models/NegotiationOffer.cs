using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Negotiation.Models
{
    public class NegotiationOffer
    {
        public NegotiationOffer()
        {
            Offers = new Dictionary<string, NegotiationTopicOffer>();
        }

        public Dictionary<String,NegotiationTopicOffer> Offers { get; set; }

        public int Score { get; set; }
    }
}