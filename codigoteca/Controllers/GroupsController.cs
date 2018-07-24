using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Mail;
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
            if(TempData["Status"] != null)
            {
                ViewBag.Status = TempData["Status"];
                ViewBag.Message = TempData["Message"];
                TempData.Remove("Status");
                TempData.Remove("Message");
            }
            int id = int.Parse(Session["UserId"].ToString());
            var groups = (from s in db.UserGroups
                              join sa in db.Groups on s.Group_GroupId equals sa.GroupID
                              where s.User_UserID == id
                              select sa).ToList();
            ViewBag.groups = groups;
            ViewBag.userId = int.Parse(Session["UserId"].ToString());
            ViewBag.isAdmin = bool.Parse(Session["isAdmin"].ToString());

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
            var postsGroup = (from s in db.PostGroups
                              join sa in db.Posts on s.Post_PostId equals sa.PostId
                              where s.Group_GroupID == id
                              select sa).ToList();

            if (groupUsers.Count != 0)
            {
                ViewBag.postsGroup = postsGroup;
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
        public ActionResult Create([Bind(Include = "GroupID,GroupName")] Group group, String[] invited)
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
                
                TempData["Status"] = true;
                TempData["Message"] = "Grupo Creado correctamente!";
                if (new InvitationsController().createSendInvitations(invited, group.GroupID))
                {
                    TempData["Message"] += " Hemos enviado las invitaciones a los usuarios ingresados";
                }
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

            int userId = int.Parse(Session["UserId"].ToString());
            User owner = db.Users.Where(a => a.UserID == group.Owner).FirstOrDefault();
            ViewBag.Owner = owner;
            ViewBag.isAdmin = false;
            ViewBag.groupId = id;
            if (userId == group.Owner)
            {
                ViewBag.isAdmin = true;
                var groupUsers = (from s in db.UserGroups
                                  join sa in db.Users on s.User_UserID equals sa.UserID
                                  where (s.Group_GroupId == id && s.User_UserID != userId)
                                  select sa).ToList();
                ViewBag.groupUsers = groupUsers;
            }

            return View(group);
        }

        // POST: Groups/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult Edit([Bind(Include = "Id,GroupName,GroupDate")] Group group, String id)
        {
            if (ModelState.IsValid)
            {
                Group g = db.Groups.Find(int.Parse(id));
                g.GroupName = group.GroupName;
                
                db.Entry(g).State = EntityState.Modified;
                db.SaveChanges();

                /*createSendInvitations(invited, int.Parse(id)); Lo pasé a invitations*/

                TempData["Status"] = true;
                TempData["Message"] = "Grupo actualizado correctamente!";

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
            db.UserGroups.Where(a => a.Group_GroupId == id).ToList().ForEach(b => db.UserGroups.Remove(b));
            db.PostGroups.Where(a => a.Group_GroupID == id).ToList().ForEach(b => db.PostGroups.Remove(b));
            db.Invitations.Where(a => a.InvitationGroup == id).ToList().ForEach(b => db.Invitations.Remove(b));
            
            Group group = db.Groups.Find(id);
            db.Groups.Remove(group);
            db.SaveChanges();

            TempData["Status"] = true;
            TempData["Message"] = "Grupo eliminado correctamente";

            return RedirectToAction("Index");
        }

        public ActionResult exitGroup(int groupId)
        {
            int userId = int.Parse(Session["UserId"].ToString());
            UserGroups ug = db.UserGroups.Where(a => a.Group_GroupId == groupId && a.User_UserID == userId).FirstOrDefault();
            db.UserGroups.Remove(ug);
            db.SaveChanges();
            Group g = db.Groups.Find(groupId);
            String groupName = g.GroupName;
            if (g.Owner == userId)
            {
                /*Cambio el owner del grupo, estoy saliendome como administrador*/
                var users = db.UserGroups.Where(a => a.Group_GroupId == groupId).ToList();
                if(users.Count() == 0)
                {
                    /*borrar grupo*/
                    return DeleteConfirmed(groupId);
                }
                else
                {
                    int newOwner = users[0].User_UserID;
                    g.Owner = newOwner;
                    db.Entry(g).State = EntityState.Modified;
                }
            }
            db.SaveChanges();
            TempData["Status"] = true;
            TempData["Message"] = "Has salido del grupo " + groupName;
            return RedirectToAction("Index");
        }

        public ActionResult deleteUser(int groupId, int userId)
        {
            db.UserGroups.Remove(db.UserGroups.Where(a => a.Group_GroupId == groupId && a.User_UserID == userId).FirstOrDefault());
            TempData["Status"] = true;
            TempData["Message"] = "Usuario eliminado";
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

            ViewBag.isAdmin = bool.Parse(Session["isAdmin"].ToString());
            int id = int.Parse(Session["UserId"].ToString());
            var groupUsers = (from s in db.UserGroups
                              join sa in db.Groups on s.Group_GroupId equals  sa.GroupID 
                              where s.User_UserID == id
                              select sa).ToList();
            ViewBag.groupUsers = groupUsers;
            var groups = (from s in db.UserGroups
                          join sa in db.Groups on s.Group_GroupId equals sa.GroupID
                          where s.User_UserID == id
                          select sa).ToList();
            ViewBag.groups = groups;
            ViewBag.userId = int.Parse(Session["UserId"].ToString());

            return View("_GruposPV");
        }

        public JsonResult BuscarPersonas(string term)
        {
            using (CodigotecaDBContext db = new CodigotecaDBContext())
            {
                var resultado = db.Users.Where(x => x.UserMail.Contains(term))
                    .Select(x => x.UserMail).Take(5).ToList();
                return Json(resultado, JsonRequestBehavior.AllowGet);
            }
        }
    }
}
