using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DonaStrategy;
using Negotiation.Models;

namespace Dona.JustBest
{
    public class DonaBest : BaseDonaStrategy
    {
        protected override void OnTimePassedEvent(object sender, ExtendedTimePassedEventArgs e)
        {
            if (!e.HasRoundPassed)
            {
                return;
            }

            if (RoundsPassed == 0)
            {
                return;
            }
            else
            {
                SendOffer(BestOffer);
            }
        }

        protected override void OnOfferReceivedEvent(object sender, Negotiation.Models.OfferEventArgs e)
        {
            if (e.Offer.Equals(BestOffer))
            {
                AcceptOffer();
            }
        }
    }
}
