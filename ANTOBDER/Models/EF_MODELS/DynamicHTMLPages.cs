namespace ANTOBDER.Models.EF_MODELS
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("DynamicHTMLPages")]
    public partial class DynamicHTMLPage
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string PageName { get; set; }

        [Required]
        public string RawHTML { get; set; }
    }
}
