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

        public NegotiationOffer(IEnumerable<KeyValuePair<string, string>> offers)
            : this(offers.ToDictionary(x=>x.Key,x=>x.Value))
        {
        }

        public NegotiationOffer(Dictionary<string, string> offers)
        {
            Offers = offers;
        }

        public Dictionary<String,String> Offers { get; set; }

        public bool Equals(NegotiationOffer other)
        {
            return (other != null && Offers.Count == other.Offers.Count && !Offers.Except(other.Offers).Any());
        }

        public override int GetHashCode()
        {
            return Offers.Aggregate(0, (x, y) => x ^ y.GetHashCode());
        }
    }
}