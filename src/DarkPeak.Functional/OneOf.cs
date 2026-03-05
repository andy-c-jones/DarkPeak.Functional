namespace DarkPeak.Functional;

/// <summary>
/// Provides static factory methods for creating OneOf instances.
/// </summary>
public static class OneOf
{
    public static OneOf<T1, T2> First<T1, T2>(T1 value) => value;
    public static OneOf<T1, T2> Second<T1, T2>(T2 value) => value;

    public static OneOf<T1, T2, T3> First<T1, T2, T3>(T1 value) => value;
    public static OneOf<T1, T2, T3> Second<T1, T2, T3>(T2 value) => value;
    public static OneOf<T1, T2, T3> Third<T1, T2, T3>(T3 value) => value;

    public static OneOf<T1, T2, T3, T4> First<T1, T2, T3, T4>(T1 value) => value;
    public static OneOf<T1, T2, T3, T4> Second<T1, T2, T3, T4>(T2 value) => value;
    public static OneOf<T1, T2, T3, T4> Third<T1, T2, T3, T4>(T3 value) => value;
    public static OneOf<T1, T2, T3, T4> Fourth<T1, T2, T3, T4>(T4 value) => value;

    public static OneOf<T1, T2, T3, T4, T5> First<T1, T2, T3, T4, T5>(T1 value) => value;
    public static OneOf<T1, T2, T3, T4, T5> Second<T1, T2, T3, T4, T5>(T2 value) => value;
    public static OneOf<T1, T2, T3, T4, T5> Third<T1, T2, T3, T4, T5>(T3 value) => value;
    public static OneOf<T1, T2, T3, T4, T5> Fourth<T1, T2, T3, T4, T5>(T4 value) => value;
    public static OneOf<T1, T2, T3, T4, T5> Fifth<T1, T2, T3, T4, T5>(T5 value) => value;

    public static OneOf<T1, T2, T3, T4, T5, T6> First<T1, T2, T3, T4, T5, T6>(T1 value) => value;
    public static OneOf<T1, T2, T3, T4, T5, T6> Second<T1, T2, T3, T4, T5, T6>(T2 value) => value;
    public static OneOf<T1, T2, T3, T4, T5, T6> Third<T1, T2, T3, T4, T5, T6>(T3 value) => value;
    public static OneOf<T1, T2, T3, T4, T5, T6> Fourth<T1, T2, T3, T4, T5, T6>(T4 value) => value;
    public static OneOf<T1, T2, T3, T4, T5, T6> Fifth<T1, T2, T3, T4, T5, T6>(T5 value) => value;
    public static OneOf<T1, T2, T3, T4, T5, T6> Sixth<T1, T2, T3, T4, T5, T6>(T6 value) => value;

    public static OneOf<T1, T2, T3, T4, T5, T6, T7> First<T1, T2, T3, T4, T5, T6, T7>(T1 value) => value;
    public static OneOf<T1, T2, T3, T4, T5, T6, T7> Second<T1, T2, T3, T4, T5, T6, T7>(T2 value) => value;
    public static OneOf<T1, T2, T3, T4, T5, T6, T7> Third<T1, T2, T3, T4, T5, T6, T7>(T3 value) => value;
    public static OneOf<T1, T2, T3, T4, T5, T6, T7> Fourth<T1, T2, T3, T4, T5, T6, T7>(T4 value) => value;
    public static OneOf<T1, T2, T3, T4, T5, T6, T7> Fifth<T1, T2, T3, T4, T5, T6, T7>(T5 value) => value;
    public static OneOf<T1, T2, T3, T4, T5, T6, T7> Sixth<T1, T2, T3, T4, T5, T6, T7>(T6 value) => value;
    public static OneOf<T1, T2, T3, T4, T5, T6, T7> Seventh<T1, T2, T3, T4, T5, T6, T7>(T7 value) => value;

    public static OneOf<T1, T2, T3, T4, T5, T6, T7, T8> First<T1, T2, T3, T4, T5, T6, T7, T8>(T1 value) => value;
    public static OneOf<T1, T2, T3, T4, T5, T6, T7, T8> Second<T1, T2, T3, T4, T5, T6, T7, T8>(T2 value) => value;
    public static OneOf<T1, T2, T3, T4, T5, T6, T7, T8> Third<T1, T2, T3, T4, T5, T6, T7, T8>(T3 value) => value;
    public static OneOf<T1, T2, T3, T4, T5, T6, T7, T8> Fourth<T1, T2, T3, T4, T5, T6, T7, T8>(T4 value) => value;
    public static OneOf<T1, T2, T3, T4, T5, T6, T7, T8> Fifth<T1, T2, T3, T4, T5, T6, T7, T8>(T5 value) => value;
    public static OneOf<T1, T2, T3, T4, T5, T6, T7, T8> Sixth<T1, T2, T3, T4, T5, T6, T7, T8>(T6 value) => value;
    public static OneOf<T1, T2, T3, T4, T5, T6, T7, T8> Seventh<T1, T2, T3, T4, T5, T6, T7, T8>(T7 value) => value;
    public static OneOf<T1, T2, T3, T4, T5, T6, T7, T8> Eighth<T1, T2, T3, T4, T5, T6, T7, T8>(T8 value) => value;

}

/// <summary>
/// Represents a discriminated union of 2 possible types.
/// </summary>
public sealed record OneOf<T1, T2>
{
    private readonly object value;
    private readonly byte index;

    private OneOf(byte index, object value)
    {
        this.index = index;
        this.value = value;
    }

    public bool IsT1 => index == 1;
    public bool IsT2 => index == 2;

    public T1 AsT1 => index == 1
        ? (T1)value
        : throw new InvalidOperationException("Value is not T1.");
    public T2 AsT2 => index == 2
        ? (T2)value
        : throw new InvalidOperationException("Value is not T2.");

    public TResult Match<TResult>(Func<T1, TResult> t1, Func<T2, TResult> t2) =>
        index switch
        {
            1 => t1((T1)value),
            2 => t2((T2)value),
            _ => throw new InvalidOperationException("Invalid OneOf state.")
        };

    public Task<TResult> MatchAsync<TResult>(Func<T1, Task<TResult>> t1, Func<T2, Task<TResult>> t2) =>
        index switch
        {
            1 => t1((T1)value),
            2 => t2((T2)value),
            _ => throw new InvalidOperationException("Invalid OneOf state.")
        };

    public OneOf<T1Result, T2> MapFirst<T1Result>(Func<T1, T1Result> mapper) =>
        Match<OneOf<T1Result, T2>>(t1 => mapper(t1), t2 => t2);

