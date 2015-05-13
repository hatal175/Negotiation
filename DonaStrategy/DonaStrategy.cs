using Negotiation.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DonaStrategy
{
    public class DonaStrategy : IAgentStrategy
    {
        private int m_roundsPassed = 0;
        private NegotiationOffer m_bestOffer;
        private Dictionary<string, OfferUtility> m_bestCombinedUtilityOffers;
        private NegotiationOffer m_currentOffer;

        private Dictionary<NegotiationOffer, OfferUtility> m_allOptions;

        public TimeSpan MinimumUpdateTime
        {
            get { return TimeSpan.FromSeconds(1); }
        }

        public NegotiationDomain Domain { get; set; }
        public SideConfig StrategyConfig { get; set; }
        public String OpponentSide { get; set; }

        public INegotiationClient Client { get; set; }

        public void Initialize(NegotiationDomain domain, SideConfig strategyConfig, String opponentSide, INegotiationClient client)
        {
            Domain = domain;
            StrategyConfig = strategyConfig;
            Client = client;
            OpponentSide = opponentSide;

            Client.NegotiationStartedEvent += Client_NegotiationStartedEvent;
            Client.NegotiationEndedEvent += client_NegotiationEndedEvent;
            Client.OfferReceivedEvent += Client_OfferReceivedEvent;
            Client.TimePassedEvent += Client_TimePassedEvent;

            CalculateOffers();
        }

        double CalculateUtility(IEnumerable<KeyValuePair<String,String>> offer, SideConfig config)
        {
            var sideDesc = Domain.GetSideDescription(config);
            return offer.Aggregate(0.0, (sum, x) => sum + sideDesc.Topics[x.Key].Options[x.Value].Score);
        }

        private void CalculateOffers()
        {
            m_allOptions = Domain.Options.Topics.Select(v => v.Value.Options.Values.Select(option => new KeyValuePair<String, String>(v.Key, option.Name))).CartesianProduct().Select(offer => new OfferUtility()
            {
                Offer = new NegotiationOffer(offer),
                Utility = CalculateUtility(offer, StrategyConfig),
                UtilityDataDict = Domain.OwnerVariantDict[OpponentSide].Keys.ToDictionary(x=>x,variantName => new UtilityData()
                {
                    OpponentUtility = CalculateUtility(offer, new SideConfig(OpponentSide, variantName))
                })
            }).ToDictionary(x=>x.Offer);

            double maxUtility = m_allOptions.Values.Max(x => x.Utility);
            Dictionary<string, double> maxOpponentDict = Domain.OwnerVariantDict[OpponentSide].Keys.ToDictionary(x => x, variantName => m_allOptions.Values.Select(x => x.UtilityDataDict[variantName].OpponentUtility).Max());

            foreach (var option in m_allOptions.Values)
            {
                option.Utility /= maxUtility;
                
                foreach(var key in option.UtilityDataDict.Keys)
                {
                    var utilityData = option.UtilityDataDict[key];
                    utilityData.OpponentUtility /= maxOpponentDict[key];
                    utilityData.CombinedUtility = option.Utility + utilityData.OpponentUtility;
                }
            }

            Dictionary<string, double> maxCombinedDict = Domain.OwnerVariantDict[OpponentSide].Keys.ToDictionary(x => x, variantName => m_allOptions.Values.Select(x => x.UtilityDataDict[variantName].CombinedUtility).Max());

            foreach (var option in m_allOptions.Values)
            {
                foreach (var key in option.UtilityDataDict.Keys)
                {
                    var utilityData = option.UtilityDataDict[key];
                    utilityData.CombinedUtility /= maxCombinedDict[key];
                    utilityData.FScore = 2 * utilityData.CombinedUtility * option.Utility / (utilityData.CombinedUtility + option.Utility);
                }
            }

            m_bestOffer = m_allOptions.Values.ArgMax(x => x.Utility).Offer;

            m_bestCombinedUtilityOffers = Domain.OwnerVariantDict[OpponentSide].Keys.ToDictionary(k => k, variantName => m_allOptions.Values.OrderByDescending(x => x.UtilityDataDict[variantName].CombinedUtility).First(x => x.Utility > x.UtilityDataDict[variantName].CombinedUtility));
        }

        void Client_TimePassedEvent(object sender, TimePassedEventArgs e)
        {
            int roundsPassed = Domain.RoundsPassed(e.RemainingTime);
            if (roundsPassed == m_roundsPassed)
                return;

            if (roundsPassed < m_bestCombinedUtilityOffers.Count)
            {
                SendOffer(m_bestCombinedUtilityOffers.Values.ElementAt(roundsPassed - 1).Offer);
            }

            m_roundsPassed = roundsPassed;
        }

        void Client_OfferReceivedEvent(object sender, OfferEventArgs e)
        {
            if (m_currentOffer == null)
            {
                return;
            }

            if (m_allOptions[e.Offer].Utility > m_allOptions[m_currentOffer].Utility)
            {
                Client.AcceptOffer();
            }
        }

        void Client_NegotiationStartedEvent(object sender, EventArgs e)
        {
            SendOffer(m_bestOffer);
        }

        void SendOffer(NegotiationOffer offer)
        {
            m_currentOffer = offer;
            Client.SendOffer(m_bestOffer);
        }

        void client_NegotiationEndedEvent(object sender, EventArgs e)
        {
            Client.NegotiationStartedEvent -= Client_NegotiationStartedEvent;
            Client.NegotiationEndedEvent -= client_NegotiationEndedEvent;
            Client.OfferReceivedEvent -= Client_OfferReceivedEvent;
            Client.TimePassedEvent -= Client_TimePassedEvent;
        }
    }
}
