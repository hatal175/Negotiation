using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Negotiation.Models
{
    public interface IAgentStrategy
    {
        TimeSpan MinimumUpdateTime {get;}

        void Initialize(NegotiationDomain domain, ScoringVariant variant, INegotiationChannel channel);
        void UpdateNegotiationStatus(NegotiationStatus status);
    }
}