    public OneOf<T1, T2Result> MapSecond<T2Result>(Func<T2, T2Result> mapper) =>
        Match<OneOf<T1, T2Result>>(t1 => t1, t2 => mapper(t2));

    public static implicit operator OneOf<T1, T2>(T1 value) => new(1, value!);
    public static implicit operator OneOf<T1, T2>(T2 value) => new(2, value!);

    public OneOf<T1, TResult> Select<TResult>(Func<T2, TResult> selector) =>
        MapSecond(selector);

    public OneOf<T1, TResult> SelectMany<TResult>(Func<T2, OneOf<T1, TResult>> selector) =>
        Match<OneOf<T1, TResult>>(t1 => t1, t2 => selector(t2));

    public OneOf<T1, TResult> SelectMany<TIntermediate, TResult>(
        Func<T2, OneOf<T1, TIntermediate>> selector,
        Func<T2, TIntermediate, TResult> projector) =>
        SelectMany(last => selector(last).Select(intermediate => projector(last, intermediate)));
}

/// <summary>
/// Represents a discriminated union of 3 possible types.
/// </summary>
public sealed record OneOf<T1, T2, T3>
{
    private readonly object value;
    private readonly byte index;

    private OneOf(byte index, object value)
    {
        this.index = index;
        this.value = value;
    }

    public bool IsT1 => index == 1;
    public bool IsT2 => index == 2;
    public bool IsT3 => index == 3;

    public T1 AsT1 => index == 1
        ? (T1)value
        : throw new InvalidOperationException("Value is not T1.");
    public T2 AsT2 => index == 2
        ? (T2)value
        : throw new InvalidOperationException("Value is not T2.");
    public T3 AsT3 => index == 3
        ? (T3)value
        : throw new InvalidOperationException("Value is not T3.");

    public TResult Match<TResult>(Func<T1, TResult> t1, Func<T2, TResult> t2, Func<T3, TResult> t3) =>
        index switch
        {
            1 => t1((T1)value),
            2 => t2((T2)value),
            3 => t3((T3)value),
            _ => throw new InvalidOperationException("Invalid OneOf state.")
        };

    public Task<TResult> MatchAsync<TResult>(Func<T1, Task<TResult>> t1, Func<T2, Task<TResult>> t2, Func<T3, Task<TResult>> t3) =>
        index switch
        {
            1 => t1((T1)value),
            2 => t2((T2)value),
            3 => t3((T3)value),
            _ => throw new InvalidOperationException("Invalid OneOf state.")
        };

    public OneOf<T1Result, T2, T3> MapFirst<T1Result>(Func<T1, T1Result> mapper) =>
        Match<OneOf<T1Result, T2, T3>>(t1 => mapper(t1), t2 => t2, t3 => t3);

    public OneOf<T1, T2Result, T3> MapSecond<T2Result>(Func<T2, T2Result> mapper) =>
        Match<OneOf<T1, T2Result, T3>>(t1 => t1, t2 => mapper(t2), t3 => t3);

    public OneOf<T1, T2, T3Result> MapThird<T3Result>(Func<T3, T3Result> mapper) =>
        Match<OneOf<T1, T2, T3Result>>(t1 => t1, t2 => t2, t3 => mapper(t3));

    public OneOf<T2, T3> ReduceFirst(Func<T1, OneOf<T2, T3>> reducer) =>
        Match<OneOf<T2, T3>>(t1 => reducer(t1), t2 => t2, t3 => t3);

    public OneOf<T1, T3> ReduceSecond(Func<T2, OneOf<T1, T3>> reducer) =>
        Match<OneOf<T1, T3>>(t1 => t1, t2 => reducer(t2), t3 => t3);

    public OneOf<T1, T2> ReduceThird(Func<T3, OneOf<T1, T2>> reducer) =>
        Match<OneOf<T1, T2>>(t1 => t1, t2 => t2, t3 => reducer(t3));

    public static implicit operator OneOf<T1, T2, T3>(T1 value) => new(1, value!);
    public static implicit operator OneOf<T1, T2, T3>(T2 value) => new(2, value!);
    public static implicit operator OneOf<T1, T2, T3>(T3 value) => new(3, value!);

    public OneOf<T1, T2, TResult> Select<TResult>(Func<T3, TResult> selector) =>
        MapThird(selector);

    public OneOf<T1, T2, TResult> SelectMany<TResult>(Func<T3, OneOf<T1, T2, TResult>> selector) =>
        Match<OneOf<T1, T2, TResult>>(t1 => t1, t2 => t2, t3 => selector(t3));

    public OneOf<T1, T2, TResult> SelectMany<TIntermediate, TResult>(
        Func<T3, OneOf<T1, T2, TIntermediate>> selector,
        Func<T3, TIntermediate, TResult> projector) =>
        SelectMany(last => selector(last).Select(intermediate => projector(last, intermediate)));
}

/// <summary>
/// Represents a discriminated union of 4 possible types.
/// </summary>
public sealed record OneOf<T1, T2, T3, T4>
{
    private readonly object value;
    private readonly byte index;

    private OneOf(byte index, object value)
    {
        this.index = index;
        this.value = value;
    }

    public bool IsT1 => index == 1;
    public bool IsT2 => index == 2;
    public bool IsT3 => index == 3;
    public bool IsT4 => index == 4;

    public T1 AsT1 => index == 1
        ? (T1)value
        : throw new InvalidOperationException("Value is not T1.");
    public T2 AsT2 => index == 2
        ? (T2)value
        : throw new InvalidOperationException("Value is not T2.");
    public T3 AsT3 => index == 3
        ? (T3)value
        : throw new InvalidOperationException("Value is not T3.");
    public T4 AsT4 => index == 4
        ? (T4)value
        : throw new InvalidOperationException("Value is not T4.");

    public TResult Match<TResult>(Func<T1, TResult> t1, Func<T2, TResult> t2, Func<T3, TResult> t3, Func<T4, TResult> t4) =>
        index switch
        {
            1 => t1((T1)value),
            2 => t2((T2)value),
            3 => t3((T3)value),
            4 => t4((T4)value),
            _ => throw new InvalidOperationException("Invalid OneOf state.")
        };

    public Task<TResult> MatchAsync<TResult>(Func<T1, Task<TResult>> t1, Func<T2, Task<TResult>> t2, Func<T3, Task<TResult>> t3, Func<T4, Task<TResult>> t4) =>
        index switch
        {
            1 => t1((T1)value),
            2 => t2((T2)value),
            3 => t3((T3)value),
            4 => t4((T4)value),
            _ => throw new InvalidOperationException("Invalid OneOf state.")
        };

