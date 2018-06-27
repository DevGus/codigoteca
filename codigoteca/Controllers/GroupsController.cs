using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using codigoteca;
using codigoteca.Models;

namespace codigoteca.Controllers
{
    public class GroupsController : Controller
    {
        private CodigotecaDBContext db = new CodigotecaDBContext();

        // GET: Groups
        [Authorize]
        public ActionResult Index()
        {
            return View(db.Groups.ToList());
        }
           
        // GET: Groups/Details/5
        [Authorize]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Group group = db.Groups.Find(id);
            if (group == null)
            {
                return HttpNotFound();
            }

            ViewBag.isAdmin = false;
            int userId = int.Parse(Session["UserId"].ToString());
            User owner = db.Users.Where(a => a.UserID == group.Owner).FirstOrDefault();
            ViewBag.Owner = owner;

            if (userId == group.Owner)
            {
                ViewBag.isAdmin = true;
            }

            var groupUsers = (from s in db.UserGroups
                              join sa in db.Users on s.User_UserID equals sa.UserID
                              where s.Group_GroupId == id
                              select sa ).ToList();
            if (groupUsers.Count != 0)
            {
                ViewBag.groupUsers = groupUsers;
            }

            return View(group);
        }

        // GET: Groups/Create
        [Authorize]
        public ActionResult Create()
        {
            return View();
        }

        // POST: Groups/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult Create([Bind(Include = "GroupID,GroupName")] Group group)
        {
            if (ModelState.IsValid)
            {
                int userId = int.Parse(Session["UserId"].ToString());
                group.Owner = userId;
                group.GroupDate = DateTime.Today;
                db.Groups.Add(group);
                db.SaveChanges();
                UserGroups userG = new UserGroups();
                userG.Group_GroupId = group.GroupID;
                userG.User_UserID = userId;
                db.UserGroups.Add(userG);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(group);
        }

        // GET: Groups/Edit/5
        [Authorize]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Group group = db.Groups.Find(id);
            if (group == null)
            {
                return HttpNotFound();
            }
            return View(group);
        }

        // POST: Groups/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult Edit([Bind(Include = "Id,GroupName,GroupDate")] Group group)
        {
            if (ModelState.IsValid)
            {
                db.Entry(group).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(group);
        }

        // GET: Groups/Delete/5
        [Authorize]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Group group = db.Groups.Find(id);
            if (group == null)
            {
                return HttpNotFound();
            }
            return View(group);
        }

        // POST: Groups/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult DeleteConfirmed(int id)
        {
            Group group = db.Groups.Find(id);
            db.Groups.Remove(group);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        public ActionResult VistaParcial()
        {
            var group = db.Groups.ToList<Group>();
            return View("_GruposPV", group);
        }
    }
}
