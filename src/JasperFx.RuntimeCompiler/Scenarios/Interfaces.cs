namespace JasperFx.RuntimeCompiler.Scenarios
{
    public interface IAction<T>
    {
        void DoStuff(T arg1);
    }

    public interface IBuilds<T>
    {
        T Build();
    }
    
    public interface IAction<TResult, T1>
    {
        TResult Create(T1 arg1);
    }
    
    public interface IAction<TResult, T1, T2>
    {
        TResult Create(T1 arg1, T2 arg2);
    }
    
    public interface IAction<TResult, T1, T2, T3>
    {
        TResult Create(T1 arg1, T2 arg2, T3 arg3);
    }
}