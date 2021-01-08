using ANTOBDER.App_Start;
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
        }
    }
}
