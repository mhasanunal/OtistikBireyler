namespace ANTOBDER.Migrations
{
    using ANTOBDER.Models.EF_MODELS;
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;
    using System.Text;

    public sealed class Configuration : DbMigrationsConfiguration<ANTOBDER.Models.EF_MODELS.EF_CONTEXT>
    {
        public static Encoding Encoding { get; set; } = Encoding.GetEncoding(1254);
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
        }

        protected override void Seed(ANTOBDER.Models.EF_MODELS.EF_CONTEXT context)
        {

            IList<User> defaultStandards = new List<User>();

            defaultStandards.Add(new User() { Username = "ercan", Password = "D029F3619114D1B1D6FA25D4E23568E0", Fullname = "Ercan Bal", Role = "ADMIN", IsActive = true });
            defaultStandards.Add(new User() { Username = "peri", Password = "63AD31547C1159605584614E08C4F450", Fullname = "Perihan Şahin Bal", Role = "ADMIN", IsActive = true });
            defaultStandards.Add(new User() { Username = "admin", Password = "cd95367beef3e8ab84842817b6ab9ea3", Fullname = "Admin", Role = "ADMIN", IsActive = true });
            defaultStandards.Add(new User() { Username = "hasan", Password = "356D27C9AB6A16490B0BC8F6B3A9D3A6", Fullname = "Kullanıcı", Role = "USER", IsActive = true });
            foreach (var item in defaultStandards)
            {
                if (context.Users.FirstOrDefault(c=>c.Username==item.Username) == null)
                {
                    context.Users.Add(item);

                }
            }

            IList<SiteSetting> settings = new List<SiteSetting>();
            settings.Add(new SiteSetting {ENUM=SettingsENUM.HOMEPAGE_SHOW_CONTENT_LIMITATION.ToString(),DataTypeENUM=DataTypeENUM.INT.ToString(),Value=25.ToString() });
            settings.Add(new SiteSetting {ENUM=SettingsENUM.IMAGE_COMPRESSION_QUALITY.ToString(),DataTypeENUM=DataTypeENUM.INT.ToString(),Value=55.ToString() });
            settings.Add(new SiteSetting {ENUM=SettingsENUM.IMAGE_COMPRESSION_MAXSUMOFWIDTHANDHEIGHT.ToString(),DataTypeENUM=DataTypeENUM.INT.ToString(),Value=3000.ToString() });
            foreach (var item in settings)
            {
                if (context.SiteSettings.FirstOrDefault(c => c.ENUM == item.ENUM) == null)
                {
                    context.SiteSettings.Add(item);

                }
            }
            context.SaveChanges();

        }
    }
}
