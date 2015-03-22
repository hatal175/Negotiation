using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Negotiation.Models
{
    public class NegotiationDomain
    {
        public String Description { get; set; }

        public Dictionary<String, Dictionary<String,NegotiationSideDescription>> OwnerVariantDict { get; private set; }

        public NegotiationDescription<NegotiationTopic<NegotiationOption>,
            NegotiationOption> Options { get; private set; }
    }
}