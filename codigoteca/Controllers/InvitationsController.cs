using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using codigoteca;
using codigoteca.Models;

namespace codigoteca.Controllers
{
    public class InvitationsController : Controller
    {
        private CodigotecaDBContext db = new CodigotecaDBContext();

        // GET: Invitations
        public ActionResult Index()
        {
            #region tempData
            if (TempData["Status"] != null)
            {
                ViewBag.Status = TempData["Status"];
                ViewBag.Message = TempData["Message"];
                TempData.Remove("Status");
                TempData.Remove("Message");
            }
            #endregion

            String mail = Session["UserMail"].ToString();
            int id = int.Parse(Session["UserId"].ToString());
            var invs = (from s in db.Invitations
                        join sa in db.Groups on s.InvitationGroup equals sa.GroupID
                        where (s.Invite == mail && s.Status=="pending")
                        select s).ToList();
            if (invs.Count() != 0)
            {
                ViewBag.pendingInvs = invs;
            }

            invs = (from s in db.Invitations
                    join sa in db.Groups on s.InvitationGroup equals sa.GroupID
                    where (s.From == mail && s.Status == "pending")
                    select s).ToList();

            if (invs.Count() != 0)
            {
                ViewBag.myInvs = invs;
            }
            invs = (from s in db.Invitations
                    join sa in db.Groups on s.InvitationGroup equals sa.GroupID
                    where (sa.Owner == id && s.Status == "pending" && s.From != mail)
                    select s).ToList();
            if (invs.Count() != 0)
            {
                ViewBag.groupInvs = invs;
            }
            return View();
        }

        // GET: Invitations/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Invitation invitation = db.Invitations.Find(id);
            if (invitation == null)
            {
                return HttpNotFound();
            }
            return View(invitation);
        }

        // GET: Invitations/Create
        [Authorize]
        public ActionResult Create(int GroupID)
        {
            {
                ViewBag.GroupID = GroupID;
            }
            return View();
        }


        // POST: Invitations/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Invite,InvitationGroup")] Invitation invitation, int GroupId, String[] invited)
        {
            if (ModelState.IsValid)
            {
                int id = int.Parse(Session["UserId"].ToString());
                createSendInvitations(invited, GroupId,id);

                TempData["Status"] = true;
                TempData["Message"] += " Hemos enviado las invitaciones a los usuarios ingresados";
            }

            return RedirectToAction("Index");
        }

