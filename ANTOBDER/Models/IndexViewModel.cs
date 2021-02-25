using ANTOBDER.Models;
using ANTOBDER.Models.EF_MODELS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ANTOBDER
{
    public class IndexViewModel
    {
        public IEnumerable<Content> Editorial { get; set; }
        public IEnumerable<Content> Events { get; set; }

    }
}