using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Negotiation.Models
{
    public enum NegotiationState
    {
        InProgress,
        EndHumanOptOut,
        EndAiOptOut,
        EndTimeout,
        EndHumanOfferAccepted,
        EndAiOfferAccepted
    }
}
