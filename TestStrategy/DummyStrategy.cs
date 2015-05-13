using Negotiation.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestStrategy
{
    public class DummyStrategy : IAgentStrategy
    {
        public INegotiationClient Client { get; set; }

        public TimeSpan MinimumUpdateTime
        {
            get { return TimeSpan.FromMilliseconds(1000); }
        }

        public void Initialize(NegotiationDomain domain, SideConfig strategyConfig, String opponentSide, INegotiationClient client)
        {
            Client = client;

            client.OfferReceivedEvent += client_OfferReceivedEvent;
            client.NegotiationEndedEvent += client_NegotiationEndedEvent;
        }

        void client_NegotiationEndedEvent(object sender, EventArgs e)
        {
            Client.OfferReceivedEvent -= client_OfferReceivedEvent;
            Client.NegotiationEndedEvent -= client_NegotiationEndedEvent;
        }

        void client_OfferReceivedEvent(object sender, OfferEventArgs e)
        {
            Client.SendOffer(e.Offer);
        }

       
    }
}
