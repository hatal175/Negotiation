using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;
using System.Xml.Serialization;
using System.Collections.Concurrent;
using System.Reflection;
using System.Timers;

namespace Negotiation.Models
{
    public class SideConfig
    {
        public SideConfig() { }
        public SideConfig(String side, String variant)
        {
            Side = side;
            Variant = variant;
        }

        public String Side { get; set; }
        public String Variant { get; set; }
        public UserType Type { get; set; }
    }

    public class AiConfig : SideConfig
    {
        public int StrategyId { get; set; }
    }

    public class XmlFile
    {
        public String Name { get; set; }
        public String Content { get; set; }

        public XmlFile()
        {

        }

        public XmlFile(string name, string content)
        {
            Name = name;
            Content = content;
        }
    }

    public class NegotiationManager
    {
        public static ConcurrentDictionary<String, NegotiationEngine> OnGoingNegotiations { get; private set; }
        public static ConcurrentDictionary<String, NegotiationTutorialModel> TutorialModels { get; private set; }

        public static int TotalRounds;
        public static TimeSpan RoundLength;
        public static Timer m_cleanupTimer;

        static NegotiationManager()
        {
            TotalRounds = 15;
            RoundLength = new TimeSpan(0, 2, 0);
            OnGoingNegotiations = new ConcurrentDictionary<string, NegotiationEngine>();
            TutorialModels = new ConcurrentDictionary<string, NegotiationTutorialModel>();
            LoadDbData();

            m_cleanupTimer = new Timer(TimeSpan.FromMinutes(1).TotalMilliseconds);
            m_cleanupTimer.Elapsed += m_cleanupTimer_Elapsed;
        }

        static void m_cleanupTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            foreach (var kvp in OnGoingNegotiations)
            {
                if (!kvp.Value.NegotiationActive && DateTime.Now - kvp.Value.NegotiationEndTime > TimeSpan.FromMinutes(10))
                {
                    NegotiationEngine engine;
                    OnGoingNegotiations.TryRemove(kvp.Key, out engine);
                }
            }
        }

        static public GameDomain GameDomain { get; set; }
        public static NegotiationDomain Domain { get; private set; }

        static void LoadDbData()
        {
            GameDomain = new NegotiationContainer().GameDomainConfigSet.First().GameDomain;

            NegotiationDomain domain = new NegotiationDomain() { RoundLength = RoundLength, NumberOfRounds = TotalRounds };
            XmlDocument doc = new XmlDocument();
            
            doc.LoadXml(GameDomain.DomainXML);
            domain.Extract(doc.ChildNodes[0]);

            Domain = domain;
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
            return new SideConfig 
            { 
                Side = Domain.OwnerVariantDict.Keys.First(), 
                Variant = Domain.OwnerVariantDict.Values.First().Keys.First() , 
                Type = UserType.Human
            };
        }

        public static AiConfig GetAiConfig()
        {
            NegotiationContainer cont = new NegotiationContainer();
            var strat = cont.StrategyConfigSet.First();

            return new AiConfig
            {
                Side = Domain.OwnerVariantDict.Keys.ElementAt(1), 
                Variant = Domain.OwnerVariantDict.Values.ElementAt(1).Keys.First(), 
                Type = UserType.Agent,
                StrategyId = strat.Id
            };
        }

        internal static void SaveNewNegotiation(NegotiationEngine engine, PreNegotiationQuestionnaireViewModel model)
        {
            NegotiationContainer cont = new NegotiationContainer();

            #region Human User
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
                Id = engine.NegotiationId,
                Type = UserType.Human,
                UserData = humanData,
                UserRole = humanRole,
                GameId = engine.NegotiationId
            };

            #endregion

            #region Ai User

            UserRole aiRole = new UserRole()
            {
                Description = engine.AiConfig.Side,
                Variant = engine.AiConfig.Variant
            };

            User aiUser = new User()
            {
                Id = engine.StrategyName + "|" + DateTime.Now,
                Type = UserType.Agent,
                StrategyId = engine.AiConfig.StrategyId,
                UserRole = aiRole,
                GameId = engine.NegotiationId,
            };

