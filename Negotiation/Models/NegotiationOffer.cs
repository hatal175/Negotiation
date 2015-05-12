using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Negotiation.Models
{
    public class NegotiationOffer : IEquatable<NegotiationOffer>
    {
        public NegotiationOffer()
        {
            Offers = new Dictionary<string, string>();
        }

        public Dictionary<String,String> Offers { get; set; }

        public bool Equals(NegotiationOffer other)
        {
            return (other != null && Offers.Count == other.Offers.Count && !Offers.Except(other.Offers).Any());
        }
    }
}