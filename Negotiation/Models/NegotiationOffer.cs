using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Negotiation.Models
{
    public class NegotiationOffer
    {
        public Dictionary<String,NegotiationTopicOffer> Offers { get; set; }
    }
}