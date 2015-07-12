using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Negotiation.Models;
using DonaStrategy;

namespace Dona.EnemyVariantLogic
{
    public class DonaEnemyVariantLogic : Basic.BasicDona
    {
        private Dictionary<string, OfferUtility> m_BestCombinedUtilityOffers;
        private Dictionary<string, OfferUtility> m_BestFScoreUtilityOffers;

        private Dictionary<string, double> m_typeUtilities;

        public override void Initialize(Negotiation.Models.NegotiationDomain domain, Negotiation.Models.SideConfig strategyConfig, string opponentSide, Negotiation.Models.INegotiationClient client)
        {
            base.Initialize(domain, strategyConfig, opponentSide, client);

            m_BestCombinedUtilityOffers = new Dictionary<string, OfferUtility>(BestCombinedUtilityOffers);
            m_BestFScoreUtilityOffers = new Dictionary<string, OfferUtility>(BestFScoreUtilityOffers);

            m_typeUtilities = Domain.OwnerVariantDict[OpponentSide].Keys.ToDictionary(k => k, k => 0.0);
        }

        protected override void OnOfferReceivedEvent(object sender, Negotiation.Models.OfferEventArgs e)
        {
            foreach (string variant in Domain.OwnerVariantDict[OpponentSide].Keys)
            {
                m_typeUtilities[variant] += AllOptions[e.Offer].UtilityDataDict[variant].OpponentUtility;
            }

            String opponentType = m_typeUtilities.ArgMax(x => x.Value).Key;

            BestFScoreUtilityOffers = new Dictionary<string, OfferUtility> { { opponentType, m_BestFScoreUtilityOffers[opponentType] } };
            BestCombinedUtilityOffers = new Dictionary<string, OfferUtility> { { opponentType, m_BestCombinedUtilityOffers[opponentType] } };

            base.OnOfferReceivedEvent(sender, e);
        }
    }
}
