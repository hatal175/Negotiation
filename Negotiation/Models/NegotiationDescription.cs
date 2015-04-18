using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;

namespace Negotiation.Models
{
    [Serializable]
    public class NegotiationDescription<T,S> : IXmlExtractable
        where T : NegotiationTopic<S>, new()
        where S : NegotiationOption, new()
    {
        public String Name { get; set; }
        public Dictionary<String,T> Topics { get; set; }

        public virtual void Extract(System.Xml.XmlNode node)
        {
            Topics = node.ChildNodes.Cast<XmlNode>().Where(x => x.Name == "issue").Select(x => { T topic = new T(); topic.Extract(x); return topic; }).ToDictionary(x => x.Name);
        }
    }
}