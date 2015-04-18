using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;
using System.Xml.Serialization;

namespace Negotiation.Models
{
    public class SideConfig
    {
        public String Side { get; set; }
        public String Variant { get; set; }
    }

    public class NegotiationDomainManager
    {
        public static int TotalRounds;

        static NegotiationDomainManager()
        {
            TotalRounds = 15;
            LoadDbData();
        }

        static public GameDomain GameDomain { get; set; }
        public static NegotiationDomain Domain { get; private set; }

        static void LoadDbData()
        {
            NegotiationContainer cont = new NegotiationContainer();

            GameDomain = cont.GameDomainConfigSet.First().GameDomain;

            Domain = new NegotiationDomain();
            XmlDocument doc = new XmlDocument();
            
            doc.LoadXml(GameDomain.DomainXML);
            Domain.Extract(doc.ChildNodes[0]);
        }

        static void SetActiveDomain(String domainName)
        {
            NegotiationContainer cont = new NegotiationContainer();

            GameDomain domain = cont.GameDomainSet.First(x => x.Name == domainName);
            cont.GameDomainConfigSet.First().GameDomain = domain;
            cont.SaveChanges();
        }

        public static SideConfig GetHumanConfig()
        {
            return new SideConfig { Side = Domain.OwnerVariantDict.Keys.First(), Variant = Domain.OwnerVariantDict.Values.First().Keys.First() };
        }

        public static SideConfig GetAiConfig()
        {
            return new SideConfig { Side = Domain.OwnerVariantDict.Keys.ElementAt(1), Variant = Domain.OwnerVariantDict.Values.ElementAt(1).Keys.First() };
        }
    }
}