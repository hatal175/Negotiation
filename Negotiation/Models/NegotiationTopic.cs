using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Negotiation.Models
{
    public class NegotiationTopic<T> where T : NegotiationOption
    {
        public List<T> Options { get; set; }
    }
}