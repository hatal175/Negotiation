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
        private Dictionary<string, OfferUtility> m_bestFScoreUtilityOffers;
        private NegotiationOffer m_currentOffer;
        private NegotiationOffer m_opponentOffer;

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

        double CalculateUtility(IEnumerable<KeyValuePair<String, String>> offer, SideConfig config)
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
                UtilityDataDict = Domain.OwnerVariantDict[OpponentSide].Keys.ToDictionary(x => x, variantName => new UtilityData()
                {
                    OpponentUtility = CalculateUtility(offer, new SideConfig(OpponentSide, variantName))
                })
            }).ToDictionary(x => x.Offer);

            double maxUtility = m_allOptions.Values.Max(x => x.Utility);
            Dictionary<string, double> maxOpponentDict = Domain.OwnerVariantDict[OpponentSide].Keys.ToDictionary(x => x, variantName => m_allOptions.Values.Select(x => x.UtilityDataDict[variantName].OpponentUtility).Max());

            foreach (var option in m_allOptions.Values)
            {
                option.Utility /= maxUtility;

                foreach (var key in option.UtilityDataDict.Keys)
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

            m_bestCombinedUtilityOffers = Domain.OwnerVariantDict[OpponentSide].Keys.ToDictionary(k => k, variantName => m_allOptions.Values.OrderByDescending(x => x.UtilityDataDict[variantName].CombinedUtility).First(x => x.Utility > x.UtilityDataDict[variantName].OpponentUtility));
            m_bestFScoreUtilityOffers = Domain.OwnerVariantDict[OpponentSide].Keys.ToDictionary(k => k, variantName => m_allOptions.Values.OrderByDescending(x => x.UtilityDataDict[variantName].FScore).First(x => x.Utility > x.UtilityDataDict[variantName].OpponentUtility));
        }

        void Client_TimePassedEvent(object sender, TimePassedEventArgs e)
        {
            int roundsPassed = Domain.RoundsPassed(e.RemainingTime);
            if (roundsPassed == m_roundsPassed && roundsPassed != Domain.NumberOfRounds - 1)
            {
                return;
            }

            m_roundsPassed = roundsPassed;
               
            if (roundsPassed == 0) 
            {
                return;
            }
            else if (roundsPassed < Domain.NumberOfRounds - 1)
            {
                CompareOffer(m_opponentOffer);
                SendOffer(m_bestFScoreUtilityOffers.Values.ElementAt(roundsPassed % m_bestFScoreUtilityOffers.Count).Offer);
            }
            else 
            {
                CompareOffer(m_opponentOffer);
                int roundPart = (int)(Domain.RoundLength.TotalSeconds / Domain.OwnerVariantDict[this.OpponentSide].Count);
                int offerIndex = Math.Min(m_bestCombinedUtilityOffers.Count - 1, ((int)e.RemainingTime.TotalSeconds) / roundPart);
                NegotiationOffer offer = m_bestCombinedUtilityOffers.Values.ElementAt(offerIndex).Offer;
                SendOffer(offer);
            }
        }

        void Client_OfferReceivedEvent(object sender, OfferEventArgs e)
        {
            m_opponentOffer = e.Offer;

            if (m_currentOffer == null)
            {
                return;
            }

            CompareOffer(e.Offer);
        }

        private void CompareOffer(NegotiationOffer offer)
        {
            if (offer == null)
            {
                return;
            }

            if (m_roundsPassed == 0)
            {
                if (offer == m_currentOffer)
                {
                    Client.AcceptOffer();
                }
            }
            else if (m_roundsPassed < Domain.NumberOfRounds - 1)
            {
                if (m_allOptions[offer].Utility >= m_bestFScoreUtilityOffers.Values.Min(x => x.Utility))
                {
                    Client.AcceptOffer();
                }
            }
            else
            {
                if (m_allOptions[offer].Utility >= m_bestCombinedUtilityOffers.Values.Min(x => x.Utility))
                {
                    Client.AcceptOffer();
                }
            }
        }

        void Client_NegotiationStartedEvent(object sender, EventArgs e)
        {
            SendOffer(m_bestOffer);
        }

        void SendOffer(NegotiationOffer offer)
        {
            if (m_currentOffer != offer)
            {
                m_currentOffer = offer;
                Client.SendOffer(m_currentOffer);
            }
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
