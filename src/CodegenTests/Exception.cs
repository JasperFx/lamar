using System;
using System.Threading.Tasks;
using Shouldly;

namespace CodegenTests;

public static class Exception<T> where T : Exception
{
    public static T ShouldBeThrownBy(Action action)
    {
        T exception = null;

        try
        {
            action();
        }
        catch (Exception e)
        {
            exception = e.ShouldBeOfType<T>();
        }

        if (exception == null)
        {
            throw new Exception("An exception was expected, but not thrown by the given action.");
        }

        return exception;
    }

    public static async Task<T> ShouldBeThrownBy(Func<Task> action)
    {
        T exception = null;

        try
        {
            await action();
        }
        catch (Exception e)
        {
            exception = e.ShouldBeOfType<T>();
        }

        if (exception == null)
        {
            throw new Exception("An exception was expected, but not thrown by the given action.");
        }

        return exception;
    }
}