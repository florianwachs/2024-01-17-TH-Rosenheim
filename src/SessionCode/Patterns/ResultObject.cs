using SessionCode.Artifacts;

namespace SessionCode.Patterns;

public class ResultObjectBefore
{
    public void DoStuff()
    {
        // Well ... do stuff
    }

    public bool DoEvenMoreStuff()
    {
        bool somethingBadHappened = Random.Shared.NextDouble() > 0.5;

        if (somethingBadHappened)
        {
            throw new InvalidOperationException("Something bad happened!!!");
        }

        var success = Random.Shared.Next(1, 10) > 5;
        return success;
    }
}

public class ResultObjectAfter
{
    public Maybe<bool> DoSomething()
    {
        bool somethingBadHappened = Random.Shared.NextDouble() > 0.5;

        if (somethingBadHappened)
        {
            return new ExceptionError<bool>(new InvalidOperationException("Something bad happened!!!"));
        }

        var success = Random.Shared.Next(1, 10) > 5;
        return new Something<bool>(success);
    }

    public void HandleDoSomething()
    {
        var result = DoSomething();

        switch (result)
        {
            case Something<bool> s:
                Console.WriteLine($"Yeahh {s.Value}");
                break;
            case ExceptionError<bool> ex:
                Console.WriteLine(ex.CapturedException.ToString());
                break;
        }
    }


    public Maybe<Customer> GetCustomer(Guid id)
    {
        bool somethingBadHappened = Random.Shared.NextDouble() > 0.5;

        if (somethingBadHappened)
        {
            return new ExceptionError<Customer>(new InvalidOperationException("Something bad happened!!!"));
        }

        var success = Random.Shared.Next(1, 10) > 5;
        return success switch
        {
            true => new Something<Customer>(new Customer()),
            _ => new Nothing<Customer>()
        };
    }

    public void HandleGetCustomer()
    {
        var result = GetCustomer(Guid.NewGuid());

        var resultingStatusCode = result switch
        {
            Something<Customer> => 200,
            Nothing<Customer> => 404,
            Error<Customer> => 500,
        };
    }
}

#region Maybe

public abstract class Maybe<T>
{
}

public class Something<T> : Maybe<T>
{
    public Something(T value)
    {
        Value = value;
    }

    public T Value { get; init; }
}

public class Nothing<T> : Maybe<T>
{
}

public abstract class Error<T> : Maybe<T>
{
}

public class ExceptionError<T>(Exception exception) : Error<T>
{
    public Exception CapturedException { get; init; } = exception;
}

#endregion