    public OneOf<T1Result, T2, T3, T4> MapFirst<T1Result>(Func<T1, T1Result> mapper) =>
        Match<OneOf<T1Result, T2, T3, T4>>(t1 => mapper(t1), t2 => t2, t3 => t3, t4 => t4);

    public OneOf<T1, T2Result, T3, T4> MapSecond<T2Result>(Func<T2, T2Result> mapper) =>
        Match<OneOf<T1, T2Result, T3, T4>>(t1 => t1, t2 => mapper(t2), t3 => t3, t4 => t4);

    public OneOf<T1, T2, T3Result, T4> MapThird<T3Result>(Func<T3, T3Result> mapper) =>
        Match<OneOf<T1, T2, T3Result, T4>>(t1 => t1, t2 => t2, t3 => mapper(t3), t4 => t4);

    public OneOf<T1, T2, T3, T4Result> MapFourth<T4Result>(Func<T4, T4Result> mapper) =>
        Match<OneOf<T1, T2, T3, T4Result>>(t1 => t1, t2 => t2, t3 => t3, t4 => mapper(t4));

    public OneOf<T2, T3, T4> ReduceFirst(Func<T1, OneOf<T2, T3, T4>> reducer) =>
        Match<OneOf<T2, T3, T4>>(t1 => reducer(t1), t2 => t2, t3 => t3, t4 => t4);

    public OneOf<T1, T3, T4> ReduceSecond(Func<T2, OneOf<T1, T3, T4>> reducer) =>
        Match<OneOf<T1, T3, T4>>(t1 => t1, t2 => reducer(t2), t3 => t3, t4 => t4);

    public OneOf<T1, T2, T4> ReduceThird(Func<T3, OneOf<T1, T2, T4>> reducer) =>
        Match<OneOf<T1, T2, T4>>(t1 => t1, t2 => t2, t3 => reducer(t3), t4 => t4);

    public OneOf<T1, T2, T3> ReduceFourth(Func<T4, OneOf<T1, T2, T3>> reducer) =>
        Match<OneOf<T1, T2, T3>>(t1 => t1, t2 => t2, t3 => t3, t4 => reducer(t4));

    public static implicit operator OneOf<T1, T2, T3, T4>(T1 value) => new(1, value!);
    public static implicit operator OneOf<T1, T2, T3, T4>(T2 value) => new(2, value!);
    public static implicit operator OneOf<T1, T2, T3, T4>(T3 value) => new(3, value!);
    public static implicit operator OneOf<T1, T2, T3, T4>(T4 value) => new(4, value!);

    public OneOf<T1, T2, T3, TResult> Select<TResult>(Func<T4, TResult> selector) =>
        MapFourth(selector);

    public OneOf<T1, T2, T3, TResult> SelectMany<TResult>(Func<T4, OneOf<T1, T2, T3, TResult>> selector) =>
        Match<OneOf<T1, T2, T3, TResult>>(t1 => t1, t2 => t2, t3 => t3, t4 => selector(t4));

    public OneOf<T1, T2, T3, TResult> SelectMany<TIntermediate, TResult>(
        Func<T4, OneOf<T1, T2, T3, TIntermediate>> selector,
        Func<T4, TIntermediate, TResult> projector) =>
        SelectMany(last => selector(last).Select(intermediate => projector(last, intermediate)));
}

/// <summary>
/// Represents a discriminated union of 5 possible types.
/// </summary>
public sealed record OneOf<T1, T2, T3, T4, T5>
{
    private readonly object value;
    private readonly byte index;

    private OneOf(byte index, object value)
    {
        this.index = index;
        this.value = value;
    }

    public bool IsT1 => index == 1;
    public bool IsT2 => index == 2;
    public bool IsT3 => index == 3;
    public bool IsT4 => index == 4;
    public bool IsT5 => index == 5;

    public T1 AsT1 => index == 1
        ? (T1)value
        : throw new InvalidOperationException("Value is not T1.");
    public T2 AsT2 => index == 2
        ? (T2)value
        : throw new InvalidOperationException("Value is not T2.");
    public T3 AsT3 => index == 3
        ? (T3)value
        : throw new InvalidOperationException("Value is not T3.");
    public T4 AsT4 => index == 4
        ? (T4)value
        : throw new InvalidOperationException("Value is not T4.");
    public T5 AsT5 => index == 5
        ? (T5)value
        : throw new InvalidOperationException("Value is not T5.");

    public TResult Match<TResult>(Func<T1, TResult> t1, Func<T2, TResult> t2, Func<T3, TResult> t3, Func<T4, TResult> t4, Func<T5, TResult> t5) =>
        index switch
        {
            1 => t1((T1)value),
            2 => t2((T2)value),
            3 => t3((T3)value),
            4 => t4((T4)value),
            5 => t5((T5)value),
            _ => throw new InvalidOperationException("Invalid OneOf state.")
        };

    public Task<TResult> MatchAsync<TResult>(Func<T1, Task<TResult>> t1, Func<T2, Task<TResult>> t2, Func<T3, Task<TResult>> t3, Func<T4, Task<TResult>> t4, Func<T5, Task<TResult>> t5) =>
        index switch
        {
            1 => t1((T1)value),
            2 => t2((T2)value),
            3 => t3((T3)value),
            4 => t4((T4)value),
            5 => t5((T5)value),
            _ => throw new InvalidOperationException("Invalid OneOf state.")
        };

    public OneOf<T1Result, T2, T3, T4, T5> MapFirst<T1Result>(Func<T1, T1Result> mapper) =>
        Match<OneOf<T1Result, T2, T3, T4, T5>>(t1 => mapper(t1), t2 => t2, t3 => t3, t4 => t4, t5 => t5);

    public OneOf<T1, T2Result, T3, T4, T5> MapSecond<T2Result>(Func<T2, T2Result> mapper) =>
        Match<OneOf<T1, T2Result, T3, T4, T5>>(t1 => t1, t2 => mapper(t2), t3 => t3, t4 => t4, t5 => t5);

    public OneOf<T1, T2, T3Result, T4, T5> MapThird<T3Result>(Func<T3, T3Result> mapper) =>
        Match<OneOf<T1, T2, T3Result, T4, T5>>(t1 => t1, t2 => t2, t3 => mapper(t3), t4 => t4, t5 => t5);

    public OneOf<T1, T2, T3, T4Result, T5> MapFourth<T4Result>(Func<T4, T4Result> mapper) =>
        Match<OneOf<T1, T2, T3, T4Result, T5>>(t1 => t1, t2 => t2, t3 => t3, t4 => mapper(t4), t5 => t5);

