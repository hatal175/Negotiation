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

            return Index();
        }
    }
}