            #endregion

            cont.GameSet.Add(new Game()
                {
                    Id = engine.NegotiationId,
                    GameDomainId = GameDomain.Id,
                    StartTime = DateTime.Now
                });

            cont.UserDataSet.Add(humanData);
            cont.UserRoleSet.Add(humanRole);
            cont.UserRoleSet.Add(aiRole);
            cont.UserSet.Add(humanUser);
            cont.UserSet.Add(aiUser);

            try
            {
                cont.SaveChanges();
            }
            catch (Exception)
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

        internal static IAgentStrategy GetStrategy(int strategyId, out String strategyName)
        {
            NegotiationContainer cont = new NegotiationContainer();
            var strat = cont.StrategySet.Find(strategyId);

            Assembly assembly = Assembly.LoadFile(HttpContext.Current.Server.MapPath(strat.DllPath));
            Type type = assembly.GetTypes().First(x => x.GetInterface("IAgentStrategy") != null);
            IAgentStrategy example = assembly.CreateInstance(type.FullName) as IAgentStrategy;

            strategyName = strat.StrategyName;

            return example;
        }

        internal static IEnumerable<Game> GetGames()
        {
            NegotiationContainer cont = new NegotiationContainer();
            return cont.GameSet;
        }

        internal static object GetGame(string gameId)
        {
            NegotiationContainer cont = new NegotiationContainer();
            return cont.GameSet.Find(gameId);
        }

        internal static void SaveNegotiationEnd(String negotiationId, int humanScore, int agentScore, DateTime negotiationEndTime)
        {
            NegotiationContainer cont = new NegotiationContainer();
            var game = cont.GameSet.Find(negotiationId);

            if (game == null) return;

            game.Users.First(x => x.Type == UserType.Agent).Score = agentScore;
            game.Users.First(x => x.Type == UserType.Human).Score = humanScore;
            game.Endtime = negotiationEndTime;

            cont.SaveChanges();
        }

        internal static object GetUser(string userId)
        {
            NegotiationContainer cont = new NegotiationContainer();
            return cont.UserSet.Find(userId);
        }

        internal static void SetNewDomain(int newActiveDomain)
        {
            NegotiationContainer cont = new NegotiationContainer();
            cont.GameDomainConfigSet.Remove(cont.GameDomainConfigSet.Find(GameDomain.Id));
            cont.GameDomainConfigSet.Add(new GameDomainConfig() { Id = newActiveDomain });
            cont.SaveChanges();

            LoadDbData();
        }

        internal static void CreateDomain(string domainName, string domainXml, IEnumerable<XmlFile> variants)
        {
            NegotiationContainer cont = new NegotiationContainer();

            var domainVariants = variants.Select(x => new DomainVariant() { Name = x.Name, VariantXML = x.Content }).ToList();
            cont.DomainVariantSet.AddRange(domainVariants);

            cont.GameDomainSet.Add(new GameDomain()
                {
                    Name = domainName,
                    DomainXML = domainXml,
                    DomainVariant = domainVariants
                });

            cont.SaveChanges();
        }

        public static IEnumerable<Models.GameDomain> GetDomains()
        {
            return new NegotiationContainer().GameDomainSet;
        }

        internal static void SetNewStrategy(int newActiveStrategy)
        {
            NegotiationContainer cont = new NegotiationContainer();
            cont.StrategyConfigSet.Remove(cont.StrategyConfigSet.Find(GetAiConfig().StrategyId));
            cont.StrategyConfigSet.Add(new StrategyConfig() { Id = newActiveStrategy });
            cont.SaveChanges();
        }

        internal static void CreateStrategy(string strategyName, string DllPath)
        {
            NegotiationContainer cont = new NegotiationContainer();

            cont.StrategySet.Add(new Strategy()
            {
                StrategyName = strategyName,
                DllPath = DllPath
            });

            cont.SaveChanges();
        }

        public static IEnumerable<Models.Strategy> GetStrategies()
        {
            return new NegotiationContainer().StrategySet;
        }

    }
}