    public OneOf<T1, T2, T3, T4, T5Result> MapFifth<T5Result>(Func<T5, T5Result> mapper) =>
        Match<OneOf<T1, T2, T3, T4, T5Result>>(t1 => t1, t2 => t2, t3 => t3, t4 => t4, t5 => mapper(t5));

    public OneOf<T2, T3, T4, T5> ReduceFirst(Func<T1, OneOf<T2, T3, T4, T5>> reducer) =>
        Match<OneOf<T2, T3, T4, T5>>(t1 => reducer(t1), t2 => t2, t3 => t3, t4 => t4, t5 => t5);

    public OneOf<T1, T3, T4, T5> ReduceSecond(Func<T2, OneOf<T1, T3, T4, T5>> reducer) =>
        Match<OneOf<T1, T3, T4, T5>>(t1 => t1, t2 => reducer(t2), t3 => t3, t4 => t4, t5 => t5);

    public OneOf<T1, T2, T4, T5> ReduceThird(Func<T3, OneOf<T1, T2, T4, T5>> reducer) =>
        Match<OneOf<T1, T2, T4, T5>>(t1 => t1, t2 => t2, t3 => reducer(t3), t4 => t4, t5 => t5);

    public OneOf<T1, T2, T3, T5> ReduceFourth(Func<T4, OneOf<T1, T2, T3, T5>> reducer) =>
        Match<OneOf<T1, T2, T3, T5>>(t1 => t1, t2 => t2, t3 => t3, t4 => reducer(t4), t5 => t5);

    public OneOf<T1, T2, T3, T4> ReduceFifth(Func<T5, OneOf<T1, T2, T3, T4>> reducer) =>
        Match<OneOf<T1, T2, T3, T4>>(t1 => t1, t2 => t2, t3 => t3, t4 => t4, t5 => reducer(t5));

    public static implicit operator OneOf<T1, T2, T3, T4, T5>(T1 value) => new(1, value!);
    public static implicit operator OneOf<T1, T2, T3, T4, T5>(T2 value) => new(2, value!);
    public static implicit operator OneOf<T1, T2, T3, T4, T5>(T3 value) => new(3, value!);
    public static implicit operator OneOf<T1, T2, T3, T4, T5>(T4 value) => new(4, value!);
    public static implicit operator OneOf<T1, T2, T3, T4, T5>(T5 value) => new(5, value!);

    public OneOf<T1, T2, T3, T4, TResult> Select<TResult>(Func<T5, TResult> selector) =>
        MapFifth(selector);

    public OneOf<T1, T2, T3, T4, TResult> SelectMany<TResult>(Func<T5, OneOf<T1, T2, T3, T4, TResult>> selector) =>
        Match<OneOf<T1, T2, T3, T4, TResult>>(t1 => t1, t2 => t2, t3 => t3, t4 => t4, t5 => selector(t5));

    public OneOf<T1, T2, T3, T4, TResult> SelectMany<TIntermediate, TResult>(
        Func<T5, OneOf<T1, T2, T3, T4, TIntermediate>> selector,
        Func<T5, TIntermediate, TResult> projector) =>
        SelectMany(last => selector(last).Select(intermediate => projector(last, intermediate)));
}

/// <summary>
/// Represents a discriminated union of 6 possible types.
/// </summary>
public sealed record OneOf<T1, T2, T3, T4, T5, T6>
{
    private readonly object value;
    private readonly byte index;

    private OneOf(byte index, object value)
    {
        this.index = index;
        this.value = value;
    }

    public bool IsT1 => index == 1;
    public bool IsT2 => index == 2;
    public bool IsT3 => index == 3;
    public bool IsT4 => index == 4;
    public bool IsT5 => index == 5;
    public bool IsT6 => index == 6;

    public T1 AsT1 => index == 1
        ? (T1)value
        : throw new InvalidOperationException("Value is not T1.");
    public T2 AsT2 => index == 2
        ? (T2)value
        : throw new InvalidOperationException("Value is not T2.");
    public T3 AsT3 => index == 3
        ? (T3)value
        : throw new InvalidOperationException("Value is not T3.");
    public T4 AsT4 => index == 4
        ? (T4)value
        : throw new InvalidOperationException("Value is not T4.");
    public T5 AsT5 => index == 5
        ? (T5)value
        : throw new InvalidOperationException("Value is not T5.");
    public T6 AsT6 => index == 6
        ? (T6)value
        : throw new InvalidOperationException("Value is not T6.");

    public TResult Match<TResult>(Func<T1, TResult> t1, Func<T2, TResult> t2, Func<T3, TResult> t3, Func<T4, TResult> t4, Func<T5, TResult> t5, Func<T6, TResult> t6) =>
        index switch
        {
            1 => t1((T1)value),
            2 => t2((T2)value),
            3 => t3((T3)value),
            4 => t4((T4)value),
            5 => t5((T5)value),
            6 => t6((T6)value),
            _ => throw new InvalidOperationException("Invalid OneOf state.")
        };

    public Task<TResult> MatchAsync<TResult>(Func<T1, Task<TResult>> t1, Func<T2, Task<TResult>> t2, Func<T3, Task<TResult>> t3, Func<T4, Task<TResult>> t4, Func<T5, Task<TResult>> t5, Func<T6, Task<TResult>> t6) =>
        index switch
        {
            1 => t1((T1)value),
            2 => t2((T2)value),
            3 => t3((T3)value),
            4 => t4((T4)value),
            5 => t5((T5)value),
            6 => t6((T6)value),
            _ => throw new InvalidOperationException("Invalid OneOf state.")
        };

    public OneOf<T1Result, T2, T3, T4, T5, T6> MapFirst<T1Result>(Func<T1, T1Result> mapper) =>
        Match<OneOf<T1Result, T2, T3, T4, T5, T6>>(t1 => mapper(t1), t2 => t2, t3 => t3, t4 => t4, t5 => t5, t6 => t6);

    public OneOf<T1, T2Result, T3, T4, T5, T6> MapSecond<T2Result>(Func<T2, T2Result> mapper) =>
        Match<OneOf<T1, T2Result, T3, T4, T5, T6>>(t1 => t1, t2 => mapper(t2), t3 => t3, t4 => t4, t5 => t5, t6 => t6);

