﻿using Dona.Basic;
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
                CompareOffer(OpponentOffer, false);
                int roundPart = (int)(Domain.RoundLength.TotalSeconds / BestFScoreUtilityOffers.Count);
                int offerIndex = Math.Min(BestFScoreUtilityOffers.Count - 1, (((int)e.RemainingTime.TotalSeconds) / roundPart) % BestFScoreUtilityOffers.Count);
                NegotiationOffer offer = BestFScoreUtilityOffers.Values.ElementAtOrDefault(offerIndex).Offer;
                SendOffer(offer, false);
            }
            else
            {
                CompareOffer(OpponentOffer, false);
                int roundPart = (int)(Domain.RoundLength.TotalSeconds / BestCombinedUtilityOffers.Count);
                int offerIndex = Math.Min(BestCombinedUtilityOffers.Count - 1, ((int)e.RemainingTime.TotalSeconds) / roundPart);
                NegotiationOffer offer = BestCombinedUtilityOffers.Values.ElementAtOrDefault(offerIndex).Offer;
                SendOffer(offer, false);
            }
        }
    }
}
