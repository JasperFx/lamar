namespace StructureMap.Testing.Examples
{
    #region sample_evil-singleton
    public class EvilSingleton
    {
        public static readonly EvilSingleton Instance = new EvilSingleton();

        private EvilSingleton()
        {
        }

        public void DoSomething()
        {
            // do something with the static data here
        }
    }


    public class EvilSingletonUser
    {
        public void DoWork()
        {
            EvilSingleton.Instance.DoSomething();
        }
    }

    #endregion

    #region sample_no-singleton
    public class SingletonService { }

    public class SingletonUser
    {
        public SingletonUser(SingletonService singleton)
        {
        }
    }

    public class SingletonScopeRegistry : Registry
    {
        public SingletonScopeRegistry()
        {
            For<SingletonService>().Singleton();
        }
    }
    #endregion
}