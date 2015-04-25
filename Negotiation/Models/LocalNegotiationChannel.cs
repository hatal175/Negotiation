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
            if (NewOfferEvent != null)
            {
                NewOfferEvent(this, new NewOfferEventArgs(offer));
            }
        }

        public void AcceptOffer()
        {
            if (OfferAcceptedEvent != null)
            {
                OfferAcceptedEvent(this, EventArgs.Empty);
            }
        }

        public event EventHandler<NewOfferEventArgs> NewOfferEvent;

        public event EventHandler OfferAcceptedEvent;

        public void OptOut()
        {
            if (OptOutEvent != null)
            {
                OptOutEvent(this, EventArgs.Empty);
            }
        }


        public event EventHandler OptOutEvent;


        public event EventHandler NegotiationStartedEvent;


        public void NegotiationStarted()
        {
            if (NegotiationStartedEvent != null)
            {
                NegotiationStartedEvent(this, EventArgs.Empty);
            }
        }
    }
}