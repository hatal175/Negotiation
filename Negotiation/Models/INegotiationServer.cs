using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Negotiation.Models
{
    public class NewOfferEventArgs : EventArgs
    {
        public NewOfferEventArgs(NegotiationOffer offer)
        {
            Offer = offer;
        }

        public NegotiationOffer Offer { get; set; }
    }

    public interface INegotiationServer
    {
        event EventHandler<NewOfferEventArgs> NewOfferEvent;
        event EventHandler OfferAcceptedEvent;
    }
}
