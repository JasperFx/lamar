using System;
using System.Data.SqlClient;
using Lamar;

namespace StructureMap.Testing.Widget
{
    // SAMPLE: IWidget
    public interface IWidget
    {
        void DoSomething();
    }
    // ENDSAMPLE

    // SAMPLE: inline-dependencies-ColorWidget
    public class ColorWidget : IWidget
    {
        public ColorWidget(string color)
        {
            Color = color;
        }

        public string Color { get; }

        #region ICloneable Members

        public object Clone()
        {
            return MemberwiseClone();
        }

        #endregion

        #region IWidget Members

        public void DoSomething()
        {
        }

        #endregion

        public override bool Equals(object obj)
        {
            if (this == obj) return true;
            var colorWidget = obj as ColorWidget;
            if (colorWidget == null) return false;
            return Equals(Color, colorWidget.Color);
        }

        public override int GetHashCode()
        {
            return Color != null ? Color.GetHashCode() : 0;
        }

        public override string ToString()
        {
            return $"Color: {Color}";
        }
    }
    // ENDSAMPLE

    public class AWidget : IWidget
    {
        #region ICloneable Members

        public object Clone()
        {
            return MemberwiseClone();
        }

        #endregion

        #region IWidget Members

        public void DoSomething()
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    public class NotPluggableWidget : IWidget
    {
        private readonly string _name;

        public NotPluggableWidget(string name)
        {
            _name = name;
        }


        public string Name { get { return _name; } }

        #region IWidget Members

        public void DoSomething()
        {
        }

        #endregion
    }

    public class MoneyWidget : IWidget
    {
        public double Amount { get; set; }

        #region IWidget Members

        public void DoSomething()
        {
        }

        #endregion
    }


    public class ConfigurationWidget : IWidget
    {
        public ConfigurationWidget(string String, string String2, int Int, long Long, byte Byte, double Double,
                                   bool Bool)
        {
            this.String = String;
            this.String2 = String2;
            this.Int = Int;
            this.Long = Long;
            this.Byte = Byte;
            this.Double = Double;
            this.Bool = Bool;
        }


        public string String { get; set; }

        public string String2 { get; set; }


        public int Int { get; set; }


        public byte Byte { get; set; }


        public long Long { get; set; }


        public double Double { get; set; }


        public bool Bool { get; set; }

        #region IWidget Members

        public void DoSomething()
        {
        }

        #endregion

        [ValidationMethod]
        public void Validate()
        {
            // Throw an exception if Long = 5
            if (Long == 5)
            {
                throw new Exception("Long should not equal 5");
            }
        }

        [ValidationMethod]
        public void Validate2()
        {
        }
    }

    public class DatabaseSettings
    {
        public string ConnectionString { get; set; }
    }

    public class DatabaseUsingService
    {
        private readonly DatabaseSettings _settings;

        public DatabaseUsingService(DatabaseSettings settings)
        {
            _settings = settings;
        }

        [ValidationMethod]
        public void Validate()
        {
            // For *now*, Lamar requires validate methods be synchronous
            using (var conn = new SqlConnection(_settings.ConnectionString))
            {
                // If this blows up, the environment check fails:)
                conn.Open();
            }
        }
    }
}