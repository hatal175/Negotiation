using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;
using System.Xml.Serialization;
using System.Collections.Concurrent;

namespace Negotiation.Models
{
    public class SideConfig
    {
        public String Side { get; set; }
        public String Variant { get; set; }
    }

    public class NegotiationManager
    {
        public static ConcurrentDictionary<String, NegotiationEngine> OnGoingNegotiations { get; private set; }
        public static ConcurrentDictionary<String, NegotiationTutorialModel> TutorialModels { get; private set; }

        public static int TotalRounds;
        public static TimeSpan RoundLength;

        static NegotiationManager()
        {
            TotalRounds = 15;
            RoundLength = new TimeSpan(0, 2, 0);
            OnGoingNegotiations = new ConcurrentDictionary<string, NegotiationEngine>();
            TutorialModels = new ConcurrentDictionary<string, NegotiationTutorialModel>();
            LoadDbData();
        }

        static public GameDomain GameDomain { get; set; }
        public static NegotiationDomain Domain { get; private set; }

        static void LoadDbData()
        {
            NegotiationContainer cont = new NegotiationContainer();

            GameDomain = cont.GameDomainConfigSet.First().GameDomain;

            Domain = new NegotiationDomain() {RoundLength = RoundLength, NumberOfRounds = TotalRounds };
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