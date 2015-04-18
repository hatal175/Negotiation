using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Negotiation.Models
{
    public class NegotiationActionModel
    {
        public String Role { get; set; }
        public NegotiationActionType Type { get; set; }
        public String Value { get; set; }
    }
}
