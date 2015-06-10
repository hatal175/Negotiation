using Dona.Basic;
using DonaStrategy;
using Negotiation.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KBAgent
{
    public class DonaKBAgent : BasicDona
    {
        private Dictionary<string, OfferUtility> m_BestCombinedUtilityOffers;
        private Dictionary<string, OfferUtility> m_BestFScoreUtilityOffers;

        private Dictionary<string, double> m_typeProbabilites;
        private Dictionary<NegotiationOffer, Dictionary<String,double>> m_offerProbabilities;

        private String m_currentOpponentTypeGuess = null;

        public override void Initialize(Negotiation.Models.NegotiationDomain domain, Negotiation.Models.SideConfig strategyConfig, string opponentSide, Negotiation.Models.INegotiationClient client)
        {
            base.Initialize(domain, strategyConfig, opponentSide, client);

            m_BestCombinedUtilityOffers = new Dictionary<string, OfferUtility>(BestCombinedUtilityOffers);
            m_BestFScoreUtilityOffers = new Dictionary<string, OfferUtility>(BestFScoreUtilityOffers);

            var opponentTypes = domain.OwnerVariantDict[opponentSide].Keys;
            m_typeProbabilites = opponentTypes.ToDictionary(k => k, x=>(1.0 / opponentTypes.Count));

            var utilitySums = opponentTypes.ToDictionary(k=>k, k=>AllOptions.Values.Sum(x=>x.UtilityDataDict[k].OpponentUtility));
            m_offerProbabilities = AllOptions.ToDictionary(kvp=>kvp.Key,kvp=>kvp.Value.UtilityDataDict.ToDictionary(kvp2=>kvp2.Key,kvp2=>kvp2.Value.OpponentUtility / utilitySums[kvp2.Key]));
        }

        protected override void OnOfferReceivedEvent(object sender, Negotiation.Models.OfferEventArgs e)
        {
            var opponentTypes = Domain.OwnerVariantDict[OpponentSide].Keys;

            var opponentOfferP = opponentTypes.Sum(x=>m_offerProbabilities[e.Offer][x] * m_typeProbabilites[x]);

            var newTypeProbabilites = opponentTypes.ToDictionary(
                x => x,
                x => m_offerProbabilities[e.Offer][x] * m_typeProbabilites[x] / opponentOfferP);

            GuessOpponentType(newTypeProbabilites.ArgMax(x => x.Value).Key);

            m_typeProbabilites = newTypeProbabilites;

            base.OnOfferReceivedEvent(sender, e);
        }

        private void GuessOpponentType(string opponentType)
        {
            if (m_currentOpponentTypeGuess == opponentType) return;

            m_currentOpponentTypeGuess = opponentType;

            BestFScoreUtilityOffers = new Dictionary<string, OfferUtility> { { opponentType, m_BestFScoreUtilityOffers[opponentType] } };
            BestCombinedUtilityOffers = new Dictionary<string, OfferUtility> { {opponentType, m_BestCombinedUtilityOffers[opponentType]} };
        }
    }
}
