namespace ANTOBDER.Models.EF_MODELS
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;
    [Table("Contents")]
    public partial class Content
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string CID { get; set; }

        [Required]
        public string Path { get; set; }

        [Required]
        [StringLength(50)]
        public string Author { get; set; }

        public DateTime On { get; set; }

        public string Tags { get; set; }

        [Required]
        [StringLength(50)]
        public string ImageFile { get; set; }

        [StringLength(250)]
        public string Describer { get; set; }

        [Required]
        [StringLength(250)]
        public string Header { get; set; }

        [StringLength(50)]
        public string HtmlFile { get; set; }
    }
}
