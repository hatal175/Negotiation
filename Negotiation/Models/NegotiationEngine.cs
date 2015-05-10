﻿using Negotiation.App_Start;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Timers;
using System.Web;

namespace Negotiation.Models
{
    public class NegotiationEngine
    {
        private System.Timers.Timer _timer;

        public NegotiationEngine(NegotiationDomain domain, 
            SideConfig humanConfig,
            SideConfig aiConfig)
        {
            Domain = domain;
            HumanChannel = new LocalNegotiationChannel();
            AiChannel = new LocalNegotiationChannel();
            HumanConfig = humanConfig;
            AiConfig = aiConfig;

            Status = new NegotiationStatus()
            {
                RemainingTime = TimeSpan.FromSeconds(domain.NumberOfRounds * domain.RoundLength.TotalSeconds),
                HumanOffer = EmptyOffer(),
                AiOffer = EmptyOffer()
            };

            Actions = new List<NegotiationActionModel>();

            RegisterChannel(HumanChannel);
            RegisterChannel(AiChannel);
        }

        private NegotiationOffer EmptyOffer()
        {
            return new NegotiationOffer()
            {
                Offers = Domain.Options.Topics.Keys.ToDictionary(x => x, x => new NegotiationTopicOffer() {TopicName=x,TopicValue="---" })
            };
        }

        private void RegisterChannel(INegotiationServer channel)
        {
            channel.NewOfferEvent += channel_NewOfferEvent;
            channel.OfferAcceptedEvent += channel_OfferAcceptedEvent;
            channel.OptOutEvent += channel_OptOutEvent;
        }

        private void UnregisterChannel(INegotiationServer channel)
        {
            channel.NewOfferEvent -= channel_NewOfferEvent;
            channel.OfferAcceptedEvent -= channel_OfferAcceptedEvent;
            channel.OptOutEvent -= channel_OptOutEvent;
        }

        void channel_OptOutEvent(object sender, EventArgs e)
        {
            Status.State = sender == AiChannel ? NegotiationState.EndAiOptOut : NegotiationState.EndHumanOptOut;

            EndNegotiation();

            Status.HumanOffer.Score = CalculateOptoutScore(HumanConfig);
            Status.AiOffer.Score = CalculateOptoutScore(AiConfig);
        }

        int CalculateOptoutScore(SideConfig config)
        {
            var variant = Domain.OwnerVariantDict[config.Side][config.Variant];
            return variant.Optout + variant.TimeEffect * Domain.RoundsPassed(Status.RemainingTime);
        }

        INegotiationChannel GetOtherChannel(INegotiationChannel channel)
        {
            return (channel == AiChannel ? HumanChannel : AiChannel);
        }

        void channel_OfferAcceptedEvent(object sender, EventArgs e)
        {
            Status.State = sender == AiChannel ? NegotiationState.EndHumanOfferAccepted : NegotiationState.EndAiOfferAccepted;

            ThreadPool.QueueUserWorkItem(x =>
            {
                GetOtherChannel((INegotiationChannel)sender).OpponentAcceptedOffer();
            });

            EndNegotiation();
        }

        void channel_NewOfferEvent(object sender, OfferEventArgs e)
        {
            ThreadPool.QueueUserWorkItem(x =>
            {
                GetOtherChannel((INegotiationChannel)sender).OpponentOfferReceived(e.Offer);
            });
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
            _timer = new System.Timers.Timer()
            {
                AutoReset = true,
                Interval = 1000
            };

            _timer.Elapsed += _timer_Elapsed;

            _timer.Enabled = true;

            NegotiationActive = true;

            HumanChannel.NegotiationStarted();
            AiChannel.NegotiationStarted();
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
            Status.RemainingTime -= TimeSpan.FromSeconds(1);

            if (Status.RemainingTime.TotalSeconds == 0)
            {
                Status.State = NegotiationState.EndTimeout;
                EndNegotiation();
            }
        }
    }
}