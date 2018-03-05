using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Lamar.Util.TextWriting
{
    public class ColumnLine : Line
    {
        private readonly IColumn[] _columns;
        private readonly string[] _contents;

        public ColumnLine(IEnumerable<IColumn> columns, string[] contents)
        {
            _columns = columns.ToArray();
            _contents = contents;

            for (int i = 0; i < _columns.Count(); i++)
            {
                var column = _columns[i];
                var data = _contents[i];
                column.WatchData(data);
            }
        }


        public void WriteToConsole()
        {
            Write(Console.Out);
        }

        public void Write(TextWriter writer)
        {
            
            for (int i = 0; i < _columns.Count(); i++)
            {
                var column = _columns[i];
                var data = _contents[i];
                column.Write(writer, data);
            }

            writer.WriteLine();
        }

        public int Width
        {
            get { return _columns.Sum(x => x.Width); }
        }
    }
}