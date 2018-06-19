using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace codigoteca.Controllers
{
    public class HomeController : Controller
    {

        public ActionResult Index()
        {
            /*var context = new CodigotecaDBContext();
            context.Posts.Add(new Models.Post() { PostId=1, PostDate = DateTime.Parse("Oct 26, 2003 12:00:00 AM"), PostName = "Prueba desde index", PostDescrip = "prueba desde index" });
            context.SaveChanges();
            return context.Posts.Count().ToString();*/
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}