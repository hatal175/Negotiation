using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Negotiation.Models
{
    public class NegotiationAction
    {
        public NegotiationActionType ActionType { get; set; }
        public String UserId { get; set; }
        public int NegotiationSide { get; set; }
        TimeSpan RemainingTime { get; set; }
        Object Value { get; set; }
    }
}