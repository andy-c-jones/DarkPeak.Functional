namespace DarkPeak.Functional.Extensions;

/// <summary>
/// Extension methods for working with Option types.
/// </summary>
public static class OptionExtensions
{
    /// <summary>
    /// Converts a nullable reference type to an option.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="value">The nullable value.</param>
    /// <returns>Some if the value is not null, None otherwise.</returns>
    public static Option<T> ToOption<T>(this T? value) where T : class =>
        value is not null ? Option.Some(value) : Option.None<T>();

    /// <summary>
    /// Converts a nullable value type to an option.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="value">The nullable value.</param>
    /// <returns>Some if the value has a value, None otherwise.</returns>
    public static Option<T> ToOption<T>(this T? value) where T : struct =>
        value.HasValue ? Option.Some(value.Value) : Option.None<T>();

    /// <summary>
    /// Returns the first element of a sequence as an option, or None if the sequence is empty.
    /// </summary>
    /// <typeparam name="T">The type of the elements.</typeparam>
    /// <param name="source">The sequence.</param>
    /// <returns>Some with the first element, or None if the sequence is empty.</returns>
    public static Option<T> FirstOrNone<T>(this IEnumerable<T> source)
    {
        foreach (var item in source)
        {
            return Option.Some(item);
        }
        return Option.None<T>();
    }

    /// <summary>
    /// Returns the first element of a sequence that satisfies a condition as an option, or None if no such element is found.
    /// </summary>
    /// <typeparam name="T">The type of the elements.</typeparam>
    /// <param name="source">The sequence.</param>
    /// <param name="predicate">The condition to test.</param>
    /// <returns>Some with the first matching element, or None if no element matches.</returns>
    public static Option<T> FirstOrNone<T>(this IEnumerable<T> source, Func<T, bool> predicate)
    {
        foreach (var item in source)
        {
            if (predicate(item))
            {
                return Option.Some(item);
            }
        }
        return Option.None<T>();
    }

    /// <summary>
    /// Returns the single element of a sequence as an option, or None if the sequence is empty or contains more than one element.
    /// </summary>
    /// <typeparam name="T">The type of the elements.</typeparam>
    /// <param name="source">The sequence.</param>
    /// <returns>Some with the single element, or None if the sequence is empty or contains multiple elements.</returns>
    public static Option<T> SingleOrNone<T>(this IEnumerable<T> source)
    {
        using var enumerator = source.GetEnumerator();
        
        if (!enumerator.MoveNext())
        {
            return Option.None<T>();
        }

        var first = enumerator.Current;
        
        if (enumerator.MoveNext())
        {
            return Option.None<T>();
        }

        return Option.Some(first);
    }

    /// <summary>
    /// Returns the single element of a sequence that satisfies a condition as an option, or None if no such element exists or multiple elements match.
    /// </summary>
    /// <typeparam name="T">The type of the elements.</typeparam>
    /// <param name="source">The sequence.</param>
    /// <param name="predicate">The condition to test.</param>
    /// <returns>Some with the single matching element, or None if no element matches or multiple elements match.</returns>
    public static Option<T> SingleOrNone<T>(this IEnumerable<T> source, Func<T, bool> predicate) =>
        source.Where(predicate).SingleOrNone();

    /// <summary>
    /// Returns the last element of a sequence as an option, or None if the sequence is empty.
    /// </summary>
    /// <typeparam name="T">The type of the elements.</typeparam>
    /// <param name="source">The sequence.</param>
    /// <returns>Some with the last element, or None if the sequence is empty.</returns>
    public static Option<T> LastOrNone<T>(this IEnumerable<T> source)
    {
        using var enumerator = source.GetEnumerator();
        
        if (!enumerator.MoveNext())
        {
            return Option.None<T>();
        }

        var last = enumerator.Current;
        
        while (enumerator.MoveNext())
        {
            last = enumerator.Current;
        }

        return Option.Some(last);
    }

    /// <summary>
    /// Returns the last element of a sequence that satisfies a condition as an option, or None if no such element is found.
    /// </summary>
    /// <typeparam name="T">The type of the elements.</typeparam>
    /// <param name="source">The sequence.</param>
    /// <param name="predicate">The condition to test.</param>
    /// <returns>Some with the last matching element, or None if no element matches.</returns>
    public static Option<T> LastOrNone<T>(this IEnumerable<T> source, Func<T, bool> predicate) =>
        source.Where(predicate).LastOrNone();

