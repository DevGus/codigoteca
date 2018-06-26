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
            return View(db.Invitations.ToList());
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
        public ActionResult Create([Bind(Include = "Invite,InvitationGroup")] Invitation invitation)
        {
            if (ModelState.IsValid)
            {
                if (!userExists(invitation.Invite))
                {
                    ModelState.AddModelError("EmailNotExist", "Este mail no se encuentra registrado");
                    return RedirectToAction("../Groups/Details/"+invitation.InvitationGroup);
                }
                if (userMember(invitation))
                {
                    ModelState.AddModelError("MemberExist", "El usuario ya es miembro del grupo");
                    return RedirectToAction("../Groups/Details/" + invitation.InvitationGroup);
                }
                #region Generate Hash Code 
                invitation.InvitationHash = CreateInvitationHash();
                #endregion
                db.Invitations.Add(invitation);
                db.SaveChanges();
                SendInvitationLinkEmail(invitation.Invite, invitation.InvitationHash);
                return RedirectToAction("Index", "Groups");
            }

            return View(invitation);
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
                    db.Entry(user).Property(x => x.EmailVerified).IsModified = true;*/
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

            /*ViewBag.Message = Message;
            ViewBag.Status = Status;*/
            return View("../Index/Index");
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
        }
    }
}
