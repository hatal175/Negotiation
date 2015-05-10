using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Negotiation.Models
{
    public enum NegotiationState
    {
        [Description("Negotiation Still in prgoress")]
        InProgress,

        [Description("You opted out")]
        EndHumanOptOut,

        [Description("Your opponent opted out")]
        EndAiOptOut,

        [Description("Negotiation Timed out")]
        EndTimeout,

        [Description("Your opponent accepted your offer")]
        EndHumanOfferAccepted,

        [Description("You accepted your opponent's offer")]
        EndAiOfferAccepted
    }
}
