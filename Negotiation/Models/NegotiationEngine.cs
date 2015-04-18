using Negotiation.App_Start;
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
            INegotiationChannel[] channels,
            SideConfig humanConfig,
            SideConfig aiConfig)
        {
            Domain = domain;
            _channels = channels;
            HumanConfig = humanConfig;
            AiConfig = aiConfig;
        }

        public NegotiationDomain Domain { get; private set; }
        public SideConfig HumanConfig { get; set; }
        public SideConfig AiConfig { get; set; }

        public void BeginNegotiation()
        {

        }



    }
}