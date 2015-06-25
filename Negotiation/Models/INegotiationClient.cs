using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Negotiation.Models
{
    public interface INegotiationClient
    {
        void OptOut();
        void SendOffer(NegotiationOffer offer);
        void AcceptOffer();
        void RejectOffer();
        void SignAgreement();

        event EventHandler NegotiationStartedEvent;
        event EventHandler<OfferEventArgs> OfferReceivedEvent;
        event EventHandler OpponentAcceptedOfferEvent;
        event EventHandler OpponentRejectedOfferEvent;
        event EventHandler OpponentOptOutReceivedEvent;
        event EventHandler TimeOutEvent;
        event EventHandler<TimePassedEventArgs> TimePassedEvent;
        event EventHandler NegotiationEndedEvent;
    }
}
