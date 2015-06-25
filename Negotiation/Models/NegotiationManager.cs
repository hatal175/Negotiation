using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;
using System.Xml.Serialization;
using System.Collections.Concurrent;
using System.Reflection;
using System.Timers;
using System.IO;

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
        private static Timer m_cleanupTimer;

        private static String m_currentStrategyDirPath;

        static NegotiationManager()
        {
            TotalRounds = 15;
            RoundLength = new TimeSpan(0, 2, 0);
            OnGoingNegotiations = new ConcurrentDictionary<string, NegotiationEngine>();
            TutorialModels = new ConcurrentDictionary<string, NegotiationTutorialModel>();
            LoadDbData();

            m_cleanupTimer = new Timer(TimeSpan.FromMinutes(1).TotalMilliseconds);
            m_cleanupTimer.Elapsed += m_cleanupTimer_Elapsed;

            SetupStrategy();

            AppDomain.CurrentDomain.AssemblyResolve += ResolveStrategyDlls;
        }

        private static void SetupStrategy()
        {
            NegotiationContainer cont = new NegotiationContainer();
            var strat = cont.StrategyConfigSet.FirstOrDefault();

            m_currentStrategyDirPath = Path.GetDirectoryName(HttpContext.Current.Server.MapPath(strat.Strategy.DllPath));
        }

        static Assembly ResolveStrategyDlls (object sender, ResolveEventArgs args)
        {
            AssemblyName assemblyName = new AssemblyName(args.Name);
            String resolveDllPath = Path.Combine(m_currentStrategyDirPath, assemblyName.Name + ".dll");
            if (File.Exists(resolveDllPath))
            {
                return Assembly.LoadFile(resolveDllPath); ;
            }

            return null;
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
        static public GameDomainConfig Config { get; set; }

        static void LoadDbData()
        {
            var domainConfig = new NegotiationContainer().GameDomainConfigSet.FirstOrDefault();

            if (domainConfig != null)
            {
                Config = domainConfig;
                GameDomain = domainConfig.GameDomain;

                LoadDomain();
            }
        }

        private static void LoadDomain()
        {
            NegotiationDomain domain = new NegotiationDomain() { RoundLength = RoundLength, NumberOfRounds = TotalRounds };
            XmlDocument doc = new XmlDocument();

            doc.LoadXml(GameDomain.DomainXML);
            domain.Extract(doc.ChildNodes[0]);

            Domain = domain;
        }

        public static SideConfig GetHumanConfig()
        {
            return new SideConfig 
            { 
                Side = Config.HumanSide, 
                Variant = Config.HumanVariant, 
                Type = UserType.Human
            };
        }

        public static AiConfig GetAiConfig()
        {
            NegotiationContainer cont = new NegotiationContainer();
            var gameConfig = cont.GameDomainConfigSet.First();
            var strat = cont.StrategyConfigSet.FirstOrDefault();

            return new AiConfig
            {
                Side = gameConfig.AiSide,
                Variant = gameConfig.AiVariant, 
                Type = UserType.Agent,
                StrategyId = strat != null ? strat.Id : 0
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
                Type = type,
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

        internal static void SaveOfferRejected(NegotiationEngine engine, SideConfig side)
        {
            SaveAction(engine, side, NegotiationActionType.RejectOffer);
        }

        internal static void SaveAgreementSigned(NegotiationEngine engine, SideConfig side)
        {
            SaveAction(engine, side, NegotiationActionType.Sign);
        }

        internal static void SaveTimeout(NegotiationEngine engine)
        {
            SaveAction(engine, engine.HumanConfig, NegotiationActionType.Timeout);
        }

        internal static IAgentStrategy GetStrategy(int strategyId, out String strategyName)
        {
            NegotiationContainer cont = new NegotiationContainer();
            var strat = cont.StrategySet.Find(strategyId);

            String dllPath = HttpContext.Current.Server.MapPath(strat.DllPath);
            String dllDir = System.IO.Path.GetDirectoryName(dllPath);

            Assembly assembly = Assembly.LoadFile(dllPath);

            Type type = assembly.GetTypes().First(x => x.GetInterface("IAgentStrategy") != null);
            IAgentStrategy example = assembly.CreateInstance(type.FullName) as IAgentStrategy;

            strategyName = strat.StrategyName;

            return example;
        }

        private static Assembly MyResolveEventHandler(object sender, ResolveEventArgs args)
        {
            throw new NotImplementedException();
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

            if (GameDomain != null)
            {
                var activeConfig = cont.GameDomainConfigSet.Find(GameDomain.Id);
                if (activeConfig != null)
                {
                    cont.GameDomainConfigSet.Remove(activeConfig);
                }
            }

            GameDomain = cont.GameDomainSet.Find(newActiveDomain);
            LoadDomain();
            
            string humanSide = Domain.OwnerVariantDict.Keys.First();
            string aiSide = Domain.OwnerVariantDict.Keys.ElementAt(1);

            Config = new GameDomainConfig()
            {
                Id = newActiveDomain,
                HumanSide = humanSide,
                HumanVariant = Domain.OwnerVariantDict[humanSide].Keys.First(),
                AiSide = aiSide,
                AiVariant = Domain.OwnerVariantDict[aiSide].Keys.First()
            };

            cont.GameDomainConfigSet.Add(Config);

            try
            {
                cont.SaveChanges();
            }
            catch (Exception ex)
            {
                throw;
            }
            
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

            if (GameDomain == null)
            {
                SetNewDomain(cont.GameDomainSet.First().Id);
            }
        }

        public static IEnumerable<Models.GameDomain> GetDomains()
        {
            return new NegotiationContainer().GameDomainSet;
        }

        internal static void SetNewStrategy(int newActiveStrategy)
        {
            NegotiationContainer cont = new NegotiationContainer();
            
            var stratId = GetAiConfig().StrategyId;
            if (stratId != 0)
            {
                cont.StrategyConfigSet.Remove(cont.StrategyConfigSet.Find(stratId));
            }
            
            cont.StrategyConfigSet.Add(new StrategyConfig() { Id = newActiveStrategy });
            cont.SaveChanges();

            SetupStrategy();
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

            if (!cont.StrategyConfigSet.Any())
            {
                SetNewStrategy(cont.StrategySet.First().Id);
            }
        }

        public static IEnumerable<Models.Strategy> GetStrategies()
        {
            return new NegotiationContainer().StrategySet;
        }

        internal static void SaveUserOptionChange(NegotiationEngine engine, string topic, string option)
        {
            SaveAction(engine, engine.HumanConfig, NegotiationActionType.MakeChange, String.Format("{{\"{0}\":\"{1}\"}}",topic,option));
        }

        internal static void SetDomainVariants(string humanSide, string humanVariant, string aiVariant)
        {
            NegotiationContainer cont = new NegotiationContainer();
            var domainConfig = new NegotiationContainer().GameDomainConfigSet.First();

            domainConfig.HumanSide = humanSide;
            domainConfig.HumanVariant = humanVariant;
            domainConfig.AiSide = Domain.OwnerVariantDict.Keys.Except(humanSide).First();
            domainConfig.AiVariant = aiVariant;

            cont.SaveChanges();

            Config = domainConfig;
        }

       
    }
}