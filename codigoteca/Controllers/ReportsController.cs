using codigoteca.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace codigoteca.Controllers
{
    public class ReportsController : Controller
    {
        private CodigotecaDBContext db = new CodigotecaDBContext();

        /*public bool isAdmin;
        
        public ReportsController()
        {
            int id = int.Parse(Session["UserId"].ToString());
            isAdmin = bool.Parse(Session["isAdmin"].ToString());
        }*/
        // GET: Reports

        public ActionResult Index()
        {
            return RedirectToAction("Posts");
        }

        // GET: Reports/Posts
        [Authorize]
        public ActionResult Posts(int search = 0,String from=null, String to=null)
        {
            
            if (!bool.Parse(Session["isAdmin"].ToString()))
            {
                return RedirectToAction("../Index/Index");
            }
            var chart = db.Posts
                    .GroupBy(p => p.PostDate.Month)
                    .Select(g => new Chart { month = g.Key, count = g.Count() }).ToList();

            ViewBag.chart = chart;

            var posts = db.Posts.ToList();
            if (search == 1)
            {
                if(from == null || to == null || from =="" || to == "")
                {
                    ViewBag.status = false;
                    ViewBag.Message= "Las dos fechas son necesarias para buscar";
                }
                else
                {
                    DateTime fromDate = DateTime.ParseExact(from, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                    DateTime toDate = DateTime.ParseExact(to, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                    int test = DateTime.Compare(fromDate, toDate);
                    if (DateTime.Compare(fromDate, toDate) == 1)
                    {
                        ViewBag.status = false;
                        ViewBag.Message = "La fecha desde no puede ser mayor a la fecha hasta";
                    }
                    else
                    {
                        posts = db.Posts.Where(a => a.PostDate >= fromDate && a.PostDate <= toDate).ToList();
                    }
                }

            }
            return View(posts);
        }

        [Authorize]
        public ActionResult Invitations(int search = 0, String from = null, String to = null)
        {
            if (!bool.Parse(Session["isAdmin"].ToString()))
            {
                return RedirectToAction("../Index/Index");
            }
            var chart = db.Invitations
                  .GroupBy(p => p.Date.Month)
                  .Select(g => new Chart { month = g.Key, count = g.Count() }).ToList();

            ViewBag.chart = chart;

            var invitations = db.Invitations.ToList();
            if (search == 1)
            {
                if (from == null || to == null || from == "" || to == "")
                {
                    ViewBag.status = false;
                    ViewBag.Message = "Las dos fechas son necesarias para buscar";
                }
                else
                {
                    DateTime fromDate = DateTime.ParseExact(from, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                    DateTime toDate = DateTime.ParseExact(to, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                    
                    if (DateTime.Compare(fromDate, toDate) == 1)
                    {
                        ViewBag.status = false;
                        ViewBag.Message = "La fecha desde no puede ser mayor a la fecha hasta";
                    }
                    else
                    {
                        invitations = db.Invitations.Where(a => a.Date >= fromDate && a.Date <= toDate).ToList();
                    }
                }

            }
            return View(invitations);
        }

        [Authorize]
        public ActionResult Groups(int search = 0, String from = null, String to = null)
        {
            if (!bool.Parse(Session["isAdmin"].ToString()))
            {
                return RedirectToAction("../Index/Index");
            }
            var chart = db.Groups
                   .GroupBy(p => p.GroupDate.Month)
                   .Select(g => new Chart { month = g.Key, count = g.Count() }).ToList();

            ViewBag.chart = chart;
          
            var groups = db.Groups.ToList();
            if (search == 1)
            {
                if (from == null || to == null || from == "" || to == "")
                {
                    ViewBag.status = false;
                    ViewBag.Message = "Las dos fechas son necesarias para buscar";
                }
                else
                {
                    DateTime fromDate = DateTime.ParseExact(from, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                    DateTime toDate = DateTime.ParseExact(to, "dd/MM/yyyy", CultureInfo.InvariantCulture);

                    if (DateTime.Compare(fromDate, toDate) == 1)
                    {
                        ViewBag.status = false;
                        ViewBag.Message = "La fecha desde no puede ser mayor a la fecha hasta";
                    }
                    else
                    {
                        groups = db.Groups.Where(a => a.GroupDate >= fromDate && a.GroupDate <= toDate).ToList();
                    }
                }

            }
            return View(groups);
        }
    }
}