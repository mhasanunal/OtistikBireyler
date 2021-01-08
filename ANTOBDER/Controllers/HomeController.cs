﻿using ANTOBDER.App_Start;
using ANTOBDER.Models;
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
            List<BaseContent> faaliyetler;
            using (var db = new ContextBase())
            {
                faaliyetler = db.Contents
                    .Where(e => !e.IsEditorial)
                    .OrderByDescending(e => e.On)
                    .Take(6)
                    .ToList();

            }
            return View(new IndexViewModel()
            {
                Events = faaliyetler

            });
        }
        public ActionResult ShowPage(string id)
        {
            var possiblePath = _Extentions.PagesRootPath() + "\\" + id + ".html";
            if (!System.IO.File.Exists(possiblePath))
            {
                return HttpNotFound();
            }
            var page = System.IO.File.ReadAllText(possiblePath);
            return View(new Tuple<string, string>(id.ToTitleCase(), page));
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