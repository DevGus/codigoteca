using codigoteca.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace codigoteca.Controllers
{   
    public class AuthController : Controller
    {
        private CodigotecaDBContext db = new CodigotecaDBContext();

        // GET: Auth
        public ActionResult Index(){
            return View("Logon");
        }

        // POST: login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login([Bind(Include = "UserMail,UserPass")] User loginUser, string returnUrl)
        {
            User user = db.Users.Where(a => a.UserMail == loginUser.UserMail).FirstOrDefault();
            if (user != null && user.Decrypt(user.UserHash).Equals(loginUser.UserPass)){
                if (user.EmailVerified == 0)
                {
                    ViewBag.Message = "Aún no has verificado tu correo electrónico";
                    ViewBag.Status = false;
                    return View("Logon");
                }
                /*Usuario logueado*/
                FormsAuthentication.SetAuthCookie(user.UserID.ToString(), false);

                Session["UserId"] = user.UserID;
                Session["UserName"] = user.UserName.ToString();
                Session["UserMail"] = user.UserMail.ToString();
                if (Url.IsLocalUrl(returnUrl) && returnUrl.Length > 1 && returnUrl.StartsWith("/")
                    && !returnUrl.StartsWith("//") && !returnUrl.StartsWith("/\\")){
                    return Redirect(returnUrl);
                } else {
                    return RedirectToAction("../Index/Index");
                }
            } else {
                /*Usuario Incorrecto*/
                ModelState.AddModelError("IncorrectLogin", "Los datos ingresados son incorrectos");
                return View("Logon");
            }
        }

        public ActionResult LogOut()
        {
            FormsAuthentication.SignOut();
            Session.Abandon();
            return RedirectToAction("Index");
        }

        public ActionResult Register()
        {
            return View("Register");
        }

        // POST: register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register([Bind(Include = "UserName,UserLastName,UserAge,UserMail,UserPass,ConfirmPassword")] User user){
            if (ModelState.IsValid){
                if (userExists(user.UserMail)){
                    ModelState.AddModelError("EmailExist", "Este mail ya se encuentra registrado");
                    return View("Register", user);
                }
                #region Generate Activation Code 
                user.ActivationCode = CreateActivationKey();
                #endregion
                
                user.UserHash = user.Encrypt(user.UserPass);
                db.Users.Add(user);
                db.SaveChanges();
                SendVerificationLinkEmail(user.UserMail, user.ActivationCode.ToString());
                ViewBag.Status = true;
                ViewBag.Message = "La cuenta fue creada con éxito. Se ha enviado un mail para confirmar el registro de usuario a: " +
                user.UserMail;
                return View("Logon");
            }
            ViewBag.Message = "Invalid request";
            ViewBag.Status = false;
            return View("Logon", user);
        }


        [NonAction]
        public bool userExists(string email)
        {
            var v = db.Users.Where(a => a.UserMail == email).FirstOrDefault();
            return v != null;
        }

        [NonAction]
        private string CreateActivationKey(){
            var activationKey = Guid.NewGuid().ToString();

            var activationKeyAlreadyExists = db.Users.Any(key => key.ActivationCode == activationKey);

            if (activationKeyAlreadyExists){
                activationKey = CreateActivationKey();
            }

            return activationKey;
        }

        [NonAction]
        public void SendVerificationLinkEmail(string emailID, string activationCode)
        {
            var verifyUrl = "/Users/VerifyAccount/" + activationCode;
            var link = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, verifyUrl);

            var fromEmail = new MailAddress("codigoteca.net@gmail.com", "Codigoteca");
            var toEmail = new MailAddress(emailID);
            var fromEmailPassword = "codigoteca123"; // Replace with actual password
            string subject = "Cuenta creada!";

            string body =
                "<html xmlns='http://www.w3.org/1999/xhtml'> <head> <meta http-equiv='Content-Type' content='text/html; charset=utf-8' /> <title>contacto</title> </head> <body> <table width='600' border='0' align='center' cellpadding='0' cellspacing='0'> <tr> <td align='center' valign='top' bgcolor='#f2f2f2' style='background-color:#f2f2f2; font-family:Arial, Helvetica, sans-serif; font-size:13px; color:#212121; padding:10px; border: 2px solid #f2f2f2'><table width='100%' border='0' cellspacing='0' cellpadding='0' style='margin-top:10px;'> <tr> <td align='left' valign='top' style='font-family:Arial, Helvetica, sans-serif; font-size:13px; color:#212121;'> <div style='font-size:28px;'> Creación de cuenta | Codigoteca </div><br> <div> <br> <h1>Cuenta creada con éxito!</h1> <p>Para confirmar el mail, haga clic en el siguiente <a href='" + link + "'> link </a> </p> </div> </td> </tr> </table></td> </tr> <tr> <td align='left' valign='top' bgcolor='#ffffff' style='background-color:#ffffff;'><table width='100%' border='0' cellspacing='0' cellpadding='15'> <tr> <td align='left' valign='top' style='color:#212121; font-family:Arial, Helvetica, sans-serif; font-size:13px;'> CODIGOTECA<br> </td> </tr> </table></td> </tr> </table> </body> </html>";

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