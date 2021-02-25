namespace ANTOBDER.Models.EF_MODELS
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("SiteSettings")]
    public partial class SiteSetting
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string ENUM { get; set; }

        [Required]
        [StringLength(250)]
        public string Value { get; set; }

        [Required]
        [StringLength(50)]
        public string DataTypeENUM { get; set; }
    }
}
