using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;

namespace ANTOBDER.Models
{
    public class ContentCreateModel
    {
        public string Header { get; set; }
        public HttpPostedFileBase ImageFile { get; set; }
        public string CID { get; set; }
        public IEnumerable<HttpPostedFileBase> ImageGallery { get; set; }
        public string Author { get; set; }
        public DateTime AddedOn{ get; set; }
        public string Tags { get; set; }
        public string HtmlFileName { get; set; } = "Index.html";
        public string HeaderImageFileName { get; set; } = "header";

        [AllowHtml]
        public string HTMLBody { get; set; }
    }
}