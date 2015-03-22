using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Negotiation.Models
{
    public class LocalNegotiationChannel : INegotiationChannel
    {
        public void SendOffer(NegotiationOffer offer)
        {
            NewOfferEvent(this, new NewOfferEventArgs(offer));
        }

        public void AcceptOffer()
        {
            OfferAcceptedEvent(this, EventArgs.Empty);
        }

        public event EventHandler<NewOfferEventArgs> NewOfferEvent;

        public event EventHandler OfferAcceptedEvent;
    }
}