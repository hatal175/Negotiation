using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Negotiation.Models
{
    public class OfferEventArgs : EventArgs
    {
        public OfferEventArgs(NegotiationOffer offer)
        {
            Offer = offer;
        }

        public NegotiationOffer Offer { get; set; }
    }

    public interface INegotiationServer
    {
        event EventHandler<OfferEventArgs> NewOfferEvent;
        event EventHandler OfferAcceptedEvent;
        event EventHandler OfferRejectedEvent;
        event EventHandler AgreementSignedEvent;
        event EventHandler OptOutEvent;
        
        void NegotiationStarted();
        void NegotiationEnded();
        void OpponentOfferReceived(NegotiationOffer offer);
        void OpponentAcceptedOffer();
        void OpponentOptOutReceived();
        void TimeOut();
        void TimePassed(TimeSpan remainingTime);
    }
}
