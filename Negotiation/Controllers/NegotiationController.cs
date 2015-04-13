using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Negotiation.Models;

namespace Negotiation.Controllers
{
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

             NegotiationTutorialModel tutModel = CreateTutorialModel();

             return NegotiationTutorial(tutModel);
        }

        public ActionResult NegotiationTutorial(NegotiationTutorialModel model)
        {
            return View(model);
        }

        private NegotiationTutorialModel CreateTutorialModel()
        {
            return new NegotiationTutorialModel
            {
                Questions = new List<QuestionModel>
                {
                    new QuestionModel 
                    {
                        Question = "Whose side are you playing in the negotiation?"
                        
                    },
                    new QuestionModel 
                    {
                        Question = "If the same agreement was reached in round 3 or in round 6, which of the following is correct?"
                        
                    },
                    new QuestionModel 
                    {
                        Question = "What is your score if no agreement had been reached by the end of the last round?"
                        
                    },
                    new QuestionModel 
                    {
                        Question = "What is the meaning of \"opting out\"?",
                        Options = 
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
                return View("NegotiationTutorial");

            return View();
        }
    }
}