    public OneOf<T1, T2, T3Result, T4, T5, T6> MapThird<T3Result>(Func<T3, T3Result> mapper) =>
        Match<OneOf<T1, T2, T3Result, T4, T5, T6>>(t1 => t1, t2 => t2, t3 => mapper(t3), t4 => t4, t5 => t5, t6 => t6);

    public OneOf<T1, T2, T3, T4Result, T5, T6> MapFourth<T4Result>(Func<T4, T4Result> mapper) =>
        Match<OneOf<T1, T2, T3, T4Result, T5, T6>>(t1 => t1, t2 => t2, t3 => t3, t4 => mapper(t4), t5 => t5, t6 => t6);

    public OneOf<T1, T2, T3, T4, T5Result, T6> MapFifth<T5Result>(Func<T5, T5Result> mapper) =>
        Match<OneOf<T1, T2, T3, T4, T5Result, T6>>(t1 => t1, t2 => t2, t3 => t3, t4 => t4, t5 => mapper(t5), t6 => t6);

    public OneOf<T1, T2, T3, T4, T5, T6Result> MapSixth<T6Result>(Func<T6, T6Result> mapper) =>
        Match<OneOf<T1, T2, T3, T4, T5, T6Result>>(t1 => t1, t2 => t2, t3 => t3, t4 => t4, t5 => t5, t6 => mapper(t6));

    public OneOf<T2, T3, T4, T5, T6> ReduceFirst(Func<T1, OneOf<T2, T3, T4, T5, T6>> reducer) =>
        Match<OneOf<T2, T3, T4, T5, T6>>(t1 => reducer(t1), t2 => t2, t3 => t3, t4 => t4, t5 => t5, t6 => t6);

    public OneOf<T1, T3, T4, T5, T6> ReduceSecond(Func<T2, OneOf<T1, T3, T4, T5, T6>> reducer) =>
        Match<OneOf<T1, T3, T4, T5, T6>>(t1 => t1, t2 => reducer(t2), t3 => t3, t4 => t4, t5 => t5, t6 => t6);

    public OneOf<T1, T2, T4, T5, T6> ReduceThird(Func<T3, OneOf<T1, T2, T4, T5, T6>> reducer) =>
        Match<OneOf<T1, T2, T4, T5, T6>>(t1 => t1, t2 => t2, t3 => reducer(t3), t4 => t4, t5 => t5, t6 => t6);

    public OneOf<T1, T2, T3, T5, T6> ReduceFourth(Func<T4, OneOf<T1, T2, T3, T5, T6>> reducer) =>
        Match<OneOf<T1, T2, T3, T5, T6>>(t1 => t1, t2 => t2, t3 => t3, t4 => reducer(t4), t5 => t5, t6 => t6);

    public OneOf<T1, T2, T3, T4, T6> ReduceFifth(Func<T5, OneOf<T1, T2, T3, T4, T6>> reducer) =>
        Match<OneOf<T1, T2, T3, T4, T6>>(t1 => t1, t2 => t2, t3 => t3, t4 => t4, t5 => reducer(t5), t6 => t6);

    public OneOf<T1, T2, T3, T4, T5> ReduceSixth(Func<T6, OneOf<T1, T2, T3, T4, T5>> reducer) =>
        Match<OneOf<T1, T2, T3, T4, T5>>(t1 => t1, t2 => t2, t3 => t3, t4 => t4, t5 => t5, t6 => reducer(t6));

    public static implicit operator OneOf<T1, T2, T3, T4, T5, T6>(T1 value) => new(1, value!);
    public static implicit operator OneOf<T1, T2, T3, T4, T5, T6>(T2 value) => new(2, value!);
    public static implicit operator OneOf<T1, T2, T3, T4, T5, T6>(T3 value) => new(3, value!);
    public static implicit operator OneOf<T1, T2, T3, T4, T5, T6>(T4 value) => new(4, value!);
    public static implicit operator OneOf<T1, T2, T3, T4, T5, T6>(T5 value) => new(5, value!);
    public static implicit operator OneOf<T1, T2, T3, T4, T5, T6>(T6 value) => new(6, value!);

    public OneOf<T1, T2, T3, T4, T5, TResult> Select<TResult>(Func<T6, TResult> selector) =>
        MapSixth(selector);

    public OneOf<T1, T2, T3, T4, T5, TResult> SelectMany<TResult>(Func<T6, OneOf<T1, T2, T3, T4, T5, TResult>> selector) =>
        Match<OneOf<T1, T2, T3, T4, T5, TResult>>(t1 => t1, t2 => t2, t3 => t3, t4 => t4, t5 => t5, t6 => selector(t6));

    public OneOf<T1, T2, T3, T4, T5, TResult> SelectMany<TIntermediate, TResult>(
        Func<T6, OneOf<T1, T2, T3, T4, T5, TIntermediate>> selector,
        Func<T6, TIntermediate, TResult> projector) =>
        SelectMany(last => selector(last).Select(intermediate => projector(last, intermediate)));
}

/// <summary>
/// Represents a discriminated union of 7 possible types.
/// </summary>
public sealed record OneOf<T1, T2, T3, T4, T5, T6, T7>
{
    private readonly object value;
    private readonly byte index;

    private OneOf(byte index, object value)
    {
        this.index = index;
        this.value = value;
    }

    public bool IsT1 => index == 1;
    public bool IsT2 => index == 2;
    public bool IsT3 => index == 3;
    public bool IsT4 => index == 4;
    public bool IsT5 => index == 5;
    public bool IsT6 => index == 6;
    public bool IsT7 => index == 7;

    public T1 AsT1 => index == 1
        ? (T1)value
        : throw new InvalidOperationException("Value is not T1.");
    public T2 AsT2 => index == 2
        ? (T2)value
        : throw new InvalidOperationException("Value is not T2.");
    public T3 AsT3 => index == 3
        ? (T3)value
        : throw new InvalidOperationException("Value is not T3.");
    public T4 AsT4 => index == 4
        ? (T4)value
        : throw new InvalidOperationException("Value is not T4.");
    public T5 AsT5 => index == 5
        ? (T5)value
        : throw new InvalidOperationException("Value is not T5.");
    public T6 AsT6 => index == 6
        ? (T6)value
        : throw new InvalidOperationException("Value is not T6.");
    public T7 AsT7 => index == 7
        ? (T7)value
        : throw new InvalidOperationException("Value is not T7.");

    public TResult Match<TResult>(Func<T1, TResult> t1, Func<T2, TResult> t2, Func<T3, TResult> t3, Func<T4, TResult> t4, Func<T5, TResult> t5, Func<T6, TResult> t6, Func<T7, TResult> t7) =>
        index switch
        {
            1 => t1((T1)value),
            2 => t2((T2)value),
            3 => t3((T3)value),
            4 => t4((T4)value),
            5 => t5((T5)value),
            6 => t6((T6)value),
            7 => t7((T7)value),
            _ => throw new InvalidOperationException("Invalid OneOf state.")
        };

