using ANTOBDER.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ANTOBDER.Models.EF_MODELS;
using System.Net.Mime;
using System.IO;

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
            Models.EF_MODELS.DynamicHTMLPage page = new DynamicHTMLPage();
            page.RawHTML = ReadFile(id);
            page.PageName = id;


            return View(page);
        }
        [HttpPost]
        public ActionResult Edit(string id, ChangeDynamicPageViewModel model)
        {
            DynamicHTMLPage page = new DynamicHTMLPage();
            if (!Exists(id))
            {
                return HttpNotFound();
            }
            page.PageName = id;
            WriteFile(id, model.HtmlBody);
            TempData["Message"] = "Değişiklikler kaydedildi!";
            return RedirectToAction("Edit", "Pages", new { id = id });
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

        public ActionResult Download(string id)
        {
            if (!Exists(id))
            {
                return HttpNotFound();
            }

            var cd = new ContentDisposition("attachment")
            {
                FileName = $"{id}.html",
                Inline = false
            };
            Response.AppendHeader("Content-Disposition", cd.ToString());

            byte[] file = System.IO.File.ReadAllBytes(Path(id));

            return File(file, "text/html");
        }
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Upload(string id, HttpPostedFileWrapper file)
        {
            id = id?.Replace(".html", "");
            if (!Exists(id))
            {
                return HttpNotFound();
            }
            using (var mem = new MemoryStream())
            {
                file.InputStream.CopyTo(mem);
                var path = Path(id);

                System.IO.File.WriteAllBytes(path, mem.ToArray());

            }
            TempData["Message"] = id + " Dosyası yüklendi ve güncellendi!";
            return RedirectToAction("Edit", "Pages", new { id = id });
        }
    }
}