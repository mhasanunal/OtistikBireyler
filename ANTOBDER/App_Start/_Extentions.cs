using ANTOBDER.Models;
using ANTOBDER.Modules;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Web;
using System.Web.UI.WebControls;

namespace ANTOBDER

{
    namespace App_Start
    {

    }
    public static class _Extentions
    {
        internal static string RemoveUnwantedTags(string data, int length)
        {
            if (string.IsNullOrEmpty(data)) return string.Empty;

            var document = new HtmlDocument();
            document.LoadHtml(data);

            var acceptableTags = new String[] { };

            var nodes = new Queue<HtmlNode>(document.DocumentNode.SelectNodes("./*|./text()"));
            while (nodes.Count > 0)
            {
                var node = nodes.Dequeue();
                var parentNode = node.ParentNode;

                if (!acceptableTags.Contains(node.Name) && node.Name != "#text")
                {
                    var childNodes = node.SelectNodes("./*|./text()");

                    if (childNodes != null)
                    {
                        foreach (var child in childNodes)
                        {
                            nodes.Enqueue(child);
                            parentNode.InsertBefore(child, node);
                        }
                    }

                    parentNode.RemoveChild(node);

                }
            }
            length = document.DocumentNode.InnerHtml.Length < length ? document.DocumentNode.InnerHtml.Length : length;
            return document.DocumentNode.InnerHtml.Substring(0, length).Replace("&nbsp;", " ");
        }



        /// <summary>
        /// App Root Directory
        /// </summary>
        /// <returns></returns>
        public static string GetRootDirectory()
        {
            return AppDomain.CurrentDomain.BaseDirectory;
        }
        /// <summary>
        /// </summary>
        /// <returns></returns>
        public static string ArticleRootPath()
        {
            var tailing = "articles";
            return GetRootDirectory() + "" + tailing;
        }
        /// <summary>
        /// Checks if Id is specific format
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static bool IsIdSafe(string id)
        {
            var year = id.Substring(0, 4);
            var month = id.Substring(5, 2);
            var date = id.Substring(8, 2);
            var hour = id.Substring(11, 2);
            var minute = id.Substring(13, 2);
            var second = id.Substring(15, 2);
            var type = id.Last().ToString();

            try
            {
                var asDate = new DateTime(int.Parse(year), int.Parse(month), int.Parse(date), int.Parse(hour), int.Parse(minute), int.Parse(second));
                if (type != EditorialConstant && type != EventConstant)
                {
                    return false;
                }
            }
            catch (Exception)
            {

                return false;
            }
            return true;
        }

        public static string EventsRootPath()
        {
            var tailing = "events";
            return ArticleRootPath() + "\\" + tailing;
        }
        public static string ToTitleCase(this string input)
        {

            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(input?.ToLower() ?? "");
        }

        public static IEnumerable<string> GetDynamicPages()
        {
            return Directory.EnumerateFiles(PagesRootPath());
        }
        public static string PagesRootPath()
        {
            var tailing = "pages";
            return GetRootDirectory() + "\\" + tailing;
        }
        public static string EditorialRootPath()
        {
            var tailing = "editorial";
            return ArticleRootPath() + "\\" + tailing;
        }
        /// <summary>
        /// Root Path For Contents
        /// </summary>
        /// <param name="isEditorial"></param>
        /// <returns></returns>
        public static string RootPathFor(bool isEditorial)
        {
            return isEditorial ? EditorialRootPath() : EventsRootPath();
        }

        public static string GetUserBaseDir()
        {
            return GetRootDirectory() + "\\App_Data\\userInfos.txt";
        }

        public static string GetDatabaseDir()
        {

            return GetRootDirectory() + "\\App_Data\\knowledgebase\\db.accdb";
        }
        public static string SubDirectoryFor(bool isEditorial, int year)
        {
            return RootPathFor(isEditorial) + "\\" + year;
        }

        public static string ContentsFor(bool isEditorial, int year, int month)
        {
            return SubDirectoryFor(isEditorial, year) + "\\" + month.ToString("0#");
        }

