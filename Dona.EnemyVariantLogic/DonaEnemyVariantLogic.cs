using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Negotiation.Models;

namespace Dona.EnemyVariantLogic
{
    public class DonaEnemyVariantLogic : Basic.BasicDona
    {
        public HashSet<String> PossibleEnemyVariants { get; set; }

        public override void Initialize(Negotiation.Models.NegotiationDomain domain, Negotiation.Models.SideConfig strategyConfig, string opponentSide, Negotiation.Models.INegotiationClient client)
        {
            base.Initialize(domain, strategyConfig, opponentSide, client);

            PossibleEnemyVariants = new HashSet<String>(domain.OwnerVariantDict[opponentSide].Keys);
        }

        protected override void OnOfferReceivedEvent(object sender, Negotiation.Models.OfferEventArgs e)
        {
            if (PossibleEnemyVariants.Count > 1)
            {
                var utilites = PossibleEnemyVariants.Select(x => new {
                    Variant = x,
                    Utility = CalculateUtility(e.Offer.Offers, new SideConfig(OpponentSide, x))
                });

                double maxUtility = utilites.Max(x=>x.Utility);

                var removedEnemyVariants = utilites.Where(x => x.Utility < maxUtility).Select(x => x.Variant).ToList();

                if (removedEnemyVariants.Count > 0)
                {
                    foreach (var item in removedEnemyVariants)
	                {
                        PossibleEnemyVariants.Remove(item);
                        BestCombinedUtilityOffers.Remove(item);
                        BestFScoreUtilityOffers.Remove(item);
	                }
                }
            }

            base.OnOfferReceivedEvent(sender, e);
        }
    }
}
