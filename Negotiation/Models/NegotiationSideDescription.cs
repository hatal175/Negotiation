using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;

namespace Negotiation.Models
{
    public class NegotiationSideDescription :
        NegotiationDescription<NegotiationTopic<ScoredNegotiationOption>,
        ScoredNegotiationOption>
    {
        public int Reservation { get; set; }
        public int Optout { get; set; }
        public int TimeEffect { get; set; }

        public override void Extract(XmlNode node)
        {
            int weightMultiplier = int.Parse(node.SelectSingleNode("//weightmultiplyer").Attributes["value"].Value);

            var objectiveNode = node.SelectSingleNode("//objective");
            base.Extract(objectiveNode);

            var weights = objectiveNode.ChildNodes.Cast<XmlNode>().Where(x => x.Name == "weight").ToDictionary(x => int.Parse(x.Attributes["index"].Value), x => double.Parse(x.Attributes["value"].Value));
            
            foreach (var topic in Topics.Values.ToDictionary(x=>x.Index))
            {
                int weight = (int)(weightMultiplier * weights[topic.Key]);

                foreach(var option in topic.Value.Options)
                {
                    option.Value.Score *= weight;
                }
            }
            
            Reservation = int.Parse(node.SelectSingleNode("//reservation").Attributes["value"].Value);
            TimeEffect = int.Parse(node.SelectSingleNode("//timeeffect").Attributes["value"].Value);
            Optout = int.Parse(node.SelectSingleNode("//optout").Attributes["value"].Value);
        }
    }
}