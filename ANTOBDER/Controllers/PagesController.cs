using ANTOBDER.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ANTOBDER.Controllers
{
    [Authorize]
    public class PagesController : Controller
    {
        public ActionResult Create(string id)
        {
            return View();
        }

        public ActionResult Rename(string id, string name)
        {
            return View();
        }
        bool Exists(string id)
        {
            return System.IO.File.Exists(Path(id));
        }
        string Path(string id)
        {
            return _Extentions.PagesRootPath() + "\\" + id.Replace(".html", "") + ".html";
        }
        string ReadFile(string id)
        {
            return System.IO.File.ReadAllText(Path(id));
        }
        public ActionResult Edit(string id)
        {
            if (!Exists(id))
            {
                return HttpNotFound();
            }
            var file = ReadFile(id);
            return View(new Tuple<string, string>(id.ToTitleCase(), file));
        }
        [HttpPost]
        public ActionResult Edit(string id, ChangeDynamicPageViewModel model)
        {
            if (!Exists(id))
            {
                return HttpNotFound();
            }
            WriteFile(id, model.HtmlBody);
            TempData["Message"] = "Değişiklikler kaydedildi!";
            return RedirectToAction("Edit", "Pages", new { id=id});
        }

        private void WriteFile(string id, string htmlBody)
        {
            System.IO.File.WriteAllText(Path(id), htmlBody);
        }

        public ActionResult Delete(string id)
        {
            if (!Exists(id))
            {
                return HttpNotFound();
            }
            //System.IO.File.Delete(_Extentions.PagesRootPath() + "\\" + id + ".html");
            return View();
        }
    }
}