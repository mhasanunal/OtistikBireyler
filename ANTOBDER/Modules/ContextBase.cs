using ANTOBDER.Models;
using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Linq;
using System.Reflection;

namespace ANTOBDER.Modules
{
    public class ContextBase:IDisposable
    {
        internal OleDbConnection dbConnection;

        internal void OpenIfNot()
        {
            if (dbConnection.State != System.Data.ConnectionState.Open)
            {
                dbConnection.Open();
            }
        }

        public BaseSet<BaseContent, int> Contents
        {
            get;set;
        }
        public BaseSet<User, int> Users
        {
            get;set;
        }

        private IEnumerable<T> _Get<T>(string[] columns, string table, string orderBy = null)
            where T : class, new()
        {
            var cmd = dbConnection.CreateCommand();
            cmd.CommandText = "SELECT TOP 100 * FROM " + table + (string.IsNullOrEmpty(orderBy) ? "" : " " + orderBy);
            OpenIfNot();
            var reader = cmd.ExecuteReader();
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    var obj = new T();
                    foreach (var item in columns)
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
                    yield return obj;
                }
            }
            dbConnection.Close();
        }

        public ContextBase(string connectionStr = null)
        {
            Contents = new BaseSet<BaseContent, int>(this);
            Users = new BaseSet<User, int>(this);
            if (connectionStr == null)
            {
                connectionStr = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + _Extentions.GetDatabaseDir();
            }
            dbConnection = new OleDbConnection(connectionStr);
        }
        public int SaveChanges()
        {
            var cmd = dbConnection.CreateCommand();
            OpenIfNot();
            int save = 0;
            foreach (var action in Contents.BuildPendingActions())
            {
                cmd.CommandText = action;
                save += cmd.ExecuteNonQuery();

            }
            foreach (var action in Users.BuildPendingActions())
            {

                cmd.CommandText = action;
                save += cmd.ExecuteNonQuery();
            }
            dbConnection.Close();
            return save;
        }

        public void Dispose()
        {
            this.dbConnection.Dispose();
        }
    }

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