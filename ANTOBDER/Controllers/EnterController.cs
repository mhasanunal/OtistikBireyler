using ANTOBDER.App_Start;
using ANTOBDER.Models;
using ANTOBDER.Modules;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;

namespace ANTOBDER.Controllers
{
    public class EnterController : Controller
    {

        [Authorize]
        public ActionResult Index()
        {
            return View();
        }
        [Authorize]
        public ActionResult AddContent()
        {
            return View();
        }
        public ActionResult ListDynamicPages()
        {
            var list = _Extentions.GetDynamicPages().Select(p => Path.GetFileName(p));
            return View(list);
        }

        void SendMail()
        {
            SmtpClient SmtpServer = new SmtpClient("smtp.live.com");
            var mail = new MailMessage();
            mail.From = new MailAddress("antobder@hotmail.com");
            mail.To.Add("antobder@hotmail.com");
            mail.Subject = "Test Mail - 1";
            mail.IsBodyHtml = true;
            string htmlBody;
            htmlBody = "Write some HTML code here";
            mail.Body = htmlBody;
            SmtpServer.Port = 587;
            SmtpServer.UseDefaultCredentials = false;
            SmtpServer.Credentials = new System.Net.NetworkCredential("youremail@hotmail.com", "password");
            SmtpServer.EnableSsl = true;
            SmtpServer.Send(mail);
        }

        public ActionResult Login(string returnUrl)
        {
            HttpContext.GetOwinContext().Authentication.SignOut();
            TempData["ReturnUrl"] = returnUrl;
            return View();
        }
        [HttpPost]
        public ActionResult Login(LoginInfo info, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "Kullanıcı adı veya Şifre girilmedi!");
                return View(info);
            }
            if (!CheckLogin(info))
            {
                ModelState.AddModelError("", "Kullanıcı adı veya Şifre hatalı!");
                return View(info);
            }
            var user = GetUser(info.Username);
            Authenticate(
                user,
                info.RememberMe);


            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index");
        }

        private void Authenticate(User user, bool rememberMe)
        {
            var ident = new ClaimsIdentity(
           new[] {

              new Claim("http://schemas.microsoft.com/accesscontrolservice/2010/07/claims/identityprovider", "ASP.NET Identity", "http://www.w3.org/2001/XMLSchema#string"),
              new Claim(ClaimTypes.NameIdentifier, user.Username),
              new Claim(ClaimTypes.GivenName,user.Fullname),
              new Claim(ClaimTypes.Role,user.Role)
           },
           DefaultAuthenticationTypes.ApplicationCookie); ;

            HttpContext.GetOwinContext().Authentication.SignIn(
               new AuthenticationProperties { IsPersistent = rememberMe }, ident);
        }
        /// <summary>
        /// Returns Username,Role,And MD5 Hashed Password, Null if no such username
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        User GetUser(string username)
        {
            User user;

            using (var db = new ContextBase())
            {
                user = db.Users.FirstOrDefault(f => f.Username == username);

            }
            if (user != null)
            {
                return user;
            }
            return null;
        }


        public ActionResult Logout()
        {
            HttpContext.GetOwinContext().Authentication.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Index");
        }
        bool CheckLogin(LoginInfo info)
        {

            var user = GetUser(info.Username);
            if (user != null)
            {
                var password = user.Password;
                var md5 = _Extentions.CreateMD5(info.Password).ToLower();
                return password.ToLower() == md5.ToLower();
            }
            return false;
        }
    }
}