    public Task<TResult> MatchAsync<TResult>(Func<T1, Task<TResult>> t1, Func<T2, Task<TResult>> t2, Func<T3, Task<TResult>> t3, Func<T4, Task<TResult>> t4, Func<T5, Task<TResult>> t5, Func<T6, Task<TResult>> t6, Func<T7, Task<TResult>> t7) =>
        index switch
        {
            1 => t1((T1)value),
            2 => t2((T2)value),
            3 => t3((T3)value),
            4 => t4((T4)value),
            5 => t5((T5)value),
            6 => t6((T6)value),
            7 => t7((T7)value),
            _ => throw new InvalidOperationException("Invalid OneOf state.")
        };

    public OneOf<T1Result, T2, T3, T4, T5, T6, T7> MapFirst<T1Result>(Func<T1, T1Result> mapper) =>
        Match<OneOf<T1Result, T2, T3, T4, T5, T6, T7>>(t1 => mapper(t1), t2 => t2, t3 => t3, t4 => t4, t5 => t5, t6 => t6, t7 => t7);

    public OneOf<T1, T2Result, T3, T4, T5, T6, T7> MapSecond<T2Result>(Func<T2, T2Result> mapper) =>
        Match<OneOf<T1, T2Result, T3, T4, T5, T6, T7>>(t1 => t1, t2 => mapper(t2), t3 => t3, t4 => t4, t5 => t5, t6 => t6, t7 => t7);

    public OneOf<T1, T2, T3Result, T4, T5, T6, T7> MapThird<T3Result>(Func<T3, T3Result> mapper) =>
        Match<OneOf<T1, T2, T3Result, T4, T5, T6, T7>>(t1 => t1, t2 => t2, t3 => mapper(t3), t4 => t4, t5 => t5, t6 => t6, t7 => t7);

    public OneOf<T1, T2, T3, T4Result, T5, T6, T7> MapFourth<T4Result>(Func<T4, T4Result> mapper) =>
        Match<OneOf<T1, T2, T3, T4Result, T5, T6, T7>>(t1 => t1, t2 => t2, t3 => t3, t4 => mapper(t4), t5 => t5, t6 => t6, t7 => t7);

    public OneOf<T1, T2, T3, T4, T5Result, T6, T7> MapFifth<T5Result>(Func<T5, T5Result> mapper) =>
        Match<OneOf<T1, T2, T3, T4, T5Result, T6, T7>>(t1 => t1, t2 => t2, t3 => t3, t4 => t4, t5 => mapper(t5), t6 => t6, t7 => t7);

    public OneOf<T1, T2, T3, T4, T5, T6Result, T7> MapSixth<T6Result>(Func<T6, T6Result> mapper) =>
        Match<OneOf<T1, T2, T3, T4, T5, T6Result, T7>>(t1 => t1, t2 => t2, t3 => t3, t4 => t4, t5 => t5, t6 => mapper(t6), t7 => t7);

    public OneOf<T1, T2, T3, T4, T5, T6, T7Result> MapSeventh<T7Result>(Func<T7, T7Result> mapper) =>
        Match<OneOf<T1, T2, T3, T4, T5, T6, T7Result>>(t1 => t1, t2 => t2, t3 => t3, t4 => t4, t5 => t5, t6 => t6, t7 => mapper(t7));

    public OneOf<T2, T3, T4, T5, T6, T7> ReduceFirst(Func<T1, OneOf<T2, T3, T4, T5, T6, T7>> reducer) =>
        Match<OneOf<T2, T3, T4, T5, T6, T7>>(t1 => reducer(t1), t2 => t2, t3 => t3, t4 => t4, t5 => t5, t6 => t6, t7 => t7);

    public OneOf<T1, T3, T4, T5, T6, T7> ReduceSecond(Func<T2, OneOf<T1, T3, T4, T5, T6, T7>> reducer) =>
        Match<OneOf<T1, T3, T4, T5, T6, T7>>(t1 => t1, t2 => reducer(t2), t3 => t3, t4 => t4, t5 => t5, t6 => t6, t7 => t7);

    public OneOf<T1, T2, T4, T5, T6, T7> ReduceThird(Func<T3, OneOf<T1, T2, T4, T5, T6, T7>> reducer) =>
        Match<OneOf<T1, T2, T4, T5, T6, T7>>(t1 => t1, t2 => t2, t3 => reducer(t3), t4 => t4, t5 => t5, t6 => t6, t7 => t7);

    public OneOf<T1, T2, T3, T5, T6, T7> ReduceFourth(Func<T4, OneOf<T1, T2, T3, T5, T6, T7>> reducer) =>
        Match<OneOf<T1, T2, T3, T5, T6, T7>>(t1 => t1, t2 => t2, t3 => t3, t4 => reducer(t4), t5 => t5, t6 => t6, t7 => t7);

    public OneOf<T1, T2, T3, T4, T6, T7> ReduceFifth(Func<T5, OneOf<T1, T2, T3, T4, T6, T7>> reducer) =>
        Match<OneOf<T1, T2, T3, T4, T6, T7>>(t1 => t1, t2 => t2, t3 => t3, t4 => t4, t5 => reducer(t5), t6 => t6, t7 => t7);

    public OneOf<T1, T2, T3, T4, T5, T7> ReduceSixth(Func<T6, OneOf<T1, T2, T3, T4, T5, T7>> reducer) =>
        Match<OneOf<T1, T2, T3, T4, T5, T7>>(t1 => t1, t2 => t2, t3 => t3, t4 => t4, t5 => t5, t6 => reducer(t6), t7 => t7);

    public OneOf<T1, T2, T3, T4, T5, T6> ReduceSeventh(Func<T7, OneOf<T1, T2, T3, T4, T5, T6>> reducer) =>
        Match<OneOf<T1, T2, T3, T4, T5, T6>>(t1 => t1, t2 => t2, t3 => t3, t4 => t4, t5 => t5, t6 => t6, t7 => reducer(t7));

