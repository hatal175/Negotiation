using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Negotiation.Models
{
    public class AgentNegotiator : BaseNegotiator
    {
        public IAgentStrategy AgentStrategy { get; set; }
    }
}