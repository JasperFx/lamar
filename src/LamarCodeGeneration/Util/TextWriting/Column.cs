using System;
using System.IO;

namespace LamarCodeGeneration.Util.TextWriting
{
    public class Column : IColumn
    {
        private readonly ColumnJustification _justification;

        public Column(ColumnJustification justification, int leftPadding, int rightPadding)
        {
            _justification = justification;
            RightPadding = rightPadding;
            LeftPadding = leftPadding;
        }

        public int RightPadding { get; set; }
        public int LeftPadding { get; set; }

        public bool Equals(Column other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other._justification, _justification) && other.RightPadding == RightPadding && other.LeftPadding == LeftPadding;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (Column)) return false;
            return Equals((Column) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int result = _justification.GetHashCode();
                result = (result*397) ^ RightPadding;
                result = (result*397) ^ LeftPadding;
                return result;
            }
        }

        public override string ToString()
        {
            return string.Format("Justification: {0}, RightPadding: {1}, LeftPadding: {2}", _justification, RightPadding, LeftPadding);
        }

        public virtual void WatchData(string contents)
        {
            if (contents.Length > _maxWidth)
            {
                _maxWidth = contents.Length;
            }
        }

        private int _maxWidth;

        public int Width
        {
            get
            {
                return _maxWidth + LeftPadding + RightPadding;
            }
        }

        public void Write(TextWriter writer, string text)
        {
            writer.Write(GetText(text));
        }

        public virtual string GetText(string text)
        {
            var answer = string.Empty;
            answer += string.Empty.PadRight(LeftPadding);
            if (_justification == ColumnJustification.left)
            {
                answer += text.PadRight(_maxWidth);
            }
            else
            {
                answer += text.PadLeft(_maxWidth);
            }

            answer += string.Empty.PadRight(RightPadding);

            return answer;
        }

        public void WriteToConsole(string text)
        {
            Write(Console.Out, text);
        }
    }
}
