using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;
using System.Xml.Serialization;
using System.Collections.Concurrent;

namespace Negotiation.Models
{
    public class SideConfig
    {
        public String Side { get; set; }
        public String Variant { get; set; }
        public UserType Type { get; set; }
    }

    public class NegotiationManager
    {
        public static ConcurrentDictionary<String, NegotiationEngine> OnGoingNegotiations { get; private set; }
        public static ConcurrentDictionary<String, NegotiationTutorialModel> TutorialModels { get; private set; }

        public static int TotalRounds;
        public static TimeSpan RoundLength;

        static NegotiationManager()
        {
            TotalRounds = 15;
            RoundLength = new TimeSpan(0, 2, 0);
            OnGoingNegotiations = new ConcurrentDictionary<string, NegotiationEngine>();
            TutorialModels = new ConcurrentDictionary<string, NegotiationTutorialModel>();
            LoadDbData();
        }

        static public GameDomain GameDomain { get; set; }
        public static NegotiationDomain Domain { get; private set; }

        static void LoadDbData()
        {
            GameDomain = new NegotiationContainer().GameDomainConfigSet.First().GameDomain;

            Domain = new NegotiationDomain() {RoundLength = RoundLength, NumberOfRounds = TotalRounds };
            XmlDocument doc = new XmlDocument();
            
            doc.LoadXml(GameDomain.DomainXML);
            Domain.Extract(doc.ChildNodes[0]);
        }

        static void SetActiveDomain(String domainName)
        {
            NegotiationContainer cont = new NegotiationContainer();
            GameDomain domain = cont.GameDomainSet.First(x => x.Name == domainName);
            cont.GameDomainConfigSet.First().GameDomain = domain;
            cont.SaveChanges();
        }

        public static SideConfig GetHumanConfig()
        {
            return new SideConfig { Side = Domain.OwnerVariantDict.Keys.First(), Variant = Domain.OwnerVariantDict.Values.First().Keys.First() , Type = UserType.Human};
        }

        public static SideConfig GetAiConfig()
        {
            return new SideConfig { Side = Domain.OwnerVariantDict.Keys.ElementAt(1), Variant = Domain.OwnerVariantDict.Values.ElementAt(1).Keys.First(), Type = UserType.Agent};
        }

        internal static void SaveNewNegotiation(string negotiationId, PreNegotiationQuestionnaireViewModel model, NegotiationEngine engine)
        {
            NegotiationContainer cont = new NegotiationContainer();

            UserRole humanRole = new UserRole()
            {
                Description = engine.HumanConfig.Side,
                Variant = engine.HumanConfig.Variant
            };

            UserData humanData = new UserData()
            {
                AgeRange = model.AgeRange,
                Country = model.BirthCountry,
                DegreeField = model.DegreeField,
                Education = model.Education,
                Gender = model.Gender,
                StudentId = model.ID,
                Name = model.Name,
                University = model.University
            };

            User humanUser = new User()
            {
                Id = negotiationId,
                Type = UserType.Human,
                UserData = humanData,
                UserRole = humanRole,
                GameId = negotiationId
            };

            cont.GameSet.Add(new Game()
                {
                    Id = negotiationId,
                    GameDomainId = GameDomain.Id,
                    Users = new List<User> { humanUser }
                });

            cont.UserDataSet.Add(humanData);
            cont.UserRoleSet.Add(humanRole);
            cont.UserSet.Add(humanUser);

            try
            {
                cont.SaveChanges();
            }
            catch (Exception ex)
            {
                
                throw;
            }
            
        }

        private static void SaveAction(NegotiationEngine engine, SideConfig side, NegotiationActionType type, String value = "")
        {
            NegotiationContainer cont = new NegotiationContainer();

            var game = cont.GameSet.Find(engine.NegotiationId);
            var user = game.Users.First(x => x.Type == side.Type);

            cont.NegotiationActionSet.Add(new NegotiationAction()
            {
                GameId = engine.NegotiationId,
                Type = NegotiationActionType.MakeOffer,
                User = user,
                RemainingTime = engine.Status.RemainingTime,
                UserId = user.Id,
                Value = value
            });
            cont.SaveChanges();
        }

        internal static void SaveNewOffer(NegotiationEngine engine, SideConfig side, string negotiationOffer)
        {
            SaveAction(engine, side, NegotiationActionType.MakeOffer, negotiationOffer);
        }

        internal static void SaveOptOut(NegotiationEngine engine, SideConfig side)
        {
            SaveAction(engine, side, NegotiationActionType.Optout);
        }

        internal static void SaveOfferAccepted(NegotiationEngine engine, SideConfig side)
        {
            SaveAction(engine, side, NegotiationActionType.AcceptOffer);
        }

        internal static void SaveTimeout(NegotiationEngine engine)
        {
            SaveAction(engine, engine.HumanConfig, NegotiationActionType.Timeout);
        }
    }
}