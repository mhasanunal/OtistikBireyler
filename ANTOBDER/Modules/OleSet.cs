using System;
using System.Collections.Generic;
using System.Linq;

namespace ANTOBDER.Modules
{
    public abstract class OleSet<T, TKey> : ICollection<T>
        where T : IEntity<TKey>
    {
        internal List<Tuple<T, Action>> _pendindActions = new List<Tuple<T, Action>>();
        protected HashSet<T> Locals = new HashSet<T>();


        public abstract IEnumerable<string> BuildPendingActions();
        internal void Set(HashSet<T> set)
        {
            Locals = set;
        }
        public int Count => Locals.Count;

        public bool IsReadOnly => false;

        public void Add(T item)
        {
            _pendindActions.Add(new Tuple<T, Action>(item, Action.CREATE));
        }

        public void Clear()
        {
            _pendindActions = this.Select(i => new Tuple<T, Action>(i, Action.DELETE)).ToList();
            Locals = new HashSet<T>();
        }

        public bool Contains(T item)
        {
            return Locals.Contains(item);
        }
        public T this[int index]
        {
            get { return this.Locals.ElementAt(index); }
        }
        public void CopyTo(T[] array, int arrayIndex)
        {
            for (int i = arrayIndex; i < this.Count; i++)
            {
                array[i] = this[i];
            }
        }

        public virtual IEnumerator<T> GetEnumerator()
        {
            return Locals.GetEnumerator();
        }

        public bool Remove(T item)
        {
            if (Locals.Contains(item))
            {
                _pendindActions.Add(new Tuple<T, Action>(item, Action.DELETE));
                Locals = Locals.Except(new HashSet<T> { item }).ToHashSet();
                return true;
            }
            return false;

        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
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