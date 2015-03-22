using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Negotiation.Models
{
    public abstract class BaseNegotiator
    {
        public String UserId { get; protected set; }
        public String NegotiatorRole { get; protected set; }
        public INegotiationClient Client { get; protected set; }
    }
}