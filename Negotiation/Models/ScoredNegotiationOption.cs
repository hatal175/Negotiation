using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Negotiation.Models
{
    public class ScoredNegotiationOption : NegotiationOption, IXmlExtractable
    {
        public ScoredNegotiationOption()
        {

        }

        public int Score { get; set; }

        public override void Extract(System.Xml.XmlNode node)
        {
            base.Extract(node);
            Score = int.Parse(node.Attributes["evaluation"].Value);
        }
    }
}