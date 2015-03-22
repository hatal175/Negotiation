using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Negotiation.Models
{
    public class ScoredNegotiationOption : NegotiationOption
    {
        public int Score { get; set; }
    }
}