using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Negotiation.Models
{
    public class NegotiationConfigurationModel<T>
    {
        public IEnumerable<T> Items { get; set; }
        public int ActiveId { get; set; }
    }

    public class NegotiationDomainConfig : NegotiationConfigurationModel<GameDomain>
    {
        public String HumanSide { get; set; }
        public String AiVariant { get; set; }
        public String HumanVariant { get; set; }
    }
}