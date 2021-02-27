using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;

namespace ANTOBDER.Models.EF_MODELS
{
    public partial class EF_CONTEXT : DbContext
    {
        public EF_CONTEXT()
            : base("name=EF_CONTEXT")
        {
           
        }

        public virtual DbSet<Content> Contents { get; set; }
        //public virtual DbSet<DynamicHTMLPage> DynamicHTMLPages { get; set; }
        public virtual DbSet<SiteSetting> SiteSettings { get; set; }
        public virtual DbSet<User> Users { get; set; }
        
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {

        }

        internal string GetSetting(SettingsENUM setting)
        {
            return this.SiteSettings.FirstOrDefault(e => e.ENUM == setting.ToString())?.Value;
        }
    }
}
