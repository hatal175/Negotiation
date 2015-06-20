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
                NewOfferEvent(this, new OfferEventArgs(offer));
            }
        }

        public void AcceptOffer()
        {
            if (OfferAcceptedEvent != null)
            {
                OfferAcceptedEvent(this, EventArgs.Empty);
            }
        }

        public event EventHandler<OfferEventArgs> NewOfferEvent;

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

        public event EventHandler<OfferEventArgs> OfferReceivedEvent;

        public void OpponentOfferReceived(NegotiationOffer offer)
        {
            if (OfferReceivedEvent != null)
            {
                OfferReceivedEvent(this, new OfferEventArgs(offer));
            }
        }

        public event EventHandler OpponentAcceptedOfferEvent;

        public void OpponentAcceptedOffer()
        {
            if (OpponentAcceptedOfferEvent != null)
            {
                OpponentAcceptedOfferEvent(this, EventArgs.Empty);
            }
        }

        public event EventHandler OpponentOptOutReceivedEvent;

        public void OpponentOptOutReceived()
        {
            if (OpponentOptOutReceivedEvent != null)
            {
                OpponentOptOutReceivedEvent(this, EventArgs.Empty);
            }
        }

        public event EventHandler TimeOutEvent;

        public void TimeOut()
        {
            if (TimeOutEvent != null)
            {
                TimeOutEvent(this, EventArgs.Empty);
            }
        }

        public event EventHandler<TimePassedEventArgs> TimePassedEvent;

        public void TimePassed(TimeSpan remainingTime)
        {
            if (TimePassedEvent != null)
            {
                TimePassedEvent(this, new TimePassedEventArgs(remainingTime));
            }
        }


        public event EventHandler NegotiationEndedEvent;

        public void NegotiationEnded()
        {
            if (NegotiationEndedEvent != null)
            {
                NegotiationEndedEvent(this, EventArgs.Empty);
            }
        }

        public void SignAgreement()
        {
            if (AgreementSignedEvent != null)
            {
                AgreementSignedEvent(this, EventArgs.Empty);
            }
        }

        public event EventHandler AgreementSignedEvent;

        public void RejectOffer()
        {
            if (OfferRejectedEvent != null)
            {
                OfferRejectedEvent(this, EventArgs.Empty);
            }
        }

        public event EventHandler OfferRejectedEvent;
    }
}