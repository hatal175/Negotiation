using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DonaStrategy;
using Negotiation.Models;

namespace Dona.JustFScore
{
    public class DonaJustFScore : BaseDonaStrategy
    {
        protected List<OfferUtility> FScoreOffersByUtility;

        public override void Initialize(NegotiationDomain domain, SideConfig strategyConfig, string opponentSide, INegotiationClient client)
        {
            if (domain.NumberOfRounds != 15)
            {
                throw new ArgumentException("This strategy has been optimized for 15 rounds", "domain");
            }

            if (domain.OwnerVariantDict[opponentSide].Keys.Count != 3)
            {
                throw new ArgumentException("This strategy has been optimized for 3 opponent variants", "domain");
            }

            base.Initialize(domain, strategyConfig, opponentSide, client);

            FScoreOffersByUtility = BestFScoreUtilityOffers.Values.OrderByDescending(x => x.Utility).ToList();
        }

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
            else if (RoundsPassed <= 7)
            {
                SendOffer(BestOffer);
            }
            else if (RoundsPassed <= 11)
            {
                CompareOffer(OpponentOffer, false);
                SendOffer(FScoreOffersByUtility[0].Offer);
            }
            else if (RoundsPassed <= 13)
            {
                CompareOffer(OpponentOffer, false);
                SendOffer(FScoreOffersByUtility[1].Offer);
            }
            else
            {
                CompareOffer(OpponentOffer, false);
                SendOffer(FScoreOffersByUtility[2].Offer);
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

            if (RoundsPassed >= 0 && RoundsPassed <= 7)
            {
                if (offer.Equals(BestOffer))
                {
                    AcceptOffer();
                    return;
                }
            }
            else if (RoundsPassed >= 8 && RoundsPassed <= 11)
            {
                if (AllOptions[offer].Utility >= FScoreOffersByUtility[0].Utility)
                {
                    AcceptOffer();
                    return;
                }
            }
            else if (RoundsPassed >= 12 && RoundsPassed <= 13)
            {
                if (AllOptions[offer].Utility >= FScoreOffersByUtility[1].Utility)
                {
                    AcceptOffer();
                    return;
                }
            }
            else if (RoundsPassed == 14)
            {
                if (AllOptions[offer].Utility >= FScoreOffersByUtility[2].Utility)
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
