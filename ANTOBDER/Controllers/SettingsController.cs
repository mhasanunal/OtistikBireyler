using ANTOBDER.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO.Compression;
using System.Net.Mime;
using ANTOBDER.Modules;
using System.Globalization;
using ANTOBDER.Models.EF_MODELS;

namespace ANTOBDER.Controllers
{
    [Authorize]
    public class SettingsController : Controller
    {

        public ActionResult Specs()
        {
            long dir = GetDirectorySize();
            long available = long.Parse(ConfigurationManager.AppSettings["DiscCapacity"]);
            long ramUsage = CalculateRamUsage();
            var spec = new SpecOverview();
            spec.DiscCapacity = available;
            spec.DiscUsage = dir;
            spec.RamUsage = ramUsage;
            return View(spec);
        }
        public ActionResult Database()
        {
            return View();
        }
        public FileResult BackUpDB()
        {
            MemoryStream fileStream = GenerateBackup();

            var cd = new ContentDisposition("attachment")
            {
                FileName = $"ANTOBDER-{DateTime.Now.ToString("yyyyMMdd")}.bkp",
                Inline = false
            };
            Response.AppendHeader("Content-Disposition", cd.ToString());

            byte[] file = fileStream.ToArray();
            //System.IO.File.Delete(_or._FolderPath+".zip");
            //string base64String = Convert.ToBase64String(file, 0, file.Length);
            return File(file, "application/zip");
        }
        [Authorize(Roles = "ADMIN,SUPER")]
        public FileResult BackUpDBBeforeDate(string date)
        {
            MemoryStream fileStream = GenerateBackup(ParseInputDate(date));

            var cd = new ContentDisposition("attachment")
            {

                FileName = $"ANTOBDER-@{DateTime.Now.ToString("yyyyMMdd")}_UNTIL@{date}.bkp",
                Inline = false
            };
            Response.AppendHeader("Content-Disposition", cd.ToString());

            byte[] file = fileStream.ToArray();
            //System.IO.File.Delete(_or._FolderPath+".zip");
            //string base64String = Convert.ToBase64String(file, 0, file.Length);
            return File(file, "application/zip");

        }
        [Authorize(Roles = "ADMIN,SUPER")]
        public FileResult BackupDBBeforeDateAndDeleteEntriesAndFolders(string date)
        {
            MemoryStream fileStream = GenerateBackup(ParseInputDate(date), true);

            var cd = new ContentDisposition("attachment")
            {
                FileName = $"ANTOBDER-{DateTime.Now.ToString("yyyyMMdd")}.bkp",
                Inline = false
            };
            Response.AppendHeader("Content-Disposition", cd.ToString());

            byte[] file = fileStream.ToArray();

            return File(file, "application/zip");
        }
        [Authorize(Roles = "ADMIN,SUPER")]
        public ActionResult ResetDatabase()
        {
            int total;
            using (var db = new EF_CONTEXT())
            {
                db.Contents.RemoveRange(db.Contents);
                total = db.SaveChanges();
            }
            TempData["Result"] = "Toplam " + total + " içerik veritabanından silindi";
            return RedirectToAction("Database");
        }

        [Authorize(Roles = "ADMIN,SUPER")]
        public ActionResult ResetDatabaseAndDeleteFolders()
        {
            int total = 0;
            using (var db = new EF_CONTEXT())
            {
                db.Contents.RemoveRange(db.Contents);
                total = db.SaveChanges();
            }
            if (Directory.Exists(_Extentions.EditorialRootPath()))
            {
                Directory.Delete(_Extentions.EditorialRootPath(), true);
            }
            if (Directory.Exists(_Extentions.EventsRootPath()))
            {
                Directory.Delete(_Extentions.EventsRootPath(), true);

            }
            _Extentions.CreateRootPaths();

            TempData["Result"] = "Toplam " + total + " içerik ve dosyaları veritabanından silindi!";
            return RedirectToAction("Database");
        }

        public ActionResult UploadBackup(HttpPostedFileBase backupFile)
        {

            int total = 0;


            string destination = UploadFile(backupFile);
            bool wipeDBFirst = false;
            total = RecoverFromBackup(destination, wipeDBFirst);

            TempData["Result"] = "Yeni İçerikler ekleniyor... Toplam " + total
                + " yeni içerik bulundu. Veritabanına güncelleniyor...";
            return RedirectToAction("Database");
        }

