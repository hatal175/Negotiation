using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;

namespace Negotiation.Models
{
    public class NegotiationTopic<T> : IXmlExtractable where T : NegotiationOption, IXmlExtractable, new()
    {
        public String Name { get; set; }
        public Dictionary<String,T> Options { get; set; }

        public virtual void Extract(System.Xml.XmlNode node)
        {
            Name = node.Attributes["name"].Value;
            Options = node.ChildNodes.Cast<XmlNode>().ToDictionary(x=>x.Attributes["value"].Value, 
                x=>{
                    T temp = new T();
                    temp.Extract(x);
                    return temp;
            });
        }
    }
}