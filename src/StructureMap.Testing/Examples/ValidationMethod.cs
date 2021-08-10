using StructureMap.Configuration.DSL;

namespace StructureMap.Testing.Examples
{
    public class ValidationMethod
    {
        public interface IDatabase
        {
            
        }

        #region sample_validation-method-usage
        public class Database : IDatabase
        {
            [ValidationMethod]
            public void TryToConnect()
            {
                // try to open a connection to the configured
                // database connection string

                // throw an exception if the database cannot
                // be reached
            }
        }
        #endregion

        public class DatabaseRegistry : Registry
        {
            public DatabaseRegistry()
            {
                For<IDatabase>().Use<Database>();
            }
        }
    }
}