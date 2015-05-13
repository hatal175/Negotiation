using Negotiation.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DonaStrategy
{
    public class OfferUtility
    {
        public NegotiationOffer Offer { get; set; }
        public double Utility { get; set; }
        public Dictionary<String, UtilityData> UtilityDataDict { get; set; }
    }
}