    public static implicit operator OneOf<T1, T2, T3, T4, T5, T6, T7>(T1 value) => new(1, value!);
    public static implicit operator OneOf<T1, T2, T3, T4, T5, T6, T7>(T2 value) => new(2, value!);
    public static implicit operator OneOf<T1, T2, T3, T4, T5, T6, T7>(T3 value) => new(3, value!);
    public static implicit operator OneOf<T1, T2, T3, T4, T5, T6, T7>(T4 value) => new(4, value!);
    public static implicit operator OneOf<T1, T2, T3, T4, T5, T6, T7>(T5 value) => new(5, value!);
    public static implicit operator OneOf<T1, T2, T3, T4, T5, T6, T7>(T6 value) => new(6, value!);
    public static implicit operator OneOf<T1, T2, T3, T4, T5, T6, T7>(T7 value) => new(7, value!);

    public OneOf<T1, T2, T3, T4, T5, T6, TResult> Select<TResult>(Func<T7, TResult> selector) =>
        MapSeventh(selector);

    public OneOf<T1, T2, T3, T4, T5, T6, TResult> SelectMany<TResult>(Func<T7, OneOf<T1, T2, T3, T4, T5, T6, TResult>> selector) =>
        Match<OneOf<T1, T2, T3, T4, T5, T6, TResult>>(t1 => t1, t2 => t2, t3 => t3, t4 => t4, t5 => t5, t6 => t6, t7 => selector(t7));

    public OneOf<T1, T2, T3, T4, T5, T6, TResult> SelectMany<TIntermediate, TResult>(
        Func<T7, OneOf<T1, T2, T3, T4, T5, T6, TIntermediate>> selector,
        Func<T7, TIntermediate, TResult> projector) =>
        SelectMany(last => selector(last).Select(intermediate => projector(last, intermediate)));
}

/// <summary>
/// Represents a discriminated union of 8 possible types.
/// </summary>
public sealed record OneOf<T1, T2, T3, T4, T5, T6, T7, T8>
{
    private readonly object value;
    private readonly byte index;

    private OneOf(byte index, object value)
    {
        this.index = index;
        this.value = value;
    }

    public bool IsT1 => index == 1;
    public bool IsT2 => index == 2;
    public bool IsT3 => index == 3;
    public bool IsT4 => index == 4;
    public bool IsT5 => index == 5;
    public bool IsT6 => index == 6;
    public bool IsT7 => index == 7;
    public bool IsT8 => index == 8;

    public T1 AsT1 => index == 1
        ? (T1)value
        : throw new InvalidOperationException("Value is not T1.");
    public T2 AsT2 => index == 2
        ? (T2)value
        : throw new InvalidOperationException("Value is not T2.");
    public T3 AsT3 => index == 3
        ? (T3)value
        : throw new InvalidOperationException("Value is not T3.");
    public T4 AsT4 => index == 4
        ? (T4)value
        : throw new InvalidOperationException("Value is not T4.");
    public T5 AsT5 => index == 5
        ? (T5)value
        : throw new InvalidOperationException("Value is not T5.");
    public T6 AsT6 => index == 6
        ? (T6)value
        : throw new InvalidOperationException("Value is not T6.");
    public T7 AsT7 => index == 7
        ? (T7)value
        : throw new InvalidOperationException("Value is not T7.");
    public T8 AsT8 => index == 8
        ? (T8)value
        : throw new InvalidOperationException("Value is not T8.");

    public TResult Match<TResult>(Func<T1, TResult> t1, Func<T2, TResult> t2, Func<T3, TResult> t3, Func<T4, TResult> t4, Func<T5, TResult> t5, Func<T6, TResult> t6, Func<T7, TResult> t7, Func<T8, TResult> t8) =>
        index switch
        {
            1 => t1((T1)value),
            2 => t2((T2)value),
            3 => t3((T3)value),
            4 => t4((T4)value),
            5 => t5((T5)value),
            6 => t6((T6)value),
            7 => t7((T7)value),
            8 => t8((T8)value),
            _ => throw new InvalidOperationException("Invalid OneOf state.")
        };

    public Task<TResult> MatchAsync<TResult>(Func<T1, Task<TResult>> t1, Func<T2, Task<TResult>> t2, Func<T3, Task<TResult>> t3, Func<T4, Task<TResult>> t4, Func<T5, Task<TResult>> t5, Func<T6, Task<TResult>> t6, Func<T7, Task<TResult>> t7, Func<T8, Task<TResult>> t8) =>
        index switch
        {
            1 => t1((T1)value),
            2 => t2((T2)value),
            3 => t3((T3)value),
            4 => t4((T4)value),
            5 => t5((T5)value),
            6 => t6((T6)value),
            7 => t7((T7)value),
            8 => t8((T8)value),
            _ => throw new InvalidOperationException("Invalid OneOf state.")
        };

    public OneOf<T1Result, T2, T3, T4, T5, T6, T7, T8> MapFirst<T1Result>(Func<T1, T1Result> mapper) =>
        Match<OneOf<T1Result, T2, T3, T4, T5, T6, T7, T8>>(t1 => mapper(t1), t2 => t2, t3 => t3, t4 => t4, t5 => t5, t6 => t6, t7 => t7, t8 => t8);

    public OneOf<T1, T2Result, T3, T4, T5, T6, T7, T8> MapSecond<T2Result>(Func<T2, T2Result> mapper) =>
        Match<OneOf<T1, T2Result, T3, T4, T5, T6, T7, T8>>(t1 => t1, t2 => mapper(t2), t3 => t3, t4 => t4, t5 => t5, t6 => t6, t7 => t7, t8 => t8);

    public OneOf<T1, T2, T3Result, T4, T5, T6, T7, T8> MapThird<T3Result>(Func<T3, T3Result> mapper) =>
        Match<OneOf<T1, T2, T3Result, T4, T5, T6, T7, T8>>(t1 => t1, t2 => t2, t3 => mapper(t3), t4 => t4, t5 => t5, t6 => t6, t7 => t7, t8 => t8);

    public OneOf<T1, T2, T3, T4Result, T5, T6, T7, T8> MapFourth<T4Result>(Func<T4, T4Result> mapper) =>
        Match<OneOf<T1, T2, T3, T4Result, T5, T6, T7, T8>>(t1 => t1, t2 => t2, t3 => t3, t4 => mapper(t4), t5 => t5, t6 => t6, t7 => t7, t8 => t8);

    public OneOf<T1, T2, T3, T4, T5Result, T6, T7, T8> MapFifth<T5Result>(Func<T5, T5Result> mapper) =>
        Match<OneOf<T1, T2, T3, T4, T5Result, T6, T7, T8>>(t1 => t1, t2 => t2, t3 => t3, t4 => t4, t5 => mapper(t5), t6 => t6, t7 => t7, t8 => t8);

