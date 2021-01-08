using ANTOBDER.App_Start;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Web;

namespace ANTOBDER.Modules
{

    public class BaseSet<T, TKey> : OleSet<T, TKey>
        where T : class,IEntity<TKey>,new()
    {
        ContextBase contextInstance;
        private IEnumerator<T> _Get(int? top = null, string orderBy = null)
        {
            var cmd = contextInstance.dbConnection.CreateCommand();
            cmd.CommandText = $"SELECT {(top == null ? "" : "TOP " + top)} * FROM " + this.Table + (string.IsNullOrEmpty(orderBy) ? "" : " " + orderBy);
            contextInstance.OpenIfNot();
            var reader = cmd.ExecuteReader();
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    var obj = new T();
                    foreach (var item in this.Columns)
                    {
                        try
                        {
                            PropertyInfo pi = typeof(T).GetProperty(item);
                            if ((reader[pi.Name] != System.DBNull.Value))
                                pi.SetValue(obj, reader[pi.Name], null);

                        }
                        catch (Exception e)
                        {

                        }

                    }
                    PropertyInfo idProp = typeof(T).GetProperty("Id");
                    if (idProp != null)
                    {
                        idProp.SetValue(obj, reader["Id"], null);

                    }
                    Locals.Add(obj);
                    yield return obj;
                }
            }
            contextInstance.dbConnection.Close();
        }


        const string DELETE = "DELETE FROM $$$TABLE$$$ WHERE Id=$$$ID$$$";
        const string INSERT = "INSERT INTO $$$TABLE$$$($$$COLUMNS$$$) VALUES($$$VALUES$$$)";
        const string UPDATE = "UPDATE $$$TABLE$$$ SET $$$VALUES$$$ WHERE Id=$$$ID$$$$";
        public override IEnumerable<string> BuildPendingActions()
        {
            if (_pendindActions.Count > 0)
            {
                foreach (var item in _pendindActions)
                {
                    string baseStr = "";
                    switch (item.Item2)
                    {
                        case Action.CREATE:
                            baseStr = INSERT.Replace("$$$TABLE$$$", this.Table)
                                            .Replace("$$$COLUMNS$$$", Columns.Aggregate("", (seed, next) => { return string.IsNullOrEmpty(seed) ? "[" + next + "]" : seed + "," + "[" + next + "]"; }))
                                            .Replace("$$$VALUES$$$", _ValuesFor(item.Item1, false));

                            break;
                        case Action.DELETE:
                            baseStr = DELETE.Replace("$$$TABLE$$$", this.Table)
                                            .Replace("$$$ID$$$", item.Item1.Id.ToString());
                            break;
                        case Action.UPDATE:
                            baseStr = UPDATE.Replace("$$$TABLE$$$", this.Table)
                                            .Replace("$$$ID$$$", item.Item1.Id.ToString())
                                            .Replace("$$$VALUES$$$", _ValuesFor(item.Item1));
                            break;
                        default:
                            break;
                    }

                    yield return baseStr;
                }
            }
            _pendindActions.Clear();
        }
        public List<T> ToList()
        {
            return this.Where(c=>1==1).ToList();
        }
        private string _ValuesFor(T item1, bool update = true)
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (var column in Columns)
            {
                try
                {
                    var prop = item1.GetType().GetProperties().FirstOrDefault(p => p.Name == column);
                    var value = prop.GetValue(item1)?.ToString();
                    if (prop.PropertyType == typeof(DateTime))
                    {
                        value = ((DateTime)prop.GetValue(item1)).ToString("yyyy-MM-dd HH:mm");
                        value = $"#{value}#";
                    }
                    else if (prop.PropertyType == typeof(string))
                    {
                        value = $"'{value}'";
                    }
                    if (string.IsNullOrEmpty(value))
                    {
                        value = "NULL";
                    }
                    if (update)
                    {
                        stringBuilder.Append($"{column}={value}");
                    }
                    else
                    {
                        stringBuilder.Append(value);
                    }
                    stringBuilder.Append(",");

                }
                catch (Exception)
                {

                    throw new Exception(column);
                };
            }

            return stringBuilder.Remove(stringBuilder.Length - 1, 1).ToString();
        }

        public BaseSet()
        {
            object hasTableAtt = typeof(T).GetCustomAttributes(typeof(System.ComponentModel.DataAnnotations.Schema.TableAttribute), false).FirstOrDefault();
            if (hasTableAtt != null)
            {
                Table = ((System.ComponentModel.DataAnnotations.Schema.TableAttribute)hasTableAtt).Name;
            }
            else
            {
                Table = typeof(T).Name;
            }
            setColumns();
        }

        public BaseSet(ContextBase context) : this()
        {
            this.contextInstance = context;
        }
        public override IEnumerator<T> GetEnumerator()
        {
            return _Get();
        }
        private void setColumns()
        {
            this.Columns = typeof(T)
                .GetProperties()
                .Where(p => p.GetSetMethod(false) != null && p.Name != "Id")
                .Select(p => p.Name).ToArray();
        }

        public string Table { get; internal set; }
        public string[] Columns { get; private set; }

    }

    //public class TextFileHandler
    //{
    //    public IEnumerable<string> ReadFile(string path)
    //    {
    //        lock (_lockObject)
    //        {
    //            Thread.Sleep(20);
    //            return File.ReadLines(path).Skip(1);
    //        }
    //    }
    //    public static Object _lockObject { get; set; } = new object();
    //    public void AppendLine(string path, string line)
    //    {
    //        lock (_lockObject)
    //        {
    //            File.AppendAllLines(path, new List<string> { line });
    //            Thread.Sleep(20);
    //        }
    //    }
    //}

    //public class EventsTextFileHandler
    //{
    //    public string Path { get; protected set; }
    //    TextFileHandler handler { get; set; } = new TextFileHandler();
    //    public EventsTextFileHandler(string path = null)
    //    {
    //        if (path == null)
    //        {
    //            path = _Extentions.GetDatabaseDir();
    //        }
    //        this.Path = path;
    //    }

    //    public IEnumerable<BaseContent> GetAll()
    //    {
    //        var lines = handler.ReadFile(this.Path);
    //        return lines.ToList().Select(c => c.Split(';')).Select(columns => new BaseContent
    //        {
    //            Id = columns[0],
    //            Path = columns[1],
    //            Author = columns[2],
    //            Date = columns[3],
    //            Tags = columns[4],
    //            Header = columns[5],
    //            ImageFile = columns[6],
    //            Describer = columns[7]
    //        });
    //    }


    //}
}