using ANTOBDER.Models;
using ANTOBDER.Models.EF_MODELS;
using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;

namespace ANTOBDER.Modules
{
    public class YALANCI_CONTEXT
    {




        #region RETRIEVING BACKUP
        public string BaseDirForCSVFiles { get; set; }
        public YALANCI_CONTEXT(string csvFilesRootPath)
        {
            BaseDirForCSVFiles = csvFilesRootPath;
        }



        public IEnumerable<Content> Contents()
        {
            var filePath = BaseDirForCSVFiles + "\\" + ContentsBackupPath;
            if (File.Exists(filePath))
            {
                var allLines = File.ReadAllLines(filePath);
                foreach (var line in allLines)
                {
                    //                    wr.WriteLine($"{item.CID};{item.Path};{item.Author};{item.On.ToString("yyyy-MM-dd HH:mm")};{item.Tags};{item.ImageFile};{item.Describer};{item.Header};{item.HtmlFile}");

                    var fields = line.Split(';');
                    yield return new Content
                    {
                        CID = fields[0],
                        Path = fields[1],
                        Author = fields[2],
                        On = DateTime.Parse(fields[3]),
                        Tags = fields[4],
                        ImageFile = fields[5],
                        Header = fields[6],
                        HtmlFile = fields[7]


                    };
                }
            }
        }
        public IEnumerable<DynamicHTMLPage> DynamicHTMLPages()
        {
            var filePath = BaseDirForCSVFiles + "\\" + DynamicHTMLPagesBackupPath;
            if (File.Exists(filePath))
            {
                var allLines = File.ReadAllLines(filePath);
                foreach (var line in allLines)
                {
                    //    wr.WriteLine($"{item.PageName};{item.RawHTML}");
                    var fields = line.Split(';');
                    yield return new DynamicHTMLPage {
                        PageName = fields[0],
                        RawHTML= fields[1]
                    };
                }
            }
        }
        public IEnumerable<SiteSetting> SiteSettings()
        {
            //wr.WriteLine($"{item.ENUM};{item.Value};{item.DataTypeENUM}");
            var filePath = BaseDirForCSVFiles + "\\" + SiteSettingsBackupPath;
            if (File.Exists(filePath))
            {
                var allLines = File.ReadAllLines(filePath);
                foreach (var line in allLines)
                {

                    var fields = line.Split(';');
                    yield return new SiteSetting
                    {
                        ENUM= fields[0],
                        Value= fields[1],
                        DataTypeENUM= fields[2]
                    };
                }
            }
        }

        #endregion

        public static string ContentsBackupPath { get; set; } = _Extentions.GetBackupFolderForDB() + "\\contents.csv";
        public static string SiteSettingsBackupPath { get; set; } = _Extentions.GetBackupFolderForDB() + "\\siteSettings.csv";
        public static string DynamicHTMLPagesBackupPath { get; set; } = _Extentions.GetBackupFolderForDB() + "\\dynamicHTMLPages.csv";

        #region GENERATING_BACKUP
        public YALANCI_CONTEXT(EF_CONTEXT db, ZipArchive zip)
        {
            Db = db;
            Zip = zip;
        }
        public EF_CONTEXT Db { get; }
        public ZipArchive Zip { get; }
        internal void ZipEntryFor_dynamicHTMLPages()
        {
            //var dynamicHTMLPages = _Extentions.GetBackupFolderForDB() + "\\dynamicHTMLPages.csv";
            var dynamicEntry = Zip.CreateEntry(DynamicHTMLPagesBackupPath);
            using (var wr = new StreamWriter(dynamicEntry.Open()))
            {
                foreach (var item in Db.DynamicHTMLPages)
                {
                    wr.WriteLine($"{item.PageName};{item.RawHTML}");
                }
                wr.Flush();
                wr.Close();
            }
        }

        internal void ZipEntryFor_siteSettings()
        {
            //var siteSettings = _Extentions.GetBackupFolderForDB() + "\\siteSettings.csv";
            var settingsEntry = Zip.CreateEntry(SiteSettingsBackupPath);

            using (var wr = new StreamWriter(settingsEntry.Open()))
            {
                foreach (var item in Db.SiteSettings)
                {
                    wr.WriteLine($"{item.ENUM};{item.Value};{item.DataTypeENUM}");
                }
                wr.Flush();
                wr.Close();
            }
        }

        internal void ZipEntryFor_contents()
        {
            //var contents = _Extentions.GetBackupFolderForDB() + "\\contents.csv";
            var contentEntry = Zip.CreateEntry(ContentsBackupPath);

            using (var wr = new StreamWriter(contentEntry.Open()))
            {
                foreach (var item in Db.Contents)
                {
                    wr.WriteLine($"{item.CID};{item.Path};{item.Author};{item.On.ToString("yyyy-MM-dd HH:mm")};{item.Tags};{item.ImageFile};{item.Describer};{item.Header};{item.HtmlFile}");
                }
                wr.Flush();
                wr.Close();
            }
        }
        #endregion
    }
}