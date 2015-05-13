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
        public AiConfig AiConfig { get; set; }
        public NegotiationStatus Status { get; set; }
        public List<NegotiationActionModel> Actions { get; set; }
        public String StrategyName { get; set; }

        public INegotiationChannel HumanChannel { get; set; }
        public INegotiationChannel AiChannel { get; set; }

        public bool NegotiationActive { get; private set; }

        public TimeSpan UpdateInterval { get; set; }
        public DateTime NegotiationEndTime { get; private set; }

        public NegotiationEngine(
            String negotiationId,
            NegotiationDomain domain,
            PreNegotiationQuestionnaireViewModel userData,
            SideConfig humanConfig,
            AiConfig aiConfig)
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

            String strategyName;
            IAgentStrategy strat = NegotiationManager.GetStrategy(aiConfig.StrategyId, out strategyName);
            strat.Initialize(domain, aiConfig, humanConfig.Side, AiChannel);

            TimeSpan defaultInterval = TimeSpan.FromSeconds(1);
            UpdateInterval = defaultInterval < strat.MinimumUpdateTime ? defaultInterval : strat.MinimumUpdateTime;

            StrategyName = strategyName;

            RegisterChannel(HumanChannel);
            RegisterChannel(AiChannel);
        }

        private NegotiationOffer EmptyOffer()
        {
            return new NegotiationOffer()
            {
                Offers = Domain.Options.Topics.Keys.ToDictionary(x => x, x => "---")
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

            Status.HumanStatus.Score = Domain.CalculateOptoutScore(HumanConfig, Status.RemainingTime);
            Status.AiStatus.Score = Domain.CalculateOptoutScore(AiConfig, Status.RemainingTime);

            this.Actions.Add(new NegotiationActionModel()
            {
                Role = (((INegotiationChannel)sender) == AiChannel) ? AiConfig.Side : HumanConfig.Side,
                Type = NegotiationActionType.Optout,
            });

            NegotiationManager.SaveOptOut(this, sender == AiChannel ? AiConfig : HumanConfig);
        }

        INegotiationChannel GetOtherChannel(INegotiationChannel channel)
        {
            return (channel == AiChannel ? HumanChannel : AiChannel);
        }

        void channel_OfferAcceptedEvent(object sender, EventArgs e)
        {
            NegotiationOffer offer;
            if (sender == AiChannel)
            {
                Status.State = NegotiationState.EndHumanOfferAccepted;
                offer = Status.HumanStatus.Offer;
            }
            else
            {
                Status.State = NegotiationState.EndAiOfferAccepted;
                offer = Status.AiStatus.Offer;
            }

            Status.HumanStatus.Score = CalculateAcceptScore(HumanConfig, offer);
            Status.AiStatus.Score = CalculateAcceptScore(AiConfig, offer);

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

        private int CalculateAcceptScore(SideConfig config, NegotiationOffer offer)
        {
            var variant = Domain.OwnerVariantDict[config.Side][config.Variant];
            return offer.Offers.Sum(x=>variant.Topics[x.Key].Options[x.Value].Score) + variant.TimeEffect * Domain.RoundsPassed(Status.RemainingTime);
        }

        void channel_NewOfferEvent(object sender, OfferEventArgs e)
        {
            JavaScriptSerializer js = new JavaScriptSerializer();
            string offer = js.Serialize(e.Offer);

            SideConfig side;

            if (((INegotiationChannel)sender) == AiChannel) 
            {
                side = AiConfig;
                Status.AiStatus.Offer = e.Offer;
            }
            else
	        {
                side = HumanConfig;
                Status.HumanStatus.Offer = e.Offer;
	        }

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
                Interval = UpdateInterval.TotalMilliseconds
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
            NegotiationEndTime = DateTime.Now;

            HumanChannel.NegotiationEnded();
            AiChannel.NegotiationEnded();

            UnregisterChannel(HumanChannel);
            UnregisterChannel(AiChannel);
        }

        void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Status.RemainingTime -= TimeSpan.FromSeconds(1);

            if (Status.RemainingTime.TotalSeconds == 0)
            {
                Status.State = NegotiationState.EndTimeout;

                Status.AiStatus.Score = Domain.CalculateTimeoutScore(AiConfig);
                Status.HumanStatus.Score = Domain.CalculateTimeoutScore(HumanConfig);

                EndNegotiation();
                NegotiationManager.SaveTimeout(this);
                return;
            }

            ThreadPool.QueueUserWorkItem(x =>
            {
                AiChannel.TimePassed(Status.RemainingTime);
            });
        }

        
    }
}