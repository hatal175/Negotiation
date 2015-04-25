using Negotiation.App_Start;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using System.Web;

namespace Negotiation.Models
{
    public class NegotiationEngine
    {
        private Timer _timer;

        public NegotiationEngine(NegotiationDomain domain, 
            INegotiationChannel humanChannel,
            INegotiationChannel aiChannel,
            SideConfig humanConfig,
            SideConfig aiConfig)
        {
            Domain = domain;
            HumanChannel = humanChannel;
            AiChannel = aiChannel;
            HumanConfig = humanConfig;
            AiConfig = aiConfig;

            Status = new NegotiationStatus()
            {
                RemainingTime = TimeSpan.FromSeconds(domain.NumberOfRounds * domain.RoundLength.TotalSeconds),
                HumanOffer = EmptyOffer(),
                AiOffer = EmptyOffer()
            };

            Actions = new List<NegotiationActionModel>();

            RegisterChannel(humanChannel);
            RegisterChannel(aiChannel);
        }

        private NegotiationOffer EmptyOffer()
        {
            return new NegotiationOffer()
            {
                Offers = Domain.Options.Topics.Keys.ToDictionary(x => x, x => new NegotiationTopicOffer() {TopicName=x,TopicValue="---" })
            };
        }

        private void RegisterChannel(INegotiationChannel channel)
        {
            channel.NewOfferEvent += channel_NewOfferEvent;
            channel.OfferAcceptedEvent += channel_OfferAcceptedEvent;
            channel.OptOutEvent += channel_OptOutEvent;
        }

        private void UnregisterChannel(INegotiationChannel channel)
        {
            channel.NewOfferEvent -= channel_NewOfferEvent;
            channel.OfferAcceptedEvent -= channel_OfferAcceptedEvent;
            channel.OptOutEvent -= channel_OptOutEvent;
        }

        void channel_OptOutEvent(object sender, EventArgs e)
        {
            EndNegotiation();

            Status.HumanOffer.Score = CalculateOptoutScore(HumanConfig);
            Status.AiOffer.Score = CalculateOptoutScore(AiConfig);
        }

        int CalculateOptoutScore(SideConfig config)
        {
            var variant = Domain.OwnerVariantDict[config.Side][config.Variant];
            return variant.Optout + variant.TimeEffect * Domain.RoundsPassed(Status.RemainingTime);
        }

        void channel_OfferAcceptedEvent(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        void channel_NewOfferEvent(object sender, NewOfferEventArgs e)
        {
            throw new NotImplementedException();
        }

        public NegotiationDomain Domain { get; private set; }
        public SideConfig HumanConfig { get; set; }
        public SideConfig AiConfig { get; set; }
        public NegotiationStatus Status { get; set; }
        public List<NegotiationActionModel> Actions {get; set;}

        public INegotiationChannel HumanChannel { get; set; }
        public INegotiationChannel AiChannel { get; set; }

        public bool NegotiationActive { get; private set; }

        public void BeginNegotiation()
        {
            _timer = new Timer()
            {
                AutoReset = true,
                Interval = 1000
            };

            _timer.Elapsed += _timer_Elapsed;

            _timer.Enabled = true;

            NegotiationActive = true;
        }

        private void EndNegotiation()
        {
            NegotiationActive = false;
            _timer.Enabled = false;

            UnregisterChannel(HumanChannel);
            UnregisterChannel(AiChannel);
        }

        void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            this.Status.RemainingTime -= TimeSpan.FromSeconds(1);
        }
    }
}