namespace StructureMap.Testing.Examples
{
    public class DependencyInjectionSample
    {
        #region sample_basic-dependency-injection
        public interface IDatabase { }

        public class DatabaseUser
        {
            // Using Constructor Injection
            public DatabaseUser(IDatabase database)
            {
            }
        }

        public class OtherDatabaseUser
        {
            // Setter Injection
            public IDatabase Database { get; set; }
        }

        #endregion

        #region sample_basic-service-location
        public class ThirdDatabaseUser
        {
            private IDatabase _database;

            public ThirdDatabaseUser(IContainer container)
            {
                // This is service location
                _database = container.GetInstance<IDatabase>();
            }
        }

        #endregion
    }
}