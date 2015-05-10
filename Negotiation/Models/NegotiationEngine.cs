using Negotiation.App_Start;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Timers;
using System.Web;
using System.Web.Script.Serialization;

namespace Negotiation.Models
{
    public class NegotiationEngine
    {
        private System.Timers.Timer _timer;

        public String NegotiationId { get; set; }
        public NegotiationDomain Domain { get; private set; }
        public SideConfig HumanConfig { get; set; }
        public SideConfig AiConfig { get; set; }
        public NegotiationStatus Status { get; set; }
        public List<NegotiationActionModel> Actions { get; set; }

        public INegotiationChannel HumanChannel { get; set; }
        public INegotiationChannel AiChannel { get; set; }

        public bool NegotiationActive { get; private set; }

        public NegotiationEngine(
            String negotiationId,
            NegotiationDomain domain, 
            SideConfig humanConfig,
            SideConfig aiConfig)
        {
            NegotiationId = negotiationId;
            Domain = domain;
            HumanChannel = new LocalNegotiationChannel();
            AiChannel = new LocalNegotiationChannel();
            HumanConfig = humanConfig;
            AiConfig = aiConfig;

            Status = new NegotiationStatus()
            {
                RemainingTime = TimeSpan.FromSeconds(domain.NumberOfRounds * domain.RoundLength.TotalSeconds),
                HumanStatus = new SideStatus() { Offer = EmptyOffer() },
                AiStatus = new SideStatus() { Offer = EmptyOffer() },
            };

            Actions = new List<NegotiationActionModel>();

            RegisterChannel(HumanChannel);
            RegisterChannel(AiChannel);
        }

        private NegotiationOffer EmptyOffer()
        {
            return new NegotiationOffer()
            {
                Offers = Domain.Options.Topics.Keys.ToDictionary(x => x, x=>"---")
            };
        }

        public NegotiationEndModel GetEndModel()
        {
            return new NegotiationEndModel()
            {
                Score = Status.HumanStatus.Score,
                Message = Status.State.GetEnumDescription()
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

            Status.HumanStatus.Score = CalculateOptoutScore(HumanConfig);
            Status.AiStatus.Score = CalculateOptoutScore(AiConfig);

            this.Actions.Add(new NegotiationActionModel()
            {
                Role = (((INegotiationChannel)sender) == AiChannel) ? AiConfig.Side : HumanConfig.Side,
                Type = NegotiationActionType.Optout,
            });

            NegotiationManager.SaveOptOut(this, sender == AiChannel ? AiConfig : HumanConfig);
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

            this.Actions.Add(new NegotiationActionModel()
            {
                Role = (((INegotiationChannel)sender) == AiChannel) ? AiConfig.Side : HumanConfig.Side,
                Type = NegotiationActionType.AcceptOffer
            });

            EndNegotiation();
        }

        void channel_NewOfferEvent(object sender, OfferEventArgs e)
        {
            JavaScriptSerializer js = new JavaScriptSerializer();
            string offer = js.Serialize(e.Offer);

            SideConfig side = (((INegotiationChannel)sender) == AiChannel) ? AiConfig : HumanConfig;

            this.Actions.Add(new NegotiationActionModel()
                {
                    Role = side.Side,
                    Type = NegotiationActionType.MakeOffer,
                    Value = offer,
                    RemainingTime = Status.RemainingTime
                });

            NegotiationManager.SaveNewOffer(this, side, offer);

            ThreadPool.QueueUserWorkItem(x =>
            {
                GetOtherChannel((INegotiationChannel)sender).OpponentOfferReceived(e.Offer);
            });
        }

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
                NegotiationManager.SaveTimeout(this);
            }
        }
    }
}