        // GET: Invitations/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Invitation invitation = db.Invitations.Find(id);
            if (invitation == null)
            {
                return HttpNotFound();
            }
            return View(invitation);
        }

        // POST: Invitations/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,InvitationHash")] Invitation invitation)
        {
            if (ModelState.IsValid)
            {
                db.Entry(invitation).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(invitation);
        }

        // GET: Invitations/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Invitation invitation = db.Invitations.Find(id);
            if (invitation == null)
            {
                return HttpNotFound();
            }
            return View(invitation);
        }

        // POST: Invitations/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Invitation invitation = db.Invitations.Find(id);
            db.Invitations.Remove(invitation);
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


        [NonAction]
        public bool userExists(string email)
        {
            var v = db.Users.Where(a => a.UserMail == email).FirstOrDefault();
            return v != null;
        }
        [NonAction]
        public bool userMember(Invitation invitation)
        {
            User user = new User();
            user = db.Users.Where(a => a.UserMail == invitation.Invite).FirstOrDefault();
            var v = db.UserGroups.Where(a => a.User_UserID == user.UserID &&
                                             a.Group_GroupId == invitation.InvitationGroup).FirstOrDefault();
            return v != null;
        }


        /*LSC 23/7*/
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
                                  where (s.Group_GroupId == GroupID && sa.UserMail == mail)
                                  select sa).ToList();
                if (groupUsers.Count != 0)
                {
                    return Json(new { success = false, responseText = "El usuario ingresado ya es integrante de este grupo" }, JsonRequestBehavior.AllowGet);
                }
                var isInvited = db.Invitations.Where(a => (a.Invite == mail && a.InvitationGroup == GroupID && a.Status == "pending")).FirstOrDefault();
                if (isInvited != null)
                {
                    return Json(new { success = false, responseText = "El usuario ingresado tiene una invitación pendiente para este grupo" }, JsonRequestBehavior.AllowGet);
                }

            }
            /* Valido*/
            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult accept(int id)
        {
            var inv = db.Invitations.Where(a => a.Id == id).FirstOrDefault();
            User u = db.Users.Find(int.Parse(Session["UserId"].ToString()));
            UserGroups ug = new UserGroups();
            ug.User_UserID = u.UserID;
            ug.Group_GroupId = inv.InvitationGroup;
            db.UserGroups.Add(ug);
            inv.Status = "acepted";
            db.Entry(inv).State = EntityState.Modified;
            /*db.Invitations.Remove(inv);*/
            db.SaveChanges();
            TempData["Status"] = true;
            TempData["Message"] = "Invitación aceptada con éxito";
            return RedirectToAction("Index");
        }

        public ActionResult cancel(int id)
        {
            var inv = db.Invitations.Where(a => a.Id == id).FirstOrDefault();
            inv.Status = "canceled";
            db.Entry(inv).State = EntityState.Modified;
            /*db.Invitations.Remove(inv);*/
            db.SaveChanges();
            TempData["Status"] = true;
            TempData["Message"] = "Invitación cancelada";
            return RedirectToAction("Index");
        }

        public ActionResult reject(int id)
        {
            var inv = db.Invitations.Where(a => a.Id == id).FirstOrDefault();
            inv.Status = "rejected";
            db.Entry(inv).State = EntityState.Modified;
            /*db.Invitations.Remove(inv);*/
            db.SaveChanges();
            TempData["Status"] = true;
            TempData["Message"] = "Invitación rechazada";
            return RedirectToAction("Index");
        }

        [Authorize]
        public ActionResult acceptInvitation(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException(nameof(id));
            }
            Invitation inv = db.Invitations.Where(a => a.InvitationHash == id).FirstOrDefault();
            User u = db.Users.Find(int.Parse(Session["UserId"].ToString()));
            if (inv != null && inv.InvitationHash == id && u.UserMail == inv.Invite)
            {
                /*Invitacion aceptada*/
                return accept(inv.Id);
            }
            else
            {
                TempData["Status"] = false;
                TempData["Message"] = "Hubo un error al aceptar la invitación";
                return RedirectToAction("Index");
            }
        }

        [Authorize]
        public bool createSendInvitations(String[] mails, int groupId, int idUser)
        {

            Invitation inv = new Invitation();
            //int id = int.Parse(Session["UserId"].ToString());
            string usermail = db.Users.Where(a => a.UserID == idUser).FirstOrDefault().UserMail;
            if (mails != null)
            {
                foreach (String mail in mails)
                {
                    inv.Date = DateTime.Today;
                    inv.Status = "pending";
                    inv.InvitationGroup = groupId;
                    inv.InvitationHash = CreateActivationKey();
                    inv.Invite = mail;
                    inv.From = usermail;
                    db.Invitations.Add(inv);
                    db.SaveChanges();

                    sendInvitationMail(inv);
                }
                return true;
            }
            return false;
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
            var link = "http://localhost:8081/Invitations/acceptInvitation/" + inv.InvitationHash;
            //var link = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, verifyUrl);

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


        /**************************
        *       TO DELETE         *
        **************************/
        /*[NonAction]
        private string CreateInvitationHash()
        {
            var activationKey = Guid.NewGuid().ToString();

            var activationKeyAlreadyExists = db.Invitations.Any(key => key.InvitationHash == activationKey);

            if (activationKeyAlreadyExists)
            {
                activationKey = CreateInvitationHash();
            }

            return activationKey;
        }

        public ActionResult ConfirmInvitation(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            var Message = "";
            var Status = false;
            Invitation invitation = new Invitation();
            invitation = db.Invitations.Where(a => a.InvitationHash == id).FirstOrDefault();
            if (invitation != null)
            {
                User user = new User();
                user = db.Users.Where(a => a.UserMail == invitation.Invite).FirstOrDefault();
                UserGroups usergroup = new UserGroups();
                usergroup.Group_GroupId = invitation.InvitationGroup;
                usergroup.User_UserID = user.UserID;
                try
                {
                    db.UserGroups.Add(usergroup);
                    db.Configuration.ValidateOnSaveEnabled = false;
                    /*db.Users.Attach(user);
                    db.Entry(user).Property(x => x.EmailVerified).IsModified = true;
        db.SaveChanges();
                }
                catch (DbEntityValidationException e)
                {
                    throw;
                }

                Message = "Se te agrego al grupo";
                Status = true;
            }
            else
            {
                Message = "Hubo un error al vincularte al grupo";
                Status = false;
            }

            
            return View("../Index/Index");
        }

        [NonAction]
        public void SendInvitationLinkEmail(string emailID, string InvitationHash)
        {
            var verifyUrl = "/Invitations/ConfirmInvitation/" + InvitationHash;
            var link = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, verifyUrl);

            var fromEmail = new MailAddress("codigoteca.net@gmail.com", "Codigoteca");
            var toEmail = new MailAddress(emailID);
            var fromEmailPassword = "codigoteca123"; // Replace with actual password
            string subject = "Group Invitation";

            string body =
                "<html xmlns='http://www.w3.org/1999/xhtml'> <head> <meta http-equiv='Content-Type' content='text/html; charset=utf-8' /> <title>contacto</title> </head> <body> <table width='600' border='0' align='center' cellpadding='0' cellspacing='0'> <tr> <td align='center' valign='top' bgcolor='#f2f2f2' style='background-color:#f2f2f2; font-family:Arial, Helvetica, sans-serif; font-size:13px; color:#212121; padding:10px; border: 2px solid #f2f2f2'><table width='100%' border='0' cellspacing='0' cellpadding='0' style='margin-top:10px;'> <tr> <td align='left' valign='top' style='font-family:Arial, Helvetica, sans-serif; font-size:13px; color:#212121;'> <div style='font-size:28px;'> Group Invitation | Codigoteca </div><br> <div> <br> <h1>Te han invitado a un grupo!</h1> <p>Para aceptar la invitacion, haga clic en el siguiente <a href='" + link + "'> link </a> </p> </div> </td> </tr> </table></td> </tr> <tr> <td align='left' valign='top' bgcolor='#ffffff' style='background-color:#ffffff;'><table width='100%' border='0' cellspacing='0' cellpadding='15'> <tr> <td align='left' valign='top' style='color:#212121; font-family:Arial, Helvetica, sans-serif; font-size:13px;'><br> </td> </tr> </table></td> </tr> </table> </body> </html>";

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
        }*/
    }
}
