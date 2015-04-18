using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Negotiation.Models
{
    public class XmlData
    {
        public String Name { get; set; }
        public String XmlString { get; set; }
    }

    public class NegotiationConfigurationModel
    {
        public List<String> Domains { get; set; }
        public String ActiveDomain { get; set; }

        public XmlData NewDomain { get; set; }
        public List<XmlData> Variants { get; set; }

    }
}