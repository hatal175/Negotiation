﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Negotiation.Models;
using System.Globalization;
using Negotiation.App_Start;

namespace Negotiation.Controllers
{
    public class NegotiationController : Controller
    {
        private Dictionary<String, NegotiationEngine> _onGoingNegotiations;

        public NegotiationController()
        {
            _onGoingNegotiations = new Dictionary<string, NegotiationEngine>();
        }

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

            String id = CreateNewNegotiation();

            NegotiationTutorialModel tutModel = CreateTutorialModel(id);

            return NegotiationTutorial(tutModel);
        }

        private string CreateNewNegotiation()
        {
            string id = this.Request.UserHostAddress + ";" + DateTime.Now.ToUniversalTime().ToString(CultureInfo.InvariantCulture);
            NegotiationEngine engine = new NegotiationEngine(NegotiationDomainManager.Domain, null, NegotiationDomainManager.GetHumanConfig(), NegotiationDomainManager.GetAiConfig());

            _onGoingNegotiations.Add(id, engine);

            return id;
        }

        public ActionResult NegotiationTutorial(NegotiationTutorialModel model)
        {
            return View("NegotiationTutorial",model);
        }

        private NegotiationTutorialModel CreateTutorialModel(string id)
        {
            NegotiationEngine engine = _onGoingNegotiations[id];

            SideConfig config = engine.HumanConfig; 

            NegotiationSideDescription desc = engine.Domain.OwnerVariantDict[config.Side][config.Variant];

            int optoutend = (desc.Optout + desc.TimeEffect * NegotiationDomainManager.TotalRounds);

            return new NegotiationTutorialModel
            {
                Id = id,
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
            if (!model.Questions.Select(x=>x.Answer).SequenceEqual(model.Questions.Select(x=>x.ActualAnswer)))
                return View("NegotiationTutorial", model);

            NegotiationEngine engine = _onGoingNegotiations[model.Id];

            NegotiationViewModel newModel = new NegotiationViewModel
            {
                Id = model.Id,
                AiSide = engine.AiConfig.Side,
                HumanConfig = engine.HumanConfig,
                RemainingTime = TimeSpan.FromSeconds(engine.Domain.NumberOfRounds * engine.Domain.RoundLength.TotalSeconds),
                Domain = engine.Domain,
                Actions = new List<NegotiationAction>()

            };

            return View("Negotiation", newModel);
        }

        public ActionResult Negotiation(NegotiationViewModel model)
        {
            return View(model);
        }

        public ActionResult NegotiationConfiguration()
        {
            return View();
        }

        public ActionResult NewActiveDomain(NegotiationConfigurationModel model)
        {
            if (NegotiationDomainManager.GameDomain.Name != model.ActiveDomain)
            {

            }

            return View("NegotiationConfiguration");
        }
    }
}