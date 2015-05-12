using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Serialization;

namespace Negotiation.Models
{
    [Serializable]
    public class NegotiationOption : IXmlExtractable
    {
        public NegotiationOption()
        {
        }

        public string Name { get; set; }

        public virtual void Extract(System.Xml.XmlNode node)
        {
            Name = node.Attributes["value"].Value;
        }
    }
}