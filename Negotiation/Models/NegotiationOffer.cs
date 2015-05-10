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
            Offers = new Dictionary<string, string>();
        }

        public Dictionary<String,String> Offers { get; set; }
    }
}