using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Negotiation.Models
{
    public interface INegotiationClient
    {
        void SendOffer(NegotiationOffer offer);
        void AcceptOffer();
    }
}
