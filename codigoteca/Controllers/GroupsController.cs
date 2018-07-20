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
                createSendInvitations(invited, group.GroupID);
                
                TempData["Status"] = true;
                TempData["Message"] = "Grupo Creado correctamente!";
                if (invited != null && invited.Length != 0)
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
            if (userId == group.Owner)
            {
                ViewBag.isAdmin = true;
                var groupUsers = (from s in db.UserGroups
                                  join sa in db.Users on s.User_UserID equals sa.UserID
                                  where s.Group_GroupId == id
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
        public ActionResult Edit([Bind(Include = "Id,GroupName,GroupDate")] Group group, String id, String[] invited)
        {
            if (ModelState.IsValid)
            {
                Group g = db.Groups.Find(int.Parse(id));
                g.GroupName = group.GroupName;
                
                db.Entry(g).State = EntityState.Modified;
                db.SaveChanges();

                createSendInvitations(invited, int.Parse(id));

                TempData["Status"] = true;
                TempData["Message"] = "Grupo actualizado correctamente!";
                if (invited != null && invited.Length != 0)
                {
                    TempData["Message"] += " Hemos enviado las invitaciones a los usuarios ingresados";
                }

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
            int id = int.Parse(Session["UserId"].ToString());
            var groupUsers = (from s in db.UserGroups
                              join sa in db.Groups on s.Group_GroupId equals  sa.GroupID 
                              where s.User_UserID == id
                              select sa).ToList();
            ViewBag.groupUsers = groupUsers;
            return View("_GruposPV");
        }

        public void createSendInvitations(String[] mails, int groupId)
        {
            Invitation inv = new Invitation();
            if (mails != null)
            {
                foreach (String mail in mails)
                {
                    inv.InvitationGroup = groupId;
                    inv.InvitationHash = CreateActivationKey();
                    inv.Invite = mail;
                    inv.From = Session["UserMail"].ToString();
                    db.Invitations.Add(inv);
                    db.SaveChanges();

                    sendInvitationMail(inv);
                }
            }
        }

        [NonAction]
        private string CreateActivationKey()
        {
            var activationKey = Guid.NewGuid().ToString();

            if (db.Invitations.Any(key => key.InvitationHash == activationKey))
            {
                activationKey = CreateActivationKey();
            }

            return activationKey;
        }

        private void sendInvitationMail(Invitation inv)
        {
            var verifyUrl = "/Groups/acceptInvitation/" + inv.InvitationHash;
            var link = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, verifyUrl);

            var fromEmail = new MailAddress("codigoteca.net@gmail.com", "Codigoteca");
            var toEmail = new MailAddress(inv.Invite);
            var fromEmailPassword = "codigoteca123"; // Replace with actual password
            string subject = "Invitación a unirse a un grupo!";

            string body =
                "<html xmlns='http://www.w3.org/1999/xhtml'> <head> <meta http-equiv='Content-Type' content='text/html; charset=utf-8' /> <title>Invitación para un grupo</title> </head> <body> <table width='600' border='0' align='center' cellpadding='0' cellspacing='0'> <tr> <td align='center' valign='top' bgcolor='#f2f2f2' style='background-color:#f2f2f2; font-family:Arial, Helvetica, sans-serif; font-size:13px; color:#212121; padding:10px; border: 2px solid #f2f2f2'><table width='100%' border='0' cellspacing='0' cellpadding='0' style='margin-top:10px;'> <tr> <td align='left' valign='top' style='font-family:Arial, Helvetica, sans-serif; font-size:13px; color:#212121;'> <div style='font-size:28px;'> <<Codigoteca>> Nueva invitación</div><br> <div> <br> <p>Para aceptar la invitación, haga clic en el siguiente <a href='" + link + "'> link </a>. <br> Si no tenes una cuenta creada podes crearte una y nirte </p> </div> </td> </tr> </table></td> </tr> <tr> <td align='left' valign='top' bgcolor='#ffffff' style='background-color:#ffffff;'><table width='100%' border='0' cellspacing='0' cellpadding='15'> <tr> <td align='left' valign='top' style='color:#212121; font-family:Arial, Helvetica, sans-serif; font-size:13px;'> CODIGOTECA<br> </td> </tr> </table></td> </tr> </table> </body> </html>";

            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 25,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromEmail.Address, fromEmailPassword)
            };

            using (var message = new MailMessage(fromEmail, toEmail)
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            })
            smtp.Send(message);
        }

        [HttpPost]
        public ActionResult validateInvitation(String mail, Boolean newGroup = true, int GroupID = -1)
        {
            /*Email vacio*/
            if (mail == null)
            {
                return Json(new { success = false, responseText = "Debes ingresar un mail" }, JsonRequestBehavior.AllowGet);
            }
            if (mail.Trim().Length == 0)
            {
                return Json(new { success = false, responseText = "Debes ingresar un mail" }, JsonRequestBehavior.AllowGet);
            }
            
            /* email invalido*/
            if (!System.Text.RegularExpressions.Regex.IsMatch(mail,
                @"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
                @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-0-9a-z]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250)))
            {
                return Json(new { success = false, responseText = "El formato ingresado no es válido" }, JsonRequestBehavior.AllowGet);
            }

            /* email de su usario*/
            User actualUser = db.Users.Find(int.Parse(Session["UserId"].ToString()));
            if (actualUser.UserMail == mail)
            {
                return Json(new { success = false, responseText = "Tss, No podes autoinvitarte!!" }, JsonRequestBehavior.AllowGet);
            }
            if (!newGroup)
            {
                /*Aca esta actualizando el grupo*/
                /*Tengo que validar que no este invitando a alguien que ya esta en el grupo*/
                var groupUsers = (from s in db.UserGroups
                                  join sa in db.Users on s.User_UserID equals sa.UserID
                                  where( s.Group_GroupId == GroupID && sa.UserMail == mail)
                                  select sa).ToList();
                if (groupUsers.Count != 0)
                {
                    return Json(new { success = false, responseText = "El usuario ingresado ya es integrante de este grupo" }, JsonRequestBehavior.AllowGet);
                }

            }
            /* Valido*/
            return Json(new { success = true } , JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public ActionResult acceptInvitation (string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException(nameof(id));
            }
            Invitation inv = db.Invitations.Where(a => a.InvitationHash == id).FirstOrDefault();
            User u = db.Users.Find(int.Parse(Session["UserId"].ToString()));
            if (inv != null && inv.InvitationHash == id && u.UserMail== inv.Invite )
            {
                /*Invitacion aceptada*/
                UserGroups ug = new UserGroups();
                ug.User_UserID = u.UserID;
                ug.Group_GroupId = inv.InvitationGroup;
                db.UserGroups.Add(ug);
                db.Invitations.Remove(inv);
                db.SaveChanges();
                TempData["Status"] = true;
                TempData["Message"] = "Invitación aceptada con éxito";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["Status"] = false;
                TempData["Message"] = "Hubo un error al aceptar la invitación";
                return RedirectToAction("Index");
            }
        }
    }
}
