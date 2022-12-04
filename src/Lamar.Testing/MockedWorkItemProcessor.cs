using System;

namespace Lamar.Testing;

public class WorkItem
{
    public DateTime Started { get; set; }
}

public interface IClock
{
    DateTime Now();
}

public class Clock : IClock
{
    public DateTime Now()
    {
        return DateTime.UtcNow;
    }
}

public class DisposableClock : IClock, IDisposable
{
    public bool WasDisposed { get; set; }

    public DateTime Now()
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
        WasDisposed = true;
    }
}

public class MockedWorkItemProcessor
{
    private readonly IClock _clock;

    public MockedWorkItemProcessor(IClock clock)
    {
        _clock = clock;
    }

    public void CheckItem(WorkItem item)
    {
        if (_clock.Now().Subtract(item.Started).Days > 5)
        {
            // yell at the developer
        }
    }
}

public class PushBasedWorkItemProcessor
{
    public void CheckItem(WorkItem item)
    {
        CheckItem(item, DateTime.UtcNow);
    }

    private void CheckItem(WorkItem item, DateTime utcNow)
    {
        if (utcNow.Subtract(item.Started).Days > 5)
        {
            // yell at the developer
        }
    }
}