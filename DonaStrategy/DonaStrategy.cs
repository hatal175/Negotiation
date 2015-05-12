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
        private NegotiationOffer m_lastOffer;

        public TimeSpan MinimumUpdateTime
        {
            get { return TimeSpan.FromSeconds(1); }
        }

        public NegotiationDomain Domain { get; set; }
        public SideConfig Config { get; set; }

        public INegotiationClient Client { get; set; }

        public void Initialize(NegotiationDomain domain, SideConfig config, INegotiationClient client)
        {
            Domain = domain;
            Config = config;
            Client = client;

            Client.NegotiationStartedEvent += Client_NegotiationStartedEvent;
            Client.NegotiationEndedEvent += client_NegotiationEndedEvent;
            Client.OfferReceivedEvent += Client_OfferReceivedEvent;
            Client.TimePassedEvent += Client_TimePassedEvent;
        }

        void Client_TimePassedEvent(object sender, TimePassedEventArgs e)
        {
            int roundsPassed = Domain.RoundsPassed(e.RemainingTime);
            if (roundsPassed == m_roundsPassed)
                return;

            RecalculateOffer();
        }

        private void RecalculateOffer()
        {
            throw new NotImplementedException();
        }

        void Client_OfferReceivedEvent(object sender, OfferEventArgs e)
        {
            throw new NotImplementedException();
        }

        void Client_NegotiationStartedEvent(object sender, EventArgs e)
        {
            RecalculateOffer();
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
