using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Negotiation.Models;
using System.Globalization;
using Negotiation.App_Start;
using System.IO;

namespace Negotiation.Controllers
{
    [SessionState(System.Web.SessionState.SessionStateBehavior.Disabled)]
    public class NegotiationController : Controller
    {
        // GET: Negotiation
        public ActionResult Index()
        {
            return View("PreNegotiationQuestionnaire");
        }

        public ActionResult PreNegotiationQuestionnaire()
        {
            return View();
        }

        public ActionResult SubmitUserData(PreNegotiationQuestionnaireViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("PreNegotiationQuestionnaire", model);
            }

            if (!model.AgreeIRB)
            {
                ModelState.AddModelError("AgreeIRB", "Please agree to the IRB form");
                return View("PreNegotiationQuestionnaire", model);
            }

            String id = CreateNewNegotiation(model);

            NegotiationTutorialModel tutModel = CreateTutorialModel(id);
            NegotiationManager.TutorialModels.TryAdd(id, tutModel);

            return NegotiationTutorial(tutModel);
        }

        private string CreateNewNegotiation(PreNegotiationQuestionnaireViewModel model)
        {
            string id = this.Request.UserHostAddress + ";" + DateTime.Now.ToUniversalTime().ToString(CultureInfo.InvariantCulture);
            NegotiationEngine engine = 
                new NegotiationEngine(
                    id,
                    NegotiationManager.Domain, 
                    model,
                    NegotiationManager.GetHumanConfig(), 
                    NegotiationManager.GetAiConfig());

            NegotiationManager.SaveNewNegotiation(engine, model);
            NegotiationManager.OnGoingNegotiations.TryAdd(id, engine);

            return id;
        }

        public ActionResult NegotiationTutorial(NegotiationTutorialModel model)
        {
            if (model == null || model.TutorialId == null)
            {
                return RedirectToAction("Negotiation");
            }

            return View("NegotiationTutorial",model);
        }

        private NegotiationTutorialModel CreateTutorialModel(string id)
        {
            NegotiationEngine engine = NegotiationManager.OnGoingNegotiations[id];

            SideConfig config = engine.HumanConfig; 

            NegotiationSideDescription desc = engine.Domain.OwnerVariantDict[config.Side][config.Variant];

            int optoutend = engine.Domain.CalculateOptoutScore(config, NegotiationManager.TotalRounds);

            return new NegotiationTutorialModel
            {
                TutorialId = id,
                Questions = new List<QuestionModel>
                {
                    new QuestionModel 
                    {
                        Question = "Whose side are you playing in the negotiation?",
                        Options = engine.Domain.OwnerVariantDict.Keys.ToList(),
                        ActualAnswer = config.Side
                        
                    },
                    new QuestionModel 
                    {
                        Question = "If the same agreement was reached in round 3 or in round 6, which of the following is correct?",
                        Options = new List<string>
                        {
                            "They have the same score.",
                            "Round 3 will have a higher score.",
                            "Round 6 will have a higher score"
                        },
                        ActualAnswer = "Round 3 will have a higher score."
                        
                    },
                    new QuestionModel 
                    {
                        Question = "What is your score if no agreement had been reached by the end of the last round?",
                        Options = new List<string>
                        {
                            "0",
                            "No score - you have lost the negotiation",
                            optoutend.ToString()
                        },
                        ActualAnswer = optoutend.ToString()
                    },
                    new QuestionModel 
                    {
                        Question = "What is the meaning of \"opting out\"?",
                        Options = new List<string>
                        {
                            "It means that the other side won the negotiation.",
                            "It means you get a predetermined amount of points minus the time decrease."
                        },
                        ActualAnswer = "It means you get a predetermined amount of points minus the time decrease."
                    }
                }
            };
        }

