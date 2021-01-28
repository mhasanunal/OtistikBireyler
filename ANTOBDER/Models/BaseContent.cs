using ANTOBDER.App_Start;
using ANTOBDER.Modules;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;

namespace ANTOBDER.Models
{
    [Table("Users")]
    public class User : IEntity<int>
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }
        public string Fullname { get; set; }
    }

    [Table("Content")]
    public class BaseContent : IEntity<int>
    {
        public int Id { get; set; }
        public string CID { get; set; }
        public string Path { get; set; }
        public string Author { get; set; }
        public DateTime On { get; set; }
        public string Tags { get; set; }
        public string ImageFile { get; set; }
        public string Describer { get; set; }
        public string Header { get; set; }
        public string HtmlFile { get; set; }
        public bool IsEditorial { get { return CID.EndsWith(_Extentions.EditorialConstant); } }
        /// <summary>
        /// Generates a relative path with respect to server path: For ex: /articles
        /// </summary>
        /// <returns></returns>
        public string GenerateRelativePath()
        {
            return this.Path.Replace(_Extentions.GetRootDirectory(), string.Empty);
        }

        public string GenerateRelativePathIndependently()
        {
            return this.Path.Split(new string[] { "articles" }, StringSplitOptions.None)[1];
        }

        public bool HasImageGallery()
        {
            return Directory.Exists(this.Path + "\\" + _Extentions.GalleryPathName);
        }
        public IEnumerable<string> IterateImageGallery()
        {
            if (this.HasImageGallery())
            {
                foreach (var item in Directory.EnumerateFiles(this.Path + "\\" + _Extentions.GalleryPathName))
                {
                    yield return item.Replace(this.Path,"");
                }
            }
        }
    }

    public class ContentRevailer
    {
        public ContentRevailer(BaseContent content)
        {

        }
    }
}