using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;

namespace Negotiation.Models
{
    public class NegotiationDomain : IXmlExtractable
    {
        public String Description { get; set; }

        public Dictionary<String, Dictionary<String,NegotiationSideDescription>> OwnerVariantDict { get; private set; }

        public NegotiationDescription<NegotiationTopic<NegotiationOption>,
            NegotiationOption> Options { get; private set; }

        public TimeSpan RoundLength { get; set; }
        public int NumberOfRounds { get; set; }


        public void Extract(System.Xml.XmlNode node)
        {
            Description = node.SelectSingleNode("//description").InnerText;

            XmlNode utilityNode = node.SelectSingleNode("//utility_space");

            Options = new NegotiationDescription<NegotiationTopic<NegotiationOption>, NegotiationOption>();
            Options.Extract(utilityNode.SelectSingleNode("//objective"));

            OwnerVariantDict = utilityNode.SelectNodes("//agent").Cast<XmlNode>().Select(x => new
            {
                Owner = x.Attributes["owner"].Value,
                                                                                                              Description = ExtractVariant(x),
                                                                                                              Name = x.Attributes["personality"].Value
            }).GroupBy(x=>x.Owner).ToDictionary(x=>x.Key,x=>x.ToDictionary(y=>y.Name,y=>y.Description));
        }

        private NegotiationSideDescription ExtractVariant(XmlNode node)
        {
            NegotiationSideDescription desc = new NegotiationSideDescription();

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(NegotiationDomainManager.GameDomain.DomainVariant.First(x => x.Name == node.Attributes["utility_space"].Value).VariantXML);
            desc.Extract(doc);

            return desc;
        }
    }
}