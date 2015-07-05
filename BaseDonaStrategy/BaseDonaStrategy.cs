using Negotiation.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DonaStrategy
{
    public abstract class BaseDonaStrategy : IAgentStrategy
    {
        protected int RoundsPassed {get; set;}
        
        protected Dictionary<string, OfferUtility> BestCombinedUtilityOffers;
        protected Dictionary<string, OfferUtility> BestFScoreUtilityOffers;

        protected NegotiationOffer CurrentOffer;
        protected NegotiationOffer OpponentOffer; 
        protected Dictionary<NegotiationOffer, OfferUtility> AllOptions { get; set; }
        protected NegotiationOffer BestOffer { get; set; }

        protected Boolean HasAcceptedLastOffer { get; set; }

        public virtual TimeSpan MinimumUpdateTime
        {
            get { return TimeSpan.FromSeconds(1); }
        }

        public NegotiationDomain Domain { get; set; }
        public SideConfig StrategyConfig { get; set; }
        public String OpponentSide { get; set; }

        public INegotiationClient Client { get; set; }

        public virtual void Initialize(NegotiationDomain domain, SideConfig strategyConfig, String opponentSide, INegotiationClient client)
        {
            Domain = domain;
            StrategyConfig = strategyConfig;
            Client = client;
            OpponentSide = opponentSide;

            Client.NegotiationStartedEvent += OnNegotiationStartedEvent;
            Client.NegotiationEndedEvent += OnNegotiationEndedEvent;
            Client.OfferReceivedEvent += OnOfferReceivedEventInner;
            Client.TimePassedEvent += OnTimePassedEventInner;
            Client.OpponentAcceptedOfferEvent += OnOpponentAcceptedOfferEvent;

            CalculateOffers();
        }

        protected double CalculateUtility(IEnumerable<KeyValuePair<String, String>> offer, SideConfig config)
        {
            var sideDesc = Domain.GetSideDescription(config);
            return offer.Aggregate(0.0, (sum, x) => sum + sideDesc.Topics[x.Key].Options[x.Value].Score);
        }

        private void CalculateOffers()
        {
            AllOptions = Domain.Options.Topics.Select(v => v.Value.Options.Values.Select(option => new KeyValuePair<String, String>(v.Key, option.Name))).CartesianProduct().Select(offer => new OfferUtility()
            {
                Offer = new NegotiationOffer(offer),
                Utility = CalculateUtility(offer, StrategyConfig),
                UtilityDataDict = Domain.OwnerVariantDict[OpponentSide].Keys.ToDictionary(x => x, variantName => new UtilityData()
                {
                    OpponentUtility = CalculateUtility(offer, new SideConfig(OpponentSide, variantName))
                })
            }).ToDictionary(x => x.Offer);

            double maxUtility = AllOptions.Values.Max(x => x.Utility);
            Dictionary<string, double> maxOpponentDict = Domain.OwnerVariantDict[OpponentSide].Keys.ToDictionary(x => x, variantName => AllOptions.Values.Select(x => x.UtilityDataDict[variantName].OpponentUtility).Max());

            foreach (var option in AllOptions.Values)
            {
                option.Utility /= maxUtility;

                foreach (var key in option.UtilityDataDict.Keys)
                {
                    var utilityData = option.UtilityDataDict[key];
                    utilityData.OpponentUtility /= maxOpponentDict[key];
                    utilityData.CombinedUtility = option.Utility + utilityData.OpponentUtility;
                }
            }

            Dictionary<string, double> maxCombinedDict = Domain.OwnerVariantDict[OpponentSide].Keys.ToDictionary(x => x, variantName => AllOptions.Values.Select(x => x.UtilityDataDict[variantName].CombinedUtility).Max());

            foreach (var option in AllOptions.Values)
            {
                foreach (var key in option.UtilityDataDict.Keys)
                {
                    var utilityData = option.UtilityDataDict[key];
                    utilityData.CombinedUtility /= maxCombinedDict[key];
                    utilityData.FScore = 2 * utilityData.CombinedUtility * option.Utility / (utilityData.CombinedUtility + option.Utility);
                }
            }

            BestOffer = AllOptions.Values.ArgMax(x => x.Utility).Offer;

            BestCombinedUtilityOffers = Domain.OwnerVariantDict[OpponentSide].Keys.ToDictionary(k => k, variantName => AllOptions.Values.OrderByDescending(x => x.UtilityDataDict[variantName].CombinedUtility).First(x => x.Utility > x.UtilityDataDict[variantName].OpponentUtility));
            BestFScoreUtilityOffers = Domain.OwnerVariantDict[OpponentSide].Keys.ToDictionary(k => k, variantName => AllOptions.Values.OrderByDescending(x => x.UtilityDataDict[variantName].FScore).First(x => x.Utility > x.UtilityDataDict[variantName].OpponentUtility));
        }

        protected abstract void OnTimePassedEvent(object sender, ExtendedTimePassedEventArgs e);

        private void OnTimePassedEventInner(object sender, TimePassedEventArgs e)
        {
            int roundsPassed = Domain.RoundsPassed(e.RemainingTime);
            Boolean hasRoundPassed = (roundsPassed != RoundsPassed);

            RoundsPassed = roundsPassed;

            OnTimePassedEvent(sender, new ExtendedTimePassedEventArgs(e.RemainingTime, hasRoundPassed));
        }

        void OnOpponentAcceptedOfferEvent(object sender, EventArgs e)
        {
            Client.SignAgreement();
        }

        protected void OnOfferReceivedEventInner(object sender, OfferEventArgs e)
        {
            HasAcceptedLastOffer = false;
            OnOfferReceivedEvent(sender, e);
        }

        protected abstract void OnOfferReceivedEvent(object sender, OfferEventArgs e);

        protected virtual void OnNegotiationStartedEvent(object sender, EventArgs e)
        {
            SendOffer(BestOffer);
        }

        protected void SendOffer(NegotiationOffer offer, Boolean sendIdentical = true)
        {
            if (offer != null)
            {
                if (sendIdentical || !CurrentOffer.Equals(offer))
                {
                    CurrentOffer = offer;
                    Client.SendOffer(CurrentOffer);
                }
            }
        }

        protected void AcceptOffer()
        {
            if (HasAcceptedLastOffer) return;

            HasAcceptedLastOffer = true;
            Client.AcceptOffer();
            Client.SignAgreement();
        }

        protected virtual void OnNegotiationEndedEvent(object sender, EventArgs e)
        {
            Client.NegotiationStartedEvent -= OnNegotiationStartedEvent;
            Client.NegotiationEndedEvent -= OnNegotiationEndedEvent;
            Client.OfferReceivedEvent -= OnOfferReceivedEventInner;
            Client.TimePassedEvent -= OnTimePassedEventInner;
            Client.OpponentAcceptedOfferEvent -= OnOpponentAcceptedOfferEvent;
        }
    }
}
