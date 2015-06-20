using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Negotiation.Models
{
    public class OfferViewModel
    {
        public NegotiationOffer Offer { get; set; }
        public String DataPrefix { get; set; }
    }
}