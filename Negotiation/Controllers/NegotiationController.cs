using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Negotiation.Models;
using System.Globalization;
using Negotiation.App_Start;

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
            //if (!ModelState.IsValid)
            //{
            //    return View("PreNegotiationQuestionnaire", model);
            //}

            //if (!model.AgreeIRB)
            //{
            //    ModelState.AddModelError("AgreeIRB", "Please agree to the IRB form");
            //    return View("PreNegotiationQuestionnaire", model);
            //}

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

            int optoutend = (desc.Optout + desc.TimeEffect * NegotiationManager.TotalRounds);

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

            //if (!model.Questions.Select(x => x.Answer).SequenceEqual(NegotiationManager.TutorialModels[model.TutorialId].Questions.Select(x => x.ActualAnswer)))
            //{
            //    return View("NegotiationTutorial", NegotiationManager.TutorialModels[model.TutorialId]);
            //}

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
                Offer = engine.Status.HumanStatus.Offer
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
                return NegotiationEnd(negotiationId);
            }

            engine.HumanChannel.AcceptOffer();

            return View("NegotiationEndView",
                engine.GetEndModel());
        }

        public ActionResult UpdateOpponentOffer(String negotiationId)
        {
            NegotiationEngine engine;
            if (negotiationId == null || !NegotiationManager.OnGoingNegotiations.TryGetValue(negotiationId, out engine))
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);

            if (!engine.NegotiationActive) return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);

            return PartialView("_OfferView", engine.Status.AiStatus.Offer);
        }

        public ActionResult UpdateActionHistory(String negotiationId)
        {
            NegotiationEngine engine;
            if (negotiationId == null || !NegotiationManager.OnGoingNegotiations.TryGetValue(negotiationId, out engine))
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);

            if (!engine.NegotiationActive) return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);

            return PartialView("_ActionHistoryView", engine.Actions);
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

        public ActionResult NegotiationConfiguration()
        {
            return View();
        }

        public ActionResult NewActiveDomain(NegotiationConfigurationModel model)
        {
            if (NegotiationManager.GameDomain.Name != model.ActiveDomain)
            {

            }

            return View("NegotiationConfiguration");
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
    }
}