        public ActionResult SubmitTutorialAnswers(NegotiationTutorialModel model)
        {
            if (model == null || model.TutorialId == null)
            {
                return RedirectToAction("Negotiation");
            }

            if (!model.Questions.Select(x => x.Answer).SequenceEqual(NegotiationManager.TutorialModels[model.TutorialId].Questions.Select(x => x.ActualAnswer)))
            {
                return View("NegotiationTutorial", NegotiationManager.TutorialModels[model.TutorialId]);
            }

            NegotiationTutorialModel temp;
            NegotiationManager.TutorialModels.TryRemove(model.TutorialId, out temp);

            NegotiationEngine engine = NegotiationManager.OnGoingNegotiations[model.TutorialId];

            NegotiationViewModel newModel = new NegotiationViewModel
            {
                Id = model.TutorialId,
                AiSide = engine.AiConfig.Side,
                HumanConfig = engine.HumanConfig,
                RemainingTime = TimeSpan.FromSeconds(engine.Domain.NumberOfRounds * engine.Domain.RoundLength.TotalSeconds),
                Domain = engine.Domain,
                Actions = new List<NegotiationActionModel>()

            };

            engine.BeginNegotiation();

            return RedirectToAction("Negotiation", new { negotiationId = model.TutorialId });
        }

        public ActionResult Negotiation(String negotiationId)
        {
            if (negotiationId == null) return RedirectToAction("Index");

            NegotiationEngine engine;
            if (negotiationId == null || !NegotiationManager.OnGoingNegotiations.TryGetValue(negotiationId, out engine))
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);

            if (!engine.NegotiationActive)
            {
                return View("NegotiationEndView",
                    engine.GetEndModel());
            }

            NegotiationViewModel model = new NegotiationViewModel
            {
                Id = negotiationId,
                AiSide = engine.AiConfig.Side,
                HumanConfig = engine.HumanConfig,
                RemainingTime = engine.Status.RemainingTime,
                Domain = engine.Domain,
                Actions = engine.Actions,
                OpponentOffer = engine.Status.AiStatus.Offer,
                Offer = engine.Status.HumanStatus.Offer,
                LastAcceptedOffer = engine.Status.LastAcceptedOffer
            };