        private int RecoverFromBackup(string destination, bool wipeDBFirst)
        {
            int total = 0;
            using (var db = new EF_CONTEXT())
            {

                if (wipeDBFirst)
                {
                    db.Contents.RemoveRange(db.Contents);
                    total = db.SaveChanges();
                }

                //var normalDbDir = _Extentions.GetDatabaseDir()
                //                    .Replace(_Extentions.GetRootDirectory(), "");
                //var backedUpDatabaseDir = destination + "\\" + normalDbDir;
                var backedUpContext = new YALANCI_CONTEXT(destination);
                foreach (var item in backedUpContext.Contents())
                {
                    if (db.Contents.Any(c => c.CID == item.CID))
                    {
                        continue;
                    }
                    total++;
                    if (item.Path.Replace(_Extentions.GetRootDirectory(), "").Length >= item.Path.Length)
                    {
                        var articles = item.Path.Split(new string[] { "articles" }, StringSplitOptions.None);
                        var tailing = articles[1];
                        item.Path = _Extentions.ArticleRootPath() + tailing;
                    }
                    db.Contents.Add(item);
                    var relative = item.GenerateRelativePathIndependently();
                    Directory.CreateDirectory(_Extentions.ArticleRootPath() + "\\" + relative);
                    var target = destination + "\\articles\\" + relative;
                    foreach (var filePath in Directory.GetFiles(target))
                    {
                        var fileName = System.IO.Path.GetFileName(filePath);
                        var destFile = _Extentions.ArticleRootPath() + "" + relative;
                        destFile = System.IO.Path.Combine(destFile, fileName);

                        System.IO.File.Copy(filePath, destFile, true);
                    }
                }

                db.SaveChanges();
            }
            System.IO.Directory.Delete(destination, true);
            return total;
        }

        private string UploadFile(HttpPostedFileBase backupFile)
        {
            var root = _Extentions.GetRootDirectory() + "\\App_Data\\backups";
            if (!Directory.Exists(root))
            {
                Directory.CreateDirectory(root);
            }

            byte[] file = null;

            using (var mem = new MemoryStream())
            {
                backupFile.InputStream.CopyTo(mem);
                file = mem.ToArray();
            }

            var path = root + "\\" + backupFile.FileName.Replace(".bkp", ".zip");
            System.IO.File.WriteAllBytes(path, file);
            var destination = root + "\\" + Guid.NewGuid().ToString("D").Substring(0, 8);
            ZipFile.ExtractToDirectory(path, destination);
            System.IO.File.Delete(path);
            return destination;
        }


        public ActionResult CreateUser(User usr)
        {
            using (var db = new EF_CONTEXT())
            {

                if (db.Users.Any(c => c.Username.ToLower() == usr.Username.ToLower()))
                {
                    TempData["Error"] = "Bu isimde [" + usr.Username + "] bir kullanıcı zaten var!";
                    return RedirectToAction("Users", "Settings");
                }
                if (User.IsInRole("USER") && usr.Role != "USER")
                {
                    usr.Role = "USER";
                }
                else if (User.IsInRole("ADMIN") && usr.Role == "SUPER")
                {
                    usr.Role = "ADMIN";
                }

                usr.Password = _Extentions.CreateMD5(usr.Password);
                db.Users.Add(usr);
                db.SaveChanges();
            }
            TempData["Message"] = "Kullanıcı [" + usr.Username + "] Başarı ile yaratıldı!";
            return RedirectToAction("Users", "Settings");
        }
        public ActionResult Users()
        {
            IEnumerable<User> users;
            using (var db = new EF_CONTEXT())
            {
                users = db.Users.ToList();
            }
            return View(users);
        }
        [Authorize(Roles = "ADMIN,SUPER"), ValidateAntiForgeryToken, HttpPost]
        public ActionResult ChangeUserPassword(int id, string changedPassword)
        {
            string username;
            using (var db = new EF_CONTEXT())
            {
                var user = db.Users.First(u => u.Id == id);
                if (user == null)
                {
                    TempData["Error"] = "Kullanıcı Bulunamadı!";
                    return RedirectToAction("Users", "Settings");
                }
                user.Password = _Extentions.CreateMD5(changedPassword);
                db.Users.Remove(user);
                db.Users.Add(user);
                db.SaveChanges();
                username = user.Username;
            }
            TempData["Message"] = "Kullanıcı [" + username + "] Parolası değiştirildi! ";
            return RedirectToAction("Users", "Settings");
        }
        [Authorize(Roles = "ADMIN,SUPER"), HttpPost]
        public ActionResult RestoreFromBackup(HttpPostedFileBase backupFile)
        {
            string destination = UploadFile(backupFile);
            bool wipeDBFirst = true;
            int total = RecoverFromBackup(destination, wipeDBFirst);
            TempData["Result"] = "Yedekten kurtarma işlemi devam ediyor. Toplam " + total +
                " içerik bulundu. Veritabanına kurtarılıyor...";
            return RedirectToAction("Database");
        }

        #region PRIVATES


