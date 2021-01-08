using ANTOBDER.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ANTOBDER
{
    public class IndexViewModel
    {
        public IEnumerable<BaseContent> Editorial { get; set; }
        public IEnumerable<BaseContent> Events { get; set; }

    }
}