            return View(model);
        }

        [HttpPost]
        public ActionResult SendOffer(String negotiationId, NegotiationOffer offer)
        {
            NegotiationEngine engine;
            if (negotiationId == null || !NegotiationManager.OnGoingNegotiations.TryGetValue(negotiationId, out engine))
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);

            if (!engine.NegotiationActive)
            {
                return NegotiationEnd(negotiationId);
            }

            engine.HumanChannel.SendOffer(offer);

            return new EmptyResult();
        }

        [HttpPost]
        public ActionResult AcceptOffer(String negotiationId)
        {
            NegotiationEngine engine;
            if (negotiationId == null || !NegotiationManager.OnGoingNegotiations.TryGetValue(negotiationId, out engine))
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);

            if (!engine.NegotiationActive)
            {
                return new EmptyResult();
            }

            engine.HumanChannel.AcceptOffer();

            return new EmptyResult();
        }

        [HttpPost]
        public ActionResult RejectOffer(String negotiationId)
        {
            NegotiationEngine engine;
            if (negotiationId == null || !NegotiationManager.OnGoingNegotiations.TryGetValue(negotiationId, out engine))
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);

            if (!engine.NegotiationActive)
            {
                return new EmptyResult();
            }

            engine.HumanChannel.RejectOffer();

            return new EmptyResult();
        }

        [HttpPost]
        public ActionResult SignAgreement(String negotiationId)
        {
            NegotiationEngine engine;
            if (negotiationId == null || !NegotiationManager.OnGoingNegotiations.TryGetValue(negotiationId, out engine))
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);

            if (!engine.NegotiationActive)
            {
                return new EmptyResult();
            }

            engine.HumanChannel.SignAgreement();

            return new EmptyResult();
        }

        public ActionResult UpdateOpponentOffer(String negotiationId)
        {
            NegotiationEngine engine;
            if (negotiationId == null || !NegotiationManager.OnGoingNegotiations.TryGetValue(negotiationId, out engine))
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);

            if (!engine.NegotiationActive) return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);

            return PartialView(
                "_OfferView",
                new OfferViewModel()
                {
                    Offer = engine.Status.AiStatus.Offer,
                    DataPrefix = "opOffer"
                });
        }

        public ActionResult UpdateLastAcceptedOffer(String negotiationId)
        {
            NegotiationEngine engine;
            if (negotiationId == null || !NegotiationManager.OnGoingNegotiations.TryGetValue(negotiationId, out engine))
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);

            if (!engine.NegotiationActive) return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);

            return PartialView(
                "_OfferView",
                new OfferViewModel()
                {
                    Offer = engine.Status.LastAcceptedOffer,
                    DataPrefix = "acceptedOffer"
                });
        }

        public ActionResult UpdateActionHistory(String negotiationId)
        {
            NegotiationEngine engine;
            if (negotiationId == null || !NegotiationManager.OnGoingNegotiations.TryGetValue(negotiationId, out engine))
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);

            if (!engine.NegotiationActive) return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);

            return PartialView("_ActionHistoryView", engine.Actions.Where(action => action.Type != NegotiationActionType.MakeChange));
        }

        public ActionResult OptOut(String negotiationId)
        {
            NegotiationEngine engine;
            if (negotiationId == null || !NegotiationManager.OnGoingNegotiations.TryGetValue(negotiationId, out engine))
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);

            engine.HumanChannel.OptOut();
            return View("NegotiationEndView",
                new NegotiationEndModel()
                {
                    Score = engine.Status.HumanStatus.Score,
                    Message = "You opted out."
                });
        }

        public ActionResult UpdateTimer(String negotiationId)
        {
            NegotiationEngine engine;
            if (negotiationId==null || !NegotiationManager.OnGoingNegotiations.TryGetValue(negotiationId, out engine))
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);

            return PartialView("NegotiationTimerView", engine.Status.RemainingTime);
        }

        public ActionResult NegotiationDomainConfiguration()
        {
            return View(new NegotiationDomainConfig()
                {
                    ActiveId = NegotiationManager.GameDomain != null ? NegotiationManager.GameDomain.Id : 0,
                    Items = NegotiationManager.GetDomains(),
                    HumanSide = NegotiationManager.GameDomain != null ? NegotiationManager.Config.HumanSide : null,
                    HumanVariant = NegotiationManager.GameDomain != null ? NegotiationManager.Config.HumanVariant : null,
                    AiVariant = NegotiationManager.GameDomain != null ? NegotiationManager.Config.AiVariant : null,
                });
        }

        public ActionResult NewActiveDomain(int newActiveDomain)
        {
            if (NegotiationManager.GameDomain == null || NegotiationManager.GameDomain.Id != newActiveDomain)
            {
                NegotiationManager.SetNewDomain(newActiveDomain);
            }

            return RedirectToAction("NegotiationDomainConfiguration");
        }

        public ActionResult SetDomainVariants(string humanSide, string humanVariant, string aiVariant)
        {
            NegotiationManager.SetDomainVariants(humanSide, humanVariant, aiVariant);

            return RedirectToAction("NegotiationDomainConfiguration");
        }

        [HttpPost]
        public ActionResult UploadDomain(String domainName, HttpPostedFileBase domainXmlFile, IEnumerable<HttpPostedFileBase> variantFiles)
        {
            if (domainName == null || domainXmlFile == null || variantFiles == null) return RedirectToAction("NegotiationDomainConfiguration");

            String domainXml;

            if (domainXmlFile.ContentLength == 0)
            {
                return RedirectToAction("NegotiationDomainConfiguration");
            }

            using (StreamReader reader = new StreamReader(domainXmlFile.InputStream))
            {
                domainXml = reader.ReadToEnd();
            }

            var variants = variantFiles.Select(x =>
                {
                    using (StreamReader reader = new StreamReader(x.InputStream))
                    {
                        return new XmlFile(x.FileName,reader.ReadToEnd());
                    }
                });

            NegotiationManager.CreateDomain(domainName, domainXml, variants);

            return RedirectToAction("NegotiationDomainConfiguration");
        }

        public ActionResult NegotiationEnd(string negotiationId)
        {
            NegotiationEngine engine;
            if (negotiationId == null || !NegotiationManager.OnGoingNegotiations.TryGetValue(negotiationId, out engine))
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);

            return View("NegotiationEndView", (object)engine.GetEndModel());
        }

        public ActionResult Tutorial(string negotiationId)
        {
            NegotiationEngine engine;
            if (negotiationId == null || !NegotiationManager.OnGoingNegotiations.TryGetValue(negotiationId, out engine))
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);

            return View("TutorialPresentationView", new TutorialPresentationModel()
                {
                    HumanConfig = engine.HumanConfig,
                    AiConfig = engine.AiConfig,
                    Domain = engine.Domain
                });
        }

        public ActionResult AllGameView()
        {
            return View("AllGameView", NegotiationManager.GetGames());
        }

        public ActionResult GameSummary(string gameId)
        {
            if (String.IsNullOrEmpty(gameId))
            {
                return RedirectToAction("Negotiation");
            }

            var game = NegotiationManager.GetGame(gameId);

            if (game == null)
            {
                return RedirectToAction("Negotiation");
            }

            return View("GameSummaryView", game);
        }

        public ActionResult UserSummary(string userId)
        {
            if (String.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Negotiation");
            }

            var user = NegotiationManager.GetUser(userId);

            if (user == null)
            {
                return RedirectToAction("Negotiation");
            }

            return View("UserSummaryView", user);
        }

        public ActionResult NegotiationStrategyConfiguration()
        {
            return View(new NegotiationConfigurationModel<Strategy>()
                {
                    ActiveId = NegotiationManager.GetAiConfig().StrategyId,
                    Items = NegotiationManager.GetStrategies()
                });
        }

        public ActionResult NewActiveStrategy(int newActiveStrategy)
        {
            if (newActiveStrategy == 0) return RedirectToAction("NegotiationStrategyConfiguration");

            if (NegotiationManager.GetAiConfig().StrategyId != newActiveStrategy)
            {
                NegotiationManager.SetNewStrategy(newActiveStrategy);
            }

            return RedirectToAction("NegotiationStrategyConfiguration");
        }

        [HttpPost]
        public ActionResult UploadStrategy(String strategyName, HttpPostedFileBase strategyDll, IEnumerable<HttpPostedFileBase> dependencyDlls)
        {
            if (strategyName == null || strategyDll == null) return RedirectToAction("NegotiationStrategyConfiguration");

            if (strategyDll.ContentLength == 0)
            {
                return RedirectToAction("NegotiationStrategyConfiguration");
            }

            String virtualDirPath = Path.Combine("~/Dlls", DateTime.Now.ToString("yyyyMMdd.hh.mm.ss.") + strategyDll.FileName);
            String virtualDllPath = Path.Combine(virtualDirPath, strategyDll.FileName);

            String DllDirPath = System.Web.HttpContext.Current.Server.MapPath(virtualDirPath);

            Directory.CreateDirectory(DllDirPath);

            String DllPath = Path.Combine(DllDirPath, strategyDll.FileName);

            using (FileStream fs = new FileStream(DllPath, FileMode.CreateNew))
            {
                strategyDll.InputStream.CopyTo(fs);
            }

            if (dependencyDlls != null && dependencyDlls.Any()) 
            {
                foreach (var dll in dependencyDlls.Where(x=>x!=null))
	            {
                    String dependencyDllPath = Path.Combine(DllDirPath, dll.FileName);

                    using (FileStream fs = new FileStream(dependencyDllPath, FileMode.CreateNew))
                    {
                        dll.InputStream.CopyTo(fs);
                    }
	            }
            }

            NegotiationManager.CreateStrategy(strategyName, virtualDllPath);

            return RedirectToAction("NegotiationStrategyConfiguration");
        }

        public ActionResult SaveUserOptionChange(String negotiationId, String topic, String option)
        {
            NegotiationEngine engine;
            if (negotiationId == null || !NegotiationManager.OnGoingNegotiations.TryGetValue(negotiationId, out engine))
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);

            NegotiationManager.SaveUserOptionChange(engine, topic, option);

            return new HttpStatusCodeResult(System.Net.HttpStatusCode.OK);
        }
    }
}