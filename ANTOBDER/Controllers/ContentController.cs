using ANTOBDER.Models;
using ANTOBDER.Modules;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace ANTOBDER.Controllers
{
    public class ContentController : Controller
    {
        [HttpPost]
        [Authorize]
        public ActionResult Create(ContentCreateModel model)
        {
            var img = model.ImageFile.FileName;
            var path = SaveFiles(model);

            var content = new BaseContent()
            {
                CID = model.CID,
                Author = model.Author.BuildUpAuthorText(),
                Describer = _Extentions.RemoveUnwantedTags(model.HTMLBody, 65),
                Header = model.Header ?? "İçerik Başlığı",
                ImageFile = "header" + Path.GetExtension(model.ImageFile.FileName),
                On = model.AddedOn,
                Path = path,
                Tags = model.Tags,
                HtmlFile = "Index.html"//If you want to change Index.html rendering start changing this.
            };
            CreateMeta(path, content);
            try
            {
                using (var dbContext = new ContextBase())
                {

                    dbContext.Contents.Add(content);
                    dbContext.SaveChanges();
                }
                TempData["Message"] = "İçerik Yaratıldı!";
                TempData["Content"] = content;
            }
            catch (Exception e)
            {
                TempData["Message"] = "Hata Oluştu! Teknik Bilgi: " + e.Message;
            }
            return RedirectToAction("Index", "Enter");
        }
        [Authorize(Roles = "SUPER")]
        public string DeleteAll()
        {
            int deleted;
            using (var dbContext = new ContextBase())
            {
                dbContext.Contents.Clear();
                deleted = dbContext.SaveChanges();

            }
            return $"Done! Deleted {deleted} amount of content!";
        }
        public ActionResult ReadArticle(string id)
        {
            BaseContent content;

            using (var dbContext = new ContextBase())
            {
                content = dbContext.Contents.FirstOrDefault(i => i.CID == id);

                if (!_Extentions.IsIdSafe(id)
                    ||
                    !Directory.Exists(_Extentions.Locate(id))
                    ||
                    content == null)
                {
                    return HttpNotFound();
                }
            }
            //sanity check
            return View(content);
        }


        public ActionResult ListArticle(
            string author,
            string tags,
            bool listAll = false)
        {
            List<BaseContent> result;
            string filter = null;
            var authorFunc = funcByAuthor(author, out string authorFilter);
            var tagsFunc = funcByTags(tags, out string tagFilter);
            using (var db = new ContextBase())
            {
                var filtered = db.Contents.Where(content => authorFunc(content) && tagsFunc(content));
                if (!listAll)
                {
                    filtered = filtered.Where(content => !content.IsEditorial);
                }
                result = filtered.ToList();
            }
            if (!string.IsNullOrEmpty(authorFilter))
            {
                filter = authorFilter;
            }
            if (!string.IsNullOrEmpty(tagFilter))
            {
                filter = string.IsNullOrEmpty(filter) ? tagFilter : filter + ";" + tagFilter;
            }
            return View(new Tuple<string, IEnumerable<BaseContent>>(filter, result));
        }

        private Func<BaseContent, bool> funcByTags(string tags, out string filter)
        {
            filter = string.IsNullOrEmpty(tags) ? null : tags.Split('_').Aggregate("Etiketler: ", (seed, next) => { return seed + next + ", "; });
            return content =>
            ((!string.IsNullOrEmpty(tags)) && tags.Split('_').Any(
                tag =>
                content.Tags.StartsWith(tag)
                ||
                content.Tags.Contains("_" + tag + "_")
                ||
                content.Tags.EndsWith(tag)))
                || string.IsNullOrEmpty(tags);
        }

        private Func<BaseContent, bool> funcByAuthor(string author, out string filter)
        {
            filter = string.IsNullOrEmpty(author) ? null : "Yazar: " + author;
            return c => (!string.IsNullOrEmpty(author) && c.Author == author) || string.IsNullOrEmpty(author);
        }

        [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "ADMIN,SUPER")]
        public ActionResult Delete(string id)
        {

            using (var dbContext = new ContextBase())
            {
                if (_Extentions.IsIdSafe(id) && Directory.Exists(_Extentions.Locate(id)))
                {

                    dbContext.Contents.Remove(dbContext.Contents.FirstOrDefault(f => f.CID == id));
                    dbContext.SaveChanges();
                    dbContext.Dispose();
                    return RedirectToAction("Index", "Home");


                }
            }
            return HttpNotFound();

        }
        #region PRIVATE

        void CreateMeta(string path, BaseContent content)
        {
            var filename = "meta.txt";
            var input = new StringBuilder();
            var coluns = new BaseSet<BaseContent, int>().Columns;
            foreach (var item in coluns)
            {
                input.Append(item + ";");
            }
            input.AppendLine();
            foreach (var item in coluns)
            {
                var prop = typeof(BaseContent).GetProperty(item);
                if (prop.PropertyType == typeof(DateTime))
                {
                    input.Append(((DateTime)prop.GetValue(content)).ToString("yyyy-MM-dd HH:mm") + ";");
                    continue;
                }
                input.Append(prop.GetValue(content) + ";");
            }
            System.IO.File.WriteAllText(path + "\\" + filename, input.ToString());
        }

        string SaveFiles(ContentCreateModel model)
        {
            var path = _GenerateDir(model.CID);
            if (!string.IsNullOrEmpty(path))
            {
                System.IO.File.WriteAllText(path + "\\Index.html", model.HTMLBody);
            }
            else
            {
                throw new Exception("Dosya yolu yazılamadı!");
            }

            var fileStream = System.IO.File.Create(path + "\\header" + Path.GetExtension(model.ImageFile.FileName));
            model.ImageFile.InputStream.Seek(0, SeekOrigin.Begin);
            model.ImageFile.InputStream.CopyTo(fileStream);
            fileStream.Close();
            return path;
        }

        string _GenerateDir(string cid)
        {
            string directory = "";
            if (_Extentions.IsIdSafe(cid) && !Directory.Exists(_Extentions.Locate(cid)))
                directory = new BaseContent() { CID = cid }.GenerateDirectory();
            return directory;
        }
        #endregion
    }
}