        public static string BuildUpAuthorText(this string author)
        {
            if (string.IsNullOrEmpty(author))
            {
                return null;
            }
            StringBuilder sb = new StringBuilder();
            bool detectedWhiteSpace = false;
            foreach (var item in author)
            {
                if (char.IsWhiteSpace(item))
                {
                    if (!detectedWhiteSpace)
                        detectedWhiteSpace = true;
                    else
                        continue;
                }
                else if (!char.IsLetter(item))
                {
                    continue;
                }
                if (detectedWhiteSpace && !char.IsWhiteSpace(item))
                {
                    detectedWhiteSpace = false;
                }
                sb.Append(item);

            }
            return _Extentions.ToTitleCase(sb.ToString().Trim());
        }

        public static string CheckForTags(this string tags)
        {
            if (string.IsNullOrEmpty(tags))
            {
                return null;
            }

            StringBuilder sb = new StringBuilder();
            bool detectedWhiteSpace = false;
            foreach (var tag in tags.Split('_'))
            {
                foreach (var item in tag)
                {
                    if (char.IsWhiteSpace(item))
                    {
                        if (!detectedWhiteSpace)
                            detectedWhiteSpace = true;
                        else
                            continue;
                    }


                    if (detectedWhiteSpace && !char.IsWhiteSpace(item))
                    {
                        detectedWhiteSpace = false;
                    }
                    sb.Append(item.ToString().ToLower(new CultureInfo("tr-TR")));

                }
                sb.Append('_');
            }
            return sb.ToString().Substring(0, sb.Length - 1);
        }
        public static void CreateRootPaths()
        {
            if (!Directory.Exists(_Extentions.EventsRootPath()))
            {
                Directory.CreateDirectory(_Extentions.EventsRootPath());
            }
            if (!Directory.Exists(_Extentions.EditorialRootPath()))
            {
                Directory.CreateDirectory(_Extentions.EditorialRootPath());
            }
            if (!Directory.Exists(_Extentions.PagesRootPath()))
            {
                Directory.CreateDirectory(_Extentions.PagesRootPath());
            }
        }
        public static string CreateMD5(string input)
        {
            // Use input string to calculate MD5 hash
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                // Convert the byte array to hexadecimal string
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }
                return sb.ToString();
            }
        }
        public static string Locate(string cid)
        {
            var dir = GetDirectoryByID(cid);
            return dir;
        }
        public static string Locate(this BaseContent ct)
        {
            return GetDirectoryByID(ct.CID);
        }
        public static string GetDirectoryByID(string cid)
        {
            var year = cid.Substring(0, 4);
            var month = cid.Substring(5, 2);
            var date = cid.Substring(8, 2);
            var hour = cid.Substring(11, 2);
            var minute = cid.Substring(13, 2);
            var second = cid.Substring(15, 2);
            var type = cid.Last().ToString();
            var root = EventsRootPath();
            if (type == EditorialConstant)
                root = EditorialRootPath();

            return $"{root}\\{year}\\{month}\\{date}-{hour}{minute}{second}";
        }
        public const string EditorialConstant = "e";
        public const string EventConstant = "f";

        public static string GalleryPathName = "gallery";

        public static string CreateID(bool isEditorial)
        {
            return $"{DateTime.Now.ToString("yyyy-MM-dd-HHmmss")}-{(isEditorial ? EditorialConstant : EventConstant)}";
        }


        public static IEnumerable<BaseContent> GetEditorials()
        {
            return new ContextBase().Contents
                .Where(e => e.IsEditorial)
                .OrderByDescending(c => c.On)
                .Take(6);
        }

        public static string GenerateDirectory(this BaseContent content)
        {

            if (Directory.Exists(GetDirectoryByID(content.CID)))
            {
                content.CID = CreateID(content.IsEditorial);
                return content.GenerateDirectory();
            }
            else
            {
                var info = Directory.CreateDirectory(content.Locate());
                return info.FullName;
            }
        }

        public static string GetUserFullName(this IIdentity identity)
        {
            return identity.GetClaimValue(ClaimTypes.GivenName);
        }
        public static string GetUsername(this IIdentity identity)
        {
            return identity.GetClaimValue(ClaimTypes.NameIdentifier);
        }
        public static string GetRole(this IIdentity identity)
        {
            return identity.GetClaimValue(ClaimTypes.Role);
        }
        public static string GetClaimValue(this IIdentity identity, string key)
        {
            return identity.AsClaimsIdentity()?.Claims?.FirstOrDefault(c => c.Type == key)?.Value;
        }

        public static ClaimsIdentity AsClaimsIdentity(this IIdentity identity)
        {
            return (ClaimsIdentity)identity;
        }
    }
}