using System;
using System.Collections.Generic;

namespace Lamar.Util.TextWriting
{
    public class ColumnSet
    {
        private readonly IList<IColumn> _columns = new List<IColumn>();

        public ColumnSet(int count)
        {
            for (int i = 0; i < count - 1; i++)
            {
                _columns.Add(new Column(ColumnJustification.left, 0, 5));
            }

            _columns.Add(new Column(ColumnJustification.left, 0, 0));
        }

        public ColumnSet(params IColumn[] columns)
        {
            _columns.AddRange(columns);
        }

        public IEnumerable<IColumn> Columns
        {
            get { return _columns; }
        }

        public Line Add(params string[] contents)
        {
            if (contents.Length != _columns.Count)
            {
                throw new ArgumentOutOfRangeException("The length of the contents has to match the number of columns");
            }

            return new ColumnLine(_columns, contents);
        }
    }
}