    public OneOf<T1, T2, T3, T4, T5, T6Result, T7, T8> MapSixth<T6Result>(Func<T6, T6Result> mapper) =>
        Match<OneOf<T1, T2, T3, T4, T5, T6Result, T7, T8>>(t1 => t1, t2 => t2, t3 => t3, t4 => t4, t5 => t5, t6 => mapper(t6), t7 => t7, t8 => t8);

    public OneOf<T1, T2, T3, T4, T5, T6, T7Result, T8> MapSeventh<T7Result>(Func<T7, T7Result> mapper) =>
        Match<OneOf<T1, T2, T3, T4, T5, T6, T7Result, T8>>(t1 => t1, t2 => t2, t3 => t3, t4 => t4, t5 => t5, t6 => t6, t7 => mapper(t7), t8 => t8);

    public OneOf<T1, T2, T3, T4, T5, T6, T7, T8Result> MapEighth<T8Result>(Func<T8, T8Result> mapper) =>
        Match<OneOf<T1, T2, T3, T4, T5, T6, T7, T8Result>>(t1 => t1, t2 => t2, t3 => t3, t4 => t4, t5 => t5, t6 => t6, t7 => t7, t8 => mapper(t8));

    public OneOf<T2, T3, T4, T5, T6, T7, T8> ReduceFirst(Func<T1, OneOf<T2, T3, T4, T5, T6, T7, T8>> reducer) =>
        Match<OneOf<T2, T3, T4, T5, T6, T7, T8>>(t1 => reducer(t1), t2 => t2, t3 => t3, t4 => t4, t5 => t5, t6 => t6, t7 => t7, t8 => t8);

    public OneOf<T1, T3, T4, T5, T6, T7, T8> ReduceSecond(Func<T2, OneOf<T1, T3, T4, T5, T6, T7, T8>> reducer) =>
        Match<OneOf<T1, T3, T4, T5, T6, T7, T8>>(t1 => t1, t2 => reducer(t2), t3 => t3, t4 => t4, t5 => t5, t6 => t6, t7 => t7, t8 => t8);

    public OneOf<T1, T2, T4, T5, T6, T7, T8> ReduceThird(Func<T3, OneOf<T1, T2, T4, T5, T6, T7, T8>> reducer) =>
        Match<OneOf<T1, T2, T4, T5, T6, T7, T8>>(t1 => t1, t2 => t2, t3 => reducer(t3), t4 => t4, t5 => t5, t6 => t6, t7 => t7, t8 => t8);

    public OneOf<T1, T2, T3, T5, T6, T7, T8> ReduceFourth(Func<T4, OneOf<T1, T2, T3, T5, T6, T7, T8>> reducer) =>
        Match<OneOf<T1, T2, T3, T5, T6, T7, T8>>(t1 => t1, t2 => t2, t3 => t3, t4 => reducer(t4), t5 => t5, t6 => t6, t7 => t7, t8 => t8);

    public OneOf<T1, T2, T3, T4, T6, T7, T8> ReduceFifth(Func<T5, OneOf<T1, T2, T3, T4, T6, T7, T8>> reducer) =>
        Match<OneOf<T1, T2, T3, T4, T6, T7, T8>>(t1 => t1, t2 => t2, t3 => t3, t4 => t4, t5 => reducer(t5), t6 => t6, t7 => t7, t8 => t8);

    public OneOf<T1, T2, T3, T4, T5, T7, T8> ReduceSixth(Func<T6, OneOf<T1, T2, T3, T4, T5, T7, T8>> reducer) =>
        Match<OneOf<T1, T2, T3, T4, T5, T7, T8>>(t1 => t1, t2 => t2, t3 => t3, t4 => t4, t5 => t5, t6 => reducer(t6), t7 => t7, t8 => t8);

    public OneOf<T1, T2, T3, T4, T5, T6, T8> ReduceSeventh(Func<T7, OneOf<T1, T2, T3, T4, T5, T6, T8>> reducer) =>
        Match<OneOf<T1, T2, T3, T4, T5, T6, T8>>(t1 => t1, t2 => t2, t3 => t3, t4 => t4, t5 => t5, t6 => t6, t7 => reducer(t7), t8 => t8);

    public OneOf<T1, T2, T3, T4, T5, T6, T7> ReduceEighth(Func<T8, OneOf<T1, T2, T3, T4, T5, T6, T7>> reducer) =>
        Match<OneOf<T1, T2, T3, T4, T5, T6, T7>>(t1 => t1, t2 => t2, t3 => t3, t4 => t4, t5 => t5, t6 => t6, t7 => t7, t8 => reducer(t8));

    public static implicit operator OneOf<T1, T2, T3, T4, T5, T6, T7, T8>(T1 value) => new(1, value!);
    public static implicit operator OneOf<T1, T2, T3, T4, T5, T6, T7, T8>(T2 value) => new(2, value!);
    public static implicit operator OneOf<T1, T2, T3, T4, T5, T6, T7, T8>(T3 value) => new(3, value!);
    public static implicit operator OneOf<T1, T2, T3, T4, T5, T6, T7, T8>(T4 value) => new(4, value!);
    public static implicit operator OneOf<T1, T2, T3, T4, T5, T6, T7, T8>(T5 value) => new(5, value!);
    public static implicit operator OneOf<T1, T2, T3, T4, T5, T6, T7, T8>(T6 value) => new(6, value!);
    public static implicit operator OneOf<T1, T2, T3, T4, T5, T6, T7, T8>(T7 value) => new(7, value!);
    public static implicit operator OneOf<T1, T2, T3, T4, T5, T6, T7, T8>(T8 value) => new(8, value!);

    public OneOf<T1, T2, T3, T4, T5, T6, T7, TResult> Select<TResult>(Func<T8, TResult> selector) =>
        MapEighth(selector);

    public OneOf<T1, T2, T3, T4, T5, T6, T7, TResult> SelectMany<TResult>(Func<T8, OneOf<T1, T2, T3, T4, T5, T6, T7, TResult>> selector) =>
        Match<OneOf<T1, T2, T3, T4, T5, T6, T7, TResult>>(t1 => t1, t2 => t2, t3 => t3, t4 => t4, t5 => t5, t6 => t6, t7 => t7, t8 => selector(t8));

    public OneOf<T1, T2, T3, T4, T5, T6, T7, TResult> SelectMany<TIntermediate, TResult>(
        Func<T8, OneOf<T1, T2, T3, T4, T5, T6, T7, TIntermediate>> selector,
        Func<T8, TIntermediate, TResult> projector) =>
        SelectMany(last => selector(last).Select(intermediate => projector(last, intermediate)));
}
