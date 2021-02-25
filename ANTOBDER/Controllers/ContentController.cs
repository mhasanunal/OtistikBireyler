using ANTOBDER.Models.EF_MODELS;
using ANTOBDER.Models;
using ANTOBDER.Modules;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace ANTOBDER.Controllers
{
    public class ContentController : Controller
    {
        const int AllowedMaxWidthInPixels = 659;

        [HttpPost]
        [Authorize]
        public ActionResult Create(ContentCreateModel model)
        {
            var img = model.ImageFile.FileName;
            var path = SaveFiles(model);

            var content = new Content()
            {
                CID = model.CID,
                Author = model.Author.BuildUpAuthorText(),
                Describer = _Extentions.RemoveUnwantedTags(model.HTMLBody, 65),
                Header = model.Header ?? "İçerik Başlığı",
                ImageFile = "header" + Path.GetExtension(model.ImageFile.FileName),
                On = model.AddedOn,
                Path = path,
                Tags = model.Tags.CheckForTags(),
                HtmlFile = "Index.html"//If you want to change Index.html rendering start changing this.
            };
            CreateMeta(path, content);
            try
            {
                using (var dbContext = new EF_CONTEXT())
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
                DeleteFiles(path);
            }
            return RedirectToAction("Index", "Enter");
        }

        private void DeleteFiles(string path)
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }
        }

        [Authorize(Roles = "SUPER")]
        public string DeleteAll()
        {
            int deleted;
            using (var dbContext = new EF_CONTEXT())
            {
                dbContext.Contents.RemoveRange(dbContext.Contents);
                deleted = dbContext.SaveChanges();

            }
            return $"Done! Deleted {deleted} amount of content!";
        }
        public ActionResult ReadArticle(string id)
        {
            Content content;

            using (var dbContext = new EF_CONTEXT())
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
            }//sanity check



            return View(content);
        }


        private const int OrientationKey = 0x0112;
        private const int NotSpecified = 0;
        private const int NormalOrientation = 1;
        private const int MirrorHorizontal = 2;
        private const int UpsideDown = 3;
        private const int MirrorVertical = 4;
        private const int MirrorHorizontalAndRotateRight = 5;
        private const int RotateLeft = 6;
        private const int MirorHorizontalAndRotateLeft = 7;
        private const int RotateRight = 8;

        /// <summary>
        /// Resize the image to the specified width and height.
        /// </summary>
        /// <param name="image">The image to resize.</param>
        /// <param name="width">The width to resize to.</param>
        /// <param name="height">The height to resize to.</param>
        /// <returns>The resized image.</returns>

        Image ImageToFixedSize(Image image, Size newSize)
        {
            var width = newSize.Width;
            var height = newSize.Height;
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }
            // Fix orientation if needed.
            if (image.PropertyIdList.Contains(OrientationKey))
            {
                var orientation = (int)image.GetPropertyItem(OrientationKey).Value[0];
                switch (orientation)
                {
                    case NotSpecified: // Assume it is good.
                    case NormalOrientation:
                        // No rotation required.
                        break;
                    case MirrorHorizontal:
                        destImage.RotateFlip(RotateFlipType.RotateNoneFlipX);
                        break;
                    case UpsideDown:
                        destImage.RotateFlip(RotateFlipType.Rotate180FlipNone);
                        break;
                    case MirrorVertical:
                        destImage.RotateFlip(RotateFlipType.Rotate180FlipX);
                        break;
                    case MirrorHorizontalAndRotateRight:
                        destImage.RotateFlip(RotateFlipType.Rotate90FlipX);
                        break;
                    case RotateLeft:
                        destImage.RotateFlip(RotateFlipType.Rotate90FlipNone);
                        break;
                    case MirorHorizontalAndRotateLeft:
                        destImage.RotateFlip(RotateFlipType.Rotate270FlipX);
                        break;
                    case RotateRight:
                        destImage.RotateFlip(RotateFlipType.Rotate270FlipNone);
                        break;
                    default:
                        break;
                }
            }


            return destImage;
        }
        public ActionResult ListArticle(
            string author,
            string tags,
            bool listAll = false
            )
        {
            List<Content> result;
            string filter = null;
            var authorFunc = funcByAuthor(author.BuildUpAuthorText(), out string authorFilter);
            var tagsFunc = funcByTags(tags, out string tagFilter);
            using (var db = new EF_CONTEXT())
            {
                var filtered = db.Contents.ToList().Where(content => authorFunc(content) && tagsFunc(content));
                if (!listAll)
                {
                    filtered = filtered.Where(content => !content.IsEditorial());
                }
                result = filtered.ToList();
            }
            if (!string.IsNullOrEmpty(authorFilter))
            {
                filter = authorFilter;
            }
            if (!string.IsNullOrEmpty(tagFilter))
            {
                tagFilter = tagFilter.Substring(0, tagFilter.Length - 1);
                filter = string.IsNullOrEmpty(filter) ? tagFilter : filter + ";" + tagFilter;
            }
            return View(new Tuple<string, IEnumerable<Content>>(filter, result));
        }

        private Func<Content, bool> funcByTags(string tags, out string filter)
        {
            filter = string.IsNullOrEmpty(tags) ? null : tags.Split('_').Aggregate("Etiket(ler): ", (seed, next) => { return seed + next + ", "; });
            var _culture = new CultureInfo("tr-TR");

            /// generic implementation seems to be illogical to implement
            /// due to neglecting actual search purpose
            return content =>
            ((!string.IsNullOrEmpty(tags)) && tags.Split(',').All(
                tag =>
                content.Tags.StartsWith(tag.ToLower(_culture))
                ||
                content.Tags.Contains("_" + tag.ToLower(_culture) + "_")
                ||
                content.Tags.EndsWith(tag.ToLower(_culture))))
                || string.IsNullOrEmpty(tags);
        }

        private Func<Content, bool> funcByAuthor(string author, out string filter)
        {
            filter = string.IsNullOrEmpty(author) ? null : "Yazar: " + author;
            return c => (!string.IsNullOrEmpty(author) && c.Author == author) || string.IsNullOrEmpty(author);
        }

        [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "ADMIN,SUPER")]
        public ActionResult Delete(string id)
        {

            using (var dbContext = new EF_CONTEXT())
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

        void CreateMeta(string path, Content content)
        {
            var filename = "meta.txt";
            var input = new StringBuilder();
            var coluns = content.Columns();
            foreach (var item in coluns)
            {
                input.Append(item + ";");
            }
            input.AppendLine();
            foreach (var item in coluns)
            {
                var prop = typeof(Content).GetProperty(item);
                if (prop.PropertyType == typeof(DateTime))
                {
                    input.Append(((DateTime)prop.GetValue(content)).ToString("yyyy-MM-dd HH:mm") + ";");
                    continue;
                }
                input.Append(prop.GetValue(content) + ";");
            }
            System.IO.File.WriteAllText(path + "\\" + filename, input.ToString());
        }
        /// <summary>
        /// returns articles root path
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        string SaveFiles(ContentCreateModel model)
        {
            var path = _GenerateDir(model.CID);
            if (!string.IsNullOrEmpty(path))
            {
                SaveHTML(path, model);
            }
            else
            {
                throw new Exception("Dosya yolu yazılamadı!");
            }

            SaveHeaderImage(path, model);

            SaveImageGallery(path + "\\" + _Extentions.GalleryPathName, model);

            return path;
        }

        private void SaveImageGallery(string path, ContentCreateModel model)
        {
            if (model.ImageGallery == null || !model.ImageGallery.Any(c => c != null))
                return;
            Directory.CreateDirectory(path + "\\" + _Extentions.GalleryPathName);
            foreach (var galleryItem in model.ImageGallery)
            {
                var extension = Path.GetExtension(galleryItem.FileName);
                var imagePath = SaveImage(path, galleryItem.FileName.Replace(extension, ""), extension, galleryItem.InputStream);

            }
        }
        private string SaveImage(string path, string name, string extension, Stream stream)
        {
            var imageFileName = name + extension;

            Antobder.Drawing.ReducerOptions opt = new Antobder.Drawing.ReducerOptions(
                filename: imageFileName,
                maxSummaryOfWidthAndHeight: int.Parse(ConfigurationManager.AppSettings["maxSummaryOfWidthAndHeight"]),
                destinationFolder: path
                );

            Antobder.Drawing.ImageSizeReducer.ReduceSizeOnDisk(stream, opt);

            //var fileStream = System.IO.File.Create(imagePath);
            //stream.Seek(0, SeekOrigin.Begin);
            //stream.CopyTo(fileStream);
            //fileStream.Close();

            //if (extension.ToLower() == ".jpg")

            //{
            //    DecideResizeOfImage(imagePath);
            //}
            return path + "\\" + imageFileName;
        }

        private void DecideResizeOfImage(string path)
        {
            var image = Image.FromFile(path);
            if (image.Width <= AllowedMaxWidthInPixels)
            {
                return;
            }

            var newSize = ResizeKeepAspect(image.Size, AllowedMaxWidthInPixels, AllowedMaxWidthInPixels, false);

            var newImage = ImageToFixedSize(image, newSize);
            image.Dispose();
            System.IO.File.Delete(path);
            newImage.Save(path);

        }
        private Size ResizeKeepAspect(Size src, int maxWidth, int maxHeight, bool enlarge = false)
        {
            maxWidth = enlarge ? maxWidth : Math.Min(maxWidth, src.Width);
            maxHeight = enlarge ? maxHeight : Math.Min(maxHeight, src.Height);

            decimal rnd = Math.Min(maxWidth / (decimal)src.Width, maxHeight / (decimal)src.Height);
            return new Size((int)Math.Round(src.Width * rnd), (int)Math.Round(src.Height * rnd));
        }
        private void SaveHeaderImage(string path, ContentCreateModel model)
        {
            var extension = Path.GetExtension(model.ImageFile.FileName);
            var imagePath = SaveImage(path, model.HeaderImageFileName, extension, model.ImageFile.InputStream);
        }

        private void SaveHTML(string path, ContentCreateModel model)
        {
            System.IO.File.WriteAllText(path + "\\" + model.HtmlFileName, model.HTMLBody);
        }

        string _GenerateDir(string cid)
        {
            string directory = "";
            if (_Extentions.IsIdSafe(cid) && !Directory.Exists(_Extentions.Locate(cid)))
                directory = new Content() { CID = cid }.GenerateDirectory();
            return directory;
        }
        #endregion
    }
}