    /// <summary>
    /// Tries to get a value from a dictionary as an option.
    /// </summary>
    /// <typeparam name="TKey">The type of the dictionary keys.</typeparam>
    /// <typeparam name="TValue">The type of the dictionary values.</typeparam>
    /// <param name="dictionary">The dictionary.</param>
    /// <param name="key">The key to look up.</param>
    /// <returns>Some with the value if the key exists, None otherwise.</returns>
    public static Option<TValue> TryGetValueAsOption<TKey, TValue>(
        this IDictionary<TKey, TValue> dictionary,
        TKey key) where TKey : notnull =>
        dictionary.TryGetValue(key, out var value) ? Option.Some(value) : Option.None<TValue>();

    /// <summary>
    /// Tries to get a value from a read-only dictionary as an option.
    /// </summary>
    /// <typeparam name="TKey">The type of the dictionary keys.</typeparam>
    /// <typeparam name="TValue">The type of the dictionary values.</typeparam>
    /// <param name="dictionary">The dictionary.</param>
    /// <param name="key">The key to look up.</param>
    /// <returns>Some with the value if the key exists, None otherwise.</returns>
    public static Option<TValue> TryGetValueAsOption<TKey, TValue>(
        this IReadOnlyDictionary<TKey, TValue> dictionary,
        TKey key) where TKey : notnull =>
        dictionary.TryGetValue(key, out var value) ? Option.Some(value) : Option.None<TValue>();

    /// <summary>
    /// Flattens a nested option (Option of Option) into a single option.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="option">The nested option.</param>
    /// <returns>A flattened option.</returns>
    public static Option<T> Flatten<T>(this Option<Option<T>> option) =>
        option.Bind(inner => inner);

    /// <summary>
    /// Filters out None values from a sequence of options and unwraps the Some values.
    /// This is useful for LINQ operations to automatically filter and unwrap in one step.
    /// </summary>
    /// <typeparam name="T">The type of the values.</typeparam>
    /// <param name="source">The sequence of options.</param>
    /// <returns>A sequence containing only the unwrapped Some values.</returns>
    /// <example>
    /// <code>
    /// var results = items
    ///     .Select(item => item.ToOption())
    ///     .Choose(); // Filters out None and unwraps Some values
    /// </code>
    /// </example>
    public static IEnumerable<T> Choose<T>(this IEnumerable<Option<T>> source)
    {
        foreach (var option in source)
        {
            if (option.IsSome)
            {
                foreach (var value in option)
                {
                    yield return value;
                }
            }
        }
    }

    /// <summary>
    /// Applies a function that returns an Option to each element and filters out None values.
    /// This is equivalent to SelectMany followed by Choose, but more efficient.
    /// </summary>
    /// <typeparam name="TSource">The type of the source elements.</typeparam>
    /// <typeparam name="TResult">The type of the result values.</typeparam>
    /// <param name="source">The source sequence.</param>
    /// <param name="selector">Function that returns an Option for each element.</param>
    /// <returns>A sequence containing only the unwrapped Some values.</returns>
    /// <example>
    /// <code>
    /// var results = items.ChooseMap(item => FindById(item.Id)); // Only returns found items
    /// </code>
    /// </example>
    public static IEnumerable<TResult> ChooseMap<TSource, TResult>(
        this IEnumerable<TSource> source,
        Func<TSource, Option<TResult>> selector)
    {
        foreach (var item in source)
        {
            var option = selector(item);
            if (option.IsSome)
            {
                foreach (var value in option)
                {
                    yield return value;
                }
            }
        }
    }

    /// <summary>
    /// Applies an async function that returns an Option to each element and filters out None values.
    /// </summary>
    /// <typeparam name="TSource">The type of the source elements.</typeparam>
    /// <typeparam name="TResult">The type of the result values.</typeparam>
    /// <param name="source">The source sequence.</param>
    /// <param name="selector">Async function that returns an Option for each element.</param>
    /// <returns>A task containing a sequence of only the unwrapped Some values.</returns>
    public static async Task<IEnumerable<TResult>> ChooseMapAsync<TSource, TResult>(
        this IEnumerable<TSource> source,
        Func<TSource, Task<Option<TResult>>> selector)
    {
        var results = new List<TResult>();
        foreach (var item in source)
        {
            var option = await selector(item);
            if (option.IsSome)
            {
                foreach (var value in option)
                {
                    results.Add(value);
                }
            }
        }
        return results;
    }

    // --- Sequence ---

