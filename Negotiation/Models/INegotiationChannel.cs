using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Negotiation.Models
{
    public interface INegotiationChannel : INegotiationClient, INegotiationServer
    {
    }
}
