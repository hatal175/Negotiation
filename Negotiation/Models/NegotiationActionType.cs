using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Negotiation.Models
{
    public enum NegotiationActionType
    {
        Connect,
        MakeChange,
        MakeOffer,
        AcceptOffer,
        Optout,
        OptoutReceive,
        Disconnect
    }
}