    /// <summary>
    /// Converts a sequence of Options into an Option of a sequence.
    /// Returns Some with all values if ALL are Some, or None if ANY is None.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="source">The sequence of options.</param>
    /// <returns>Some with all values, or None if any option is None.</returns>
    public static Option<IEnumerable<T>> Sequence<T>(this IEnumerable<Option<T>> source)
    {
        var values = new List<T>();

        foreach (var option in source)
        {
            if (option.IsSome)
            {
                foreach (var value in option)
                {
                    values.Add(value);
                }
            }
            else
            {
                return Option.None<IEnumerable<T>>();
            }
        }

        return Option.Some<IEnumerable<T>>(values);
    }

    // --- Traverse ---

    /// <summary>
    /// Applies an Option-returning function to each element, then sequences the results.
    /// Returns Some with all mapped values if all return Some, or None if any returns None.
    /// </summary>
    /// <typeparam name="T">The source element type.</typeparam>
    /// <typeparam name="TResult">The mapped value type.</typeparam>
    /// <param name="source">The source sequence.</param>
    /// <param name="func">A function that returns an Option for each element.</param>
    /// <returns>Some with all mapped values, or None.</returns>
    public static Option<IEnumerable<TResult>> Traverse<T, TResult>(
        this IEnumerable<T> source, Func<T, Option<TResult>> func)
    {
        var values = new List<TResult>();

        foreach (var item in source)
        {
            var option = func(item);
            if (option.IsSome)
            {
                foreach (var value in option)
                {
                    values.Add(value);
                }
            }
            else
            {
                return Option.None<IEnumerable<TResult>>();
            }
        }

        return Option.Some<IEnumerable<TResult>>(values);
    }

    // --- Join ---

    /// <summary>
    /// Combines two independent Options into a tuple.
    /// Returns Some only if both are Some, otherwise None.
    /// </summary>
    /// <typeparam name="T1">The first value type.</typeparam>
    /// <typeparam name="T2">The second value type.</typeparam>
    /// <param name="first">The first option.</param>
    /// <param name="second">The second option.</param>
    /// <returns>Some with a tuple of both values, or None.</returns>
    public static Option<(T1, T2)> Join<T1, T2>(this Option<T1> first, Option<T2> second) =>
        first.Bind(v1 => second.Map(v2 => (v1, v2)));

    /// <summary>
    /// Combines three independent Options into a tuple.
    /// Returns Some only if all are Some, otherwise None.
    /// </summary>
    public static Option<(T1, T2, T3)> Join<T1, T2, T3>(
        this Option<T1> first, Option<T2> second, Option<T3> third) =>
        first.Bind(v1 => second.Bind(v2 => third.Map(v3 => (v1, v2, v3))));

    /// <summary>
    /// Combines four independent Options into a tuple.
    /// Returns Some only if all are Some, otherwise None.
    /// </summary>
    public static Option<(T1, T2, T3, T4)> Join<T1, T2, T3, T4>(
        this Option<T1> first, Option<T2> second, Option<T3> third, Option<T4> fourth) =>
        first.Bind(v1 => second.Bind(v2 => third.Bind(v3 => fourth.Map(v4 => (v1, v2, v3, v4)))));

    /// <summary>
    /// Combines five independent Options into a tuple.
    /// Returns Some only if all are Some, otherwise None.
    /// </summary>
    public static Option<(T1, T2, T3, T4, T5)> Join<T1, T2, T3, T4, T5>(
        this Option<T1> first, Option<T2> second, Option<T3> third,
        Option<T4> fourth, Option<T5> fifth) =>
        first.Bind(v1 => second.Bind(v2 => third.Bind(v3 => fourth.Bind(v4 => fifth.Map(v5 => (v1, v2, v3, v4, v5))))));

    /// <summary>
    /// Combines six independent Options into a tuple.
    /// Returns Some only if all are Some, otherwise None.
    /// </summary>
    public static Option<(T1, T2, T3, T4, T5, T6)> Join<T1, T2, T3, T4, T5, T6>(
        this Option<T1> first, Option<T2> second, Option<T3> third,
        Option<T4> fourth, Option<T5> fifth, Option<T6> sixth) =>
        first.Bind(v1 => second.Bind(v2 => third.Bind(v3 => fourth.Bind(v4 => fifth.Bind(v5 => sixth.Map(v6 => (v1, v2, v3, v4, v5, v6)))))));

    /// <summary>
    /// Combines seven independent Options into a tuple.
    /// Returns Some only if all are Some, otherwise None.
    /// </summary>
    public static Option<(T1, T2, T3, T4, T5, T6, T7)> Join<T1, T2, T3, T4, T5, T6, T7>(
        this Option<T1> first, Option<T2> second, Option<T3> third,
        Option<T4> fourth, Option<T5> fifth, Option<T6> sixth,
        Option<T7> seventh) =>
        first.Bind(v1 => second.Bind(v2 => third.Bind(v3 => fourth.Bind(v4 => fifth.Bind(v5 => sixth.Bind(v6 => seventh.Map(v7 => (v1, v2, v3, v4, v5, v6, v7))))))));

    /// <summary>
    /// Combines eight independent Options into a tuple.
    /// Returns Some only if all are Some, otherwise None.
    /// </summary>
    public static Option<(T1, T2, T3, T4, T5, T6, T7, T8)> Join<T1, T2, T3, T4, T5, T6, T7, T8>(
        this Option<T1> first, Option<T2> second, Option<T3> third,
        Option<T4> fourth, Option<T5> fifth, Option<T6> sixth,
        Option<T7> seventh, Option<T8> eighth) =>
        first.Bind(v1 => second.Bind(v2 => third.Bind(v3 => fourth.Bind(v4 => fifth.Bind(v5 => sixth.Bind(v6 => seventh.Bind(v7 => eighth.Map(v8 => (v1, v2, v3, v4, v5, v6, v7, v8)))))))));

    // --- Async Sequential ---

    /// <summary>
    /// Awaits a sequence of tasks producing Options one by one.
    /// Returns Some with all values if all are Some, or None if any is None (fail-fast, sequential).
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="tasks">The tasks producing options.</param>
    /// <returns>Some with all values, or None.</returns>
    public static async Task<Option<IEnumerable<T>>> SequenceAsync<T>(
        this IEnumerable<Task<Option<T>>> tasks)
    {
        var values = new List<T>();

        foreach (var task in tasks)
        {
            var option = await task;
            if (option.IsSome)
            {
                foreach (var value in option)
                {
                    values.Add(value);
                }
            }
            else
            {
                return Option.None<IEnumerable<T>>();
            }
        }

        return Option.Some<IEnumerable<T>>(values);
    }

    /// <summary>
    /// Applies an async Option-returning function to each element sequentially, then sequences the results.
    /// Returns Some with all mapped values if all return Some, or None if any returns None (sequential).
    /// </summary>
    /// <typeparam name="T">The source element type.</typeparam>
    /// <typeparam name="TResult">The mapped value type.</typeparam>
    /// <param name="source">The source sequence.</param>
    /// <param name="func">An async function that returns an Option for each element.</param>
    /// <returns>Some with all mapped values, or None.</returns>
    public static async Task<Option<IEnumerable<TResult>>> TraverseAsync<T, TResult>(
        this IEnumerable<T> source, Func<T, Task<Option<TResult>>> func)
    {
        var values = new List<TResult>();

        foreach (var item in source)
        {
            var option = await func(item);
            if (option.IsSome)
            {
                foreach (var value in option)
                {
                    values.Add(value);
                }
            }
            else
            {
                return Option.None<IEnumerable<TResult>>();
            }
        }

        return Option.Some<IEnumerable<TResult>>(values);
    }

    // --- Async Parallel ---

    /// <summary>
    /// Awaits all tasks concurrently, then sequences the results.
    /// Returns Some with all values if all are Some, or None if any is None.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="tasks">The tasks producing options.</param>
    /// <returns>Some with all values, or None.</returns>
    public static async Task<Option<IEnumerable<T>>> SequenceParallel<T>(
        this IEnumerable<Task<Option<T>>> tasks)
    {
        var results = await Task.WhenAll(tasks);
        return results.Sequence();
    }

    /// <summary>
    /// Applies an async Option-returning function to all elements concurrently, then sequences the results.
    /// Returns Some with all mapped values if all return Some, or None if any returns None.
    /// </summary>
    /// <typeparam name="T">The source element type.</typeparam>
    /// <typeparam name="TResult">The mapped value type.</typeparam>
    /// <param name="source">The source sequence.</param>
    /// <param name="func">An async function that returns an Option for each element.</param>
    /// <returns>Some with all mapped values, or None.</returns>
    public static async Task<Option<IEnumerable<TResult>>> TraverseParallel<T, TResult>(
        this IEnumerable<T> source, Func<T, Task<Option<TResult>>> func)
    {
        var results = await Task.WhenAll(source.Select(func));
        return results.Sequence();
    }
}
