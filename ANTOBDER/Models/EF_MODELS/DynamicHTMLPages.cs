namespace ANTOBDER.Models.EF_MODELS
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class DynamicHTMLPage
    {
        public int Id { get; set; }

        public string PageName { get; set; }

        
        public string RawHTML { get; set; }
    }
}
