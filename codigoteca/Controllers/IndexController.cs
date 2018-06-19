using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace codigoteca.Controllers
{
    public class IndexController : Controller
    {
        // GET: Index
        [Authorize]
        public ActionResult Index()
        {
            return View();
        }
    }
}