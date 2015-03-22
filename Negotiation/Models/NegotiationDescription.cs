using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Negotiation.Models
{
    [Serializable]
    public class NegotiationDescription<T,S> 
        where T : NegotiationTopic<S>
        where S : NegotiationOption
    {
        public String Name { get; set; }
        public List<T> Topics { get; set; }
    }
}