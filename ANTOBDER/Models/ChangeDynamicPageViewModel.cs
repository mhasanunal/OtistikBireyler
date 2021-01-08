using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ANTOBDER.Models
{
    public class ChangeDynamicPageViewModel
    {
        [AllowHtml]
        public string HtmlBody { get; set; }
    }

    public class SpecOverview
    {
        public long DiscCapacity { get; set; }
        public long DiscUsage { get; set; }
        public long RamUsage { get; set; }
        public long RamCapacity { get; set; }

    }
}