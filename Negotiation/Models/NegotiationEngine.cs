using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Negotiation.Models
{
    public class NegotiationEngine
    {
        private INegotiationChannel[] _channels;

        public NegotiationEngine(NegotiationDomain domain, 
            INegotiationChannel[] channels)
        {
            Domain = domain;
            _channels = channels;
        }

        public NegotiationDomain Domain { get; private set; }

        public void BeginNegotiation()
        {

        }



    }
}