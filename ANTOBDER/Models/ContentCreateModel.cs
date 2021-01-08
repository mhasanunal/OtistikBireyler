using System;
using System.Web;
using System.Web.Mvc;

namespace ANTOBDER.Models
{
    public class ContentCreateModel
    {
        public string Header { get; set; }
        public HttpPostedFileBase ImageFile { get; set; }
        public string CID { get; set; }
        public string Author { get; set; }
        public DateTime AddedOn{ get; set; }
        public string Tags { get; set; }

        [AllowHtml]
        public string HTMLBody { get; set; }
    }
}