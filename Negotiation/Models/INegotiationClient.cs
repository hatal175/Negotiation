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

        event EventHandler NegotiationStartedEvent;
        event EventHandler<OfferEventArgs> OfferReceivedEvent;
        event EventHandler OpponentAcceptedOfferEvent;
        event EventHandler OpponentOptOutReceivedEvent;
        event EventHandler TimeOutEvent;
    }
}
