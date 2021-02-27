using ANTOBDER.App_Start;
using ANTOBDER.Models.EF_MODELS;
using Microsoft.AspNet.Identity;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

[assembly: OwinStartup(typeof(ANTOBDER.MvcApplication))]
namespace ANTOBDER
{
    public class MvcApplication : System.Web.HttpApplication
    {

        public void Configuration(IAppBuilder builder)
        {
            builder.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                CookieHttpOnly = true,
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                LoginPath = new PathString("/Enter/Login"),
                CookiePath = "/",
                CookieName = "_ReservedLogin",
                
                //Provider = new CookieAuthenticationProvider
                //{
                //    OnApplyRedirect = ctx =>
                //    {
                //        if (!IsAjaxRequest(ctx.Request))
                //        {
                //            ctx.Response.Redirect(ctx.RedirectUri);
                //        }
                //    }
                //}
            });
            
        }
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            _Extentions.CreateRootPaths();

            Seed();
        }

        private void Seed()
        {
            var context = new EF_CONTEXT();

            IList<User> defaultStandards = new List<User>();

            defaultStandards.Add(new User() { Username = "ercan", Password = "D029F3619114D1B1D6FA25D4E23568E0", Fullname = "Ercan Bal", Role = "ADMIN", IsActive = true });
            defaultStandards.Add(new User() { Username = "peri", Password = "63AD31547C1159605584614E08C4F450", Fullname = "Perihan Şahin Bal", Role = "ADMIN", IsActive = true });
            defaultStandards.Add(new User() { Username = "admin", Password = "cd95367beef3e8ab84842817b6ab9ea3", Fullname = "Admin", Role = "ADMIN", IsActive = true });
            defaultStandards.Add(new User() { Username = "hasan", Password = "356D27C9AB6A16490B0BC8F6B3A9D3A6", Fullname = "Hasan Ünal", Role = "SUPER", IsActive = true });
            foreach (var item in defaultStandards)
            {
                if (context.Users.FirstOrDefault(c => c.Username == item.Username) == null)
                {
                    context.Users.Add(item);

                }
            }

            IList<SiteSetting> settings = new List<SiteSetting>();
            settings.Add(new SiteSetting { ENUM = SettingsENUM.HOMEPAGE_SHOW_CONTENT_LIMITATION.ToString(), DataTypeENUM = DataTypeENUM.INT.ToString(), Value = 25.ToString() });
            settings.Add(new SiteSetting { ENUM = SettingsENUM.IMAGE_COMPRESSION_QUALITY.ToString(), DataTypeENUM = DataTypeENUM.INT.ToString(), Value = 55.ToString() });
            settings.Add(new SiteSetting { ENUM = SettingsENUM.IMAGE_COMPRESSION_MAXSUMOFWIDTHANDHEIGHT.ToString(), DataTypeENUM = DataTypeENUM.INT.ToString(), Value = 3000.ToString() });
            foreach (var item in settings)
            {
                if (context.SiteSettings.FirstOrDefault(c => c.ENUM == item.ENUM) == null)
                {
                    context.SiteSettings.Add(item);

                }
            }
            context.SaveChanges();
        }
    }
}
