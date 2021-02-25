using ANTOBDER.App_Start;
using ANTOBDER.Models;
using ANTOBDER.Models.EF_MODELS;
using ANTOBDER.Modules;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ANTOBDER.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            List<Content> faaliyetler;
            using (var db = new EF_CONTEXT())
            {
                var settingEnum = SettingsENUM.HOMEPAGE_SHOW_CONTENT_LIMITATION.ToString();
                var setting = db.SiteSettings.FirstOrDefault(s => s.ENUM == settingEnum);
                if (setting == null)
                {
                    setting = new SiteSetting() { Value = 24.ToString() };
                }

                var limitation = int.Parse(setting.Value);
                faaliyetler = db.Contents
                    .Where(e => !e.CID.EndsWith("e"))

                    .OrderByDescending(e => e.On)
                    .Take(limitation)
                    .ToList();

            }
            return View(new IndexViewModel()
            {
                Events = faaliyetler

            });
        }
        public ActionResult ShowPage(string id)
        {
            DynamicHTMLPage dPage = new DynamicHTMLPage();
            //using (var db = new EF_CONTEXT())
            //{
            //    page = db.DynamicHTMLPages.ToList().FirstOrDefault(e=>e.PageName==id);
            //    if (page==null)
            //    {
            //    return HttpNotFound();

            //    }
            //    return View(page);
            //}
            var possiblePath = _Extentions.PagesRootPath() + "\\" + id + ".html";
            if (!System.IO.File.Exists(possiblePath))
            {
                return HttpNotFound();
            }
            var page = System.IO.File.ReadAllText(possiblePath);
            dPage.PageName = id;
            dPage.RawHTML = page;
            return View(dPage);
        }

        public ActionResult GetLegalDocument()
        {
            byte[] fileBytes = System.IO.File.ReadAllBytes(_Extentions.GetRootDirectory() + "\\tuzuk.docx");
            string fileName = "Antobder-Tuzuk.docx";
            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
        }

        public ActionResult UnderConstruction()
        {
            return View();
        }

    }
}