using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Negotiation.Models
{
    public class NegotiationSideDescription :
        NegotiationDescription<NegotiationTopic<ScoredNegotiationOption>,
        ScoredNegotiationOption>
    {
        public int Reservation { get; set; }
        public int Optout { get; set; }
        public int TimeEffect { get; set; }

        public override void Extract(System.Xml.XmlNode node)
        {
            base.Extract(node.SelectSingleNode("//objective"));
            Reservation = int.Parse(node.SelectSingleNode("//reservation").Attributes["value"].Value);
            TimeEffect = int.Parse(node.SelectSingleNode("//timeeffect").Attributes["value"].Value);
            Optout = int.Parse(node.SelectSingleNode("//optout").Attributes["value"].Value);
        }
    }
}