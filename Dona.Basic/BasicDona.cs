using DonaStrategy;
using Negotiation.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dona.Basic
{
    public class BasicDona : BaseDonaStrategy
    {
        protected override void OnTimePassedEvent(object sender, ExtendedTimePassedEventArgs e)
        {
            if (!e.HasRoundPassed && RoundsPassed != Domain.NumberOfRounds - 1)
            {
                return;
            }

            if (RoundsPassed == 0)
            {
                return;
            }
            else if (RoundsPassed < Domain.NumberOfRounds - 1)
            {
                CompareOffer(OpponentOffer, false);
                SendOffer(BestFScoreUtilityOffers.Values.ElementAt(RoundsPassed % BestFScoreUtilityOffers.Count).Offer);
            }
            else
            {
                CompareOffer(OpponentOffer, false);
                int roundPart = (int)(Domain.RoundLength.TotalSeconds / Domain.OwnerVariantDict[this.OpponentSide].Count);
                int offerIndex = Math.Min(BestCombinedUtilityOffers.Count - 1, ((int)e.RemainingTime.TotalSeconds) / roundPart);
                NegotiationOffer offer = BestCombinedUtilityOffers.Values.ElementAtOrDefault(offerIndex).Offer;
                SendOffer(offer, false);
            }
        }

        protected override void OnOfferReceivedEvent(object sender, Negotiation.Models.OfferEventArgs e)
        {
            OpponentOffer = e.Offer;

            if (CurrentOffer == null)
            {
                return;
            }

            CompareOffer(e.Offer);
        }

        protected virtual void CompareOffer(NegotiationOffer offer, Boolean sendReject = true)
        {
            if (offer == null)
            {
                return;
            }

            if (RoundsPassed == 0)
            {
                if (offer.Equals(CurrentOffer))
                {
                    AcceptOffer();
                    return;
                }
            }
            else if (RoundsPassed < Domain.NumberOfRounds - 1)
            {
                if (AllOptions[offer].Utility >= BestFScoreUtilityOffers.Values.Min(x => x.Utility))
                {
                    AcceptOffer();
                    return;
                }
            }
            else
            {
                if (AllOptions[offer].Utility >= BestCombinedUtilityOffers.Values.Min(x => x.Utility))
                {
                    AcceptOffer();
                    return;
                }
            }

            if (sendReject)
                Client.RejectOffer();
        }
    }
}