        long CalculateRamUsage()
        {
            var process = Process.GetCurrentProcess();

            long memsize = 0; // memsize in KB
            //PerformanceCounter PC = new PerformanceCounter();
            //PC.CategoryName = "Process";
            //PC.CounterName = "Working Set - Private";
            //PC.InstanceName = process.ProcessName;
            memsize = process.WorkingSet64 / 1024;
            //PC.Close();
            //PC.Dispose();
            return memsize;
        }
        DateTime ParseInputDate(string date)
        {
            bool parsed = DateTime.TryParseExact(date, "yyyy-MM-ddTHH:mm:ss", new CultureInfo("tr-TR"), DateTimeStyles.AssumeLocal, out DateTime result);

            if (parsed)
            {
                return result;
            }
            throw new Exception("Girilen değeri programatik tarihe çeviremiyorum! girilen değer: " + date);
        }
        long GetDirectorySize()
        {
            var p = _Extentions.GetRootDirectory();
            // 1.
            // Get array of all file names.
            string[] a = Directory.GetFiles(p, "*.*");

            // 2.
            // Calculate total bytes of all files in a loop.
            long b = 0;
            foreach (string name in a)
            {
                // 3.
                // Use FileInfo to get length of each file.
                FileInfo info = new FileInfo(name);
                b += info.Length;
            }
            // 4.
            // Return total size
            return b;
        }
        MemoryStream GenerateBackup(
            DateTime? until = null,
            bool deleteFile = false
            )
        {
            if (until == null)
            {
                until = DateTime.Now;
            }


            var zipStream = new MemoryStream();

            using (ZipArchive zip = new ZipArchive(zipStream, ZipArchiveMode.Create, true))
            {
                BackupUntil(until, _Extentions.ArticleRootPath(), zip, deleteFile);

                BackupDBasCSV(zip);
                var dbRoot = _Extentions.GetDatabaseDir();
                var toBeReplaced = _Extentions.GetRootDirectory();
                var dbEntry = zip.CreateEntry(dbRoot.Replace(toBeReplaced + "\\", ""));

                using (var wr = new BinaryWriter(dbEntry.Open()))
                {
                    var bytes = System.IO.File.ReadAllBytes(_Extentions.GetDatabaseDir());
                    wr.Write(bytes);
                    wr.Flush();
                    wr.Close();
                }

                //if (includeStaticHTMLFiles)
                //{
                //    foreach (var item in Directory.EnumerateDirectories(_Extentions.PagesRootPath()))
                //    {
                //        var entry = zip.CreateEntry(item.Replace(toBeReplaced + "\\", ""));
                //        using (var wr = new BinaryWriter(entry.Open()))
                //        {
                //            var bytes = System.IO.File.ReadAllBytes(item);
                //            wr.Write(bytes);
                //            wr.Flush();
                //            wr.Close();
                //        }
                //    }
                //}

                if (deleteFile)
                {
                    Directory.Delete(_Extentions.EventsRootPath(), true);
                    Directory.Delete(_Extentions.EditorialRootPath(), true);
                    try
                    {
                        using (var db = new EF_CONTEXT())
                        {
                            db.Contents.RemoveRange(db.Contents);
                            db.SaveChanges();
                        }
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }

            }

            return zipStream;
        }

        private void BackupDBasCSV(ZipArchive zip)
        {
            using (var db = new EF_CONTEXT())
            {
            var yalanci = new YALANCI_CONTEXT(db,zip);

                yalanci.ZipEntryFor_dynamicHTMLPages();
                yalanci.ZipEntryFor_siteSettings();
                yalanci.ZipEntryFor_contents();

              


                
            }


        }

        void BackupUntil(DateTime? until, string path, ZipArchive zip, bool deleteFile)
        {
            IEnumerable<string> files = FromDirectory(path);
            var entries = Entries(files, _Extentions.GetRootDirectory(), zip, until);

            foreach (var item in entries)
            {
                var actualPath = files.FirstOrDefault(c => c.EndsWith(item.FullName));
                var fi = new FileInfo(actualPath);
                if (fi.LastWriteTime > until.Value.AddDays(1))
                {
                    continue;
                }
                using (var wr = new BinaryWriter(item.Open()))
                {
                    var bytes = System.IO.File.ReadAllBytes(actualPath);
                    wr.Write(bytes);
                    wr.Flush();
                    wr.Close();
                }
                if (deleteFile)
                {
                    System.IO.File.Delete(actualPath);
                }
            }

        }
        IEnumerable<ZipArchiveEntry> Entries(IEnumerable<string> files, string path, ZipArchive zip, DateTime? until)
        {
            foreach (var item in files)
            {
                if (until != null)
                {
                    var fi = new FileInfo(item);
                    if (fi.LastWriteTime > until.Value.AddDays(1))
                    {
                        continue;
                    }
                }
                yield return zip.CreateEntry(item.Replace(path, ""));
            }
        }
        IEnumerable<string> FromDirectory(string path)
        {

            return Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
        }
        #endregion
    }
}