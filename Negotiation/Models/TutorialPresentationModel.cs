using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Negotiation.Models
{
    public class TutorialPresentationModel
    {
        public SideConfig HumanConfig { get; set; }
        public SideConfig AiConfig { get; set; }
        public NegotiationDomain Domain { get; set; }
    }
}