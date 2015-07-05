using Dona.Basic;
using DonaStrategy;
using Negotiation.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dona.OfferTypesWithinRound
{
    public class DonaWithinRound : BasicDona
    {
        protected override void OnTimePassedEvent(object sender, ExtendedTimePassedEventArgs e)
        {
            if (!e.HasRoundPassed && RoundsPassed == 0)
            {
                return;
            }

            if (RoundsPassed < Domain.NumberOfRounds - 1)
            {
                RoundRobinOffer(BestFScoreUtilityOffers.Values, e.RemainingTime);
            }
            else
            {
                RoundRobinOffer(BestCombinedUtilityOffers.Values, e.RemainingTime);
            }
        }
    }
}
