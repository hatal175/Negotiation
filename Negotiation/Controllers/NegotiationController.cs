using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Negotiation.Controllers
{
    public class NegotiationController : Controller
    {
        // GET: Negotiation
        public ActionResult Index()
        {
            return PreNegotiationQuestionnaire();
        }

        public ActionResult PreNegotiationQuestionnaire()
        {
            return View();
        }
    }
}