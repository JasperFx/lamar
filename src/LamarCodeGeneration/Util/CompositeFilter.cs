namespace LamarCodeGeneration.Util;

internal class CompositeFilter<T>
{
    private readonly CompositePredicate<T> _excludes = new();
    private readonly CompositePredicate<T> _includes = new();

    internal CompositePredicate<T> Includes
    {
        get => _includes;
        set { }
    }

    internal CompositePredicate<T> Excludes
    {
        get => _excludes;
        set { }
    }

    internal bool Matches(T target)
    {
        return Includes.MatchesAny(target) && Excludes.DoesNotMatcheAny(target);
    }
}