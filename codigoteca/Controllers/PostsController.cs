using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using codigoteca;
using codigoteca.Models;

namespace codigoteca.Controllers
{
    public class PostsController : Controller
    {
        private CodigotecaDBContext db = new CodigotecaDBContext();

        // GET: Posts
        public ActionResult Index()
        {
            return View(db.Posts.ToList());
        }

        // GET: Posts/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Post post = db.Posts.Find(id);
            if (post == null)
            {
                return HttpNotFound();
            }
            return View(post);
        }

        // GET: Posts/Create
        [Authorize]
        public ActionResult Create()
        {
            int id = int.Parse(Session["UserId"].ToString());
            var groupUsers = (from s in db.Groups
                        join sa in db.UserGroups on s.GroupID equals sa.Group_GroupId
                        where sa.User_UserID == id
                        select s).ToList();
            if (groupUsers.Count != 0)
            {
                ViewBag.groupUsers = groupUsers;
            }
            return View();
        }

        // POST: Posts/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "PostName,PostDescrip,PostBody,PostLanguage")] Post post, int[] groups, int visibility, string[] tags)
        {
            /*int id = int.Parse(Session["UserId"].ToString());
            User user = new User();
            user.UserID = id;*/

            if (ModelState.IsValid)
            {
                post.PostDate = DateTime.Today;
//                post.PostOwner = user.UserID;
                post.PostVisibility = visibility;
                /*if (tags != null)
                {
                    List<string> tagList = new List<string>();
                    foreach (string tag in tags)
                    {
                        tagList.Add(tag);
                    }
                    post.PostLabels = tagList;
                }*/
                db.Posts.Add(post);
                db.SaveChanges();
                if (visibility == 1 && groups != null)
                {
                    PostGroups pg = new PostGroups();
                    foreach (var  group in groups)
                    {
                        pg.Post_PostId = post.PostId;
                        pg.Group_GroupID = group;
                        db.PostGroups.Add(pg);
                        db.SaveChanges();
                    }
                }
                ViewBag.Status = true;
                ViewBag.Message = "El post fue creado con éxito!";
                return View("Index");
            }

            return View(post);
        }

        // GET: Posts/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Post post = db.Posts.Find(id);
            if (post == null)
            {
                return HttpNotFound();
            }
            return View(post);
        }

        // POST: Posts/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "PostId,PostName,PostDescrip,PostBody,PostDate,PostVisibility,PostLanguage")] Post post)
        {
            if (ModelState.IsValid)
            {
                db.Entry(post).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(post);
        }

        // GET: Posts/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Post post = db.Posts.Find(id);
            if (post == null)
            {
                return HttpNotFound();
            }
            return View(post);
        }

        // POST: Posts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Post post = db.Posts.Find(id);
            db.Posts.Remove(post);
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
            var post = db.Posts.ToList();
            return View("_PostsPV", post);
        }
    }
}
