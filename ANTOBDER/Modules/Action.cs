namespace ANTOBDER.Modules
{
    public enum Action
    {
        CREATE,
        DELETE,
        UPDATE,
        READ
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