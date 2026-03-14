namespace DarkPeak.Functional;

/// <summary>
/// Provides static factory methods for creating <see cref="OneOf{T1, T2}"/> and higher-arity union instances.
/// </summary>
public static class OneOf
{
    /// <summary>
    /// Creates a <see cref="OneOf{T1, T2}"/> in the first case.
    /// </summary>
    /// <typeparam name="T1">The type of the first case.</typeparam>
    /// <typeparam name="T2">The type of the second case.</typeparam>
    /// <param name="value">The value to wrap.</param>
    /// <returns>A union holding <paramref name="value"/> as the first case.</returns>
    public static OneOf<T1, T2> First<T1, T2>(T1 value) => value;
    /// <summary>
    /// Creates a <see cref="OneOf{T1, T2}"/> in the second case.
    /// </summary>
    /// <typeparam name="T1">The type of the first case.</typeparam>
    /// <typeparam name="T2">The type of the second case.</typeparam>
    /// <param name="value">The value to wrap.</param>
    /// <returns>A union holding <paramref name="value"/> as the second case.</returns>
    public static OneOf<T1, T2> Second<T1, T2>(T2 value) => value;

    /// <summary>
    /// Creates a <see cref="OneOf{T1, T2, T3}"/> in the first case.
    /// </summary>
    /// <typeparam name="T1">The type of the first case.</typeparam>
    /// <typeparam name="T2">The type of the second case.</typeparam>
    /// <typeparam name="T3">The type of the third case.</typeparam>
    /// <param name="value">The value to wrap.</param>
    /// <returns>A union holding <paramref name="value"/> as the first case.</returns>
    public static OneOf<T1, T2, T3> First<T1, T2, T3>(T1 value) => value;
    /// <summary>
    /// Creates a <see cref="OneOf{T1, T2, T3}"/> in the second case.
    /// </summary>
    /// <typeparam name="T1">The type of the first case.</typeparam>
    /// <typeparam name="T2">The type of the second case.</typeparam>
    /// <typeparam name="T3">The type of the third case.</typeparam>
    /// <param name="value">The value to wrap.</param>
    /// <returns>A union holding <paramref name="value"/> as the second case.</returns>
    public static OneOf<T1, T2, T3> Second<T1, T2, T3>(T2 value) => value;
    /// <summary>
    /// Creates a <see cref="OneOf{T1, T2, T3}"/> in the third case.
    /// </summary>
    /// <typeparam name="T1">The type of the first case.</typeparam>
    /// <typeparam name="T2">The type of the second case.</typeparam>
    /// <typeparam name="T3">The type of the third case.</typeparam>
    /// <param name="value">The value to wrap.</param>
    /// <returns>A union holding <paramref name="value"/> as the third case.</returns>
    public static OneOf<T1, T2, T3> Third<T1, T2, T3>(T3 value) => value;

    /// <summary>
    /// Creates a <see cref="OneOf{T1, T2, T3, T4}"/> in the first case.
    /// </summary>
    /// <typeparam name="T1">The type of the first case.</typeparam>
    /// <typeparam name="T2">The type of the second case.</typeparam>
    /// <typeparam name="T3">The type of the third case.</typeparam>
    /// <typeparam name="T4">The type of the fourth case.</typeparam>
    /// <param name="value">The value to wrap.</param>
    /// <returns>A union holding <paramref name="value"/> as the first case.</returns>
    public static OneOf<T1, T2, T3, T4> First<T1, T2, T3, T4>(T1 value) => value;
    /// <summary>
    /// Creates a <see cref="OneOf{T1, T2, T3, T4}"/> in the second case.
    /// </summary>
    /// <typeparam name="T1">The type of the first case.</typeparam>
    /// <typeparam name="T2">The type of the second case.</typeparam>
    /// <typeparam name="T3">The type of the third case.</typeparam>
    /// <typeparam name="T4">The type of the fourth case.</typeparam>
    /// <param name="value">The value to wrap.</param>
    /// <returns>A union holding <paramref name="value"/> as the second case.</returns>
    public static OneOf<T1, T2, T3, T4> Second<T1, T2, T3, T4>(T2 value) => value;
    /// <summary>
    /// Creates a <see cref="OneOf{T1, T2, T3, T4}"/> in the third case.
    /// </summary>
    /// <typeparam name="T1">The type of the first case.</typeparam>
    /// <typeparam name="T2">The type of the second case.</typeparam>
    /// <typeparam name="T3">The type of the third case.</typeparam>
    /// <typeparam name="T4">The type of the fourth case.</typeparam>
    /// <param name="value">The value to wrap.</param>
    /// <returns>A union holding <paramref name="value"/> as the third case.</returns>
    public static OneOf<T1, T2, T3, T4> Third<T1, T2, T3, T4>(T3 value) => value;
    /// <summary>
    /// Creates a <see cref="OneOf{T1, T2, T3, T4}"/> in the fourth case.
    /// </summary>
    /// <typeparam name="T1">The type of the first case.</typeparam>
    /// <typeparam name="T2">The type of the second case.</typeparam>
    /// <typeparam name="T3">The type of the third case.</typeparam>
    /// <typeparam name="T4">The type of the fourth case.</typeparam>
    /// <param name="value">The value to wrap.</param>
    /// <returns>A union holding <paramref name="value"/> as the fourth case.</returns>
    public static OneOf<T1, T2, T3, T4> Fourth<T1, T2, T3, T4>(T4 value) => value;

    /// <summary>
    /// Creates a <see cref="OneOf{T1, T2, T3, T4, T5}"/> in the first case.
    /// </summary>
    /// <typeparam name="T1">The type of the first case.</typeparam>
    /// <typeparam name="T2">The type of the second case.</typeparam>
    /// <typeparam name="T3">The type of the third case.</typeparam>
    /// <typeparam name="T4">The type of the fourth case.</typeparam>
    /// <typeparam name="T5">The type of the fifth case.</typeparam>
    /// <param name="value">The value to wrap.</param>
    /// <returns>A union holding <paramref name="value"/> as the first case.</returns>
    public static OneOf<T1, T2, T3, T4, T5> First<T1, T2, T3, T4, T5>(T1 value) => value;
    /// <summary>
    /// Creates a <see cref="OneOf{T1, T2, T3, T4, T5}"/> in the second case.
    /// </summary>
    /// <typeparam name="T1">The type of the first case.</typeparam>
    /// <typeparam name="T2">The type of the second case.</typeparam>
    /// <typeparam name="T3">The type of the third case.</typeparam>
    /// <typeparam name="T4">The type of the fourth case.</typeparam>
    /// <typeparam name="T5">The type of the fifth case.</typeparam>
    /// <param name="value">The value to wrap.</param>
    /// <returns>A union holding <paramref name="value"/> as the second case.</returns>
    public static OneOf<T1, T2, T3, T4, T5> Second<T1, T2, T3, T4, T5>(T2 value) => value;
    /// <summary>
    /// Creates a <see cref="OneOf{T1, T2, T3, T4, T5}"/> in the third case.
    /// </summary>
    /// <typeparam name="T1">The type of the first case.</typeparam>
    /// <typeparam name="T2">The type of the second case.</typeparam>
    /// <typeparam name="T3">The type of the third case.</typeparam>
    /// <typeparam name="T4">The type of the fourth case.</typeparam>
    /// <typeparam name="T5">The type of the fifth case.</typeparam>
    /// <param name="value">The value to wrap.</param>
    /// <returns>A union holding <paramref name="value"/> as the third case.</returns>
    public static OneOf<T1, T2, T3, T4, T5> Third<T1, T2, T3, T4, T5>(T3 value) => value;
    /// <summary>
    /// Creates a <see cref="OneOf{T1, T2, T3, T4, T5}"/> in the fourth case.
    /// </summary>
    /// <typeparam name="T1">The type of the first case.</typeparam>
    /// <typeparam name="T2">The type of the second case.</typeparam>
    /// <typeparam name="T3">The type of the third case.</typeparam>
    /// <typeparam name="T4">The type of the fourth case.</typeparam>
    /// <typeparam name="T5">The type of the fifth case.</typeparam>
    /// <param name="value">The value to wrap.</param>
    /// <returns>A union holding <paramref name="value"/> as the fourth case.</returns>
    public static OneOf<T1, T2, T3, T4, T5> Fourth<T1, T2, T3, T4, T5>(T4 value) => value;
    /// <summary>
    /// Creates a <see cref="OneOf{T1, T2, T3, T4, T5}"/> in the fifth case.
    /// </summary>
    /// <typeparam name="T1">The type of the first case.</typeparam>
    /// <typeparam name="T2">The type of the second case.</typeparam>
    /// <typeparam name="T3">The type of the third case.</typeparam>
    /// <typeparam name="T4">The type of the fourth case.</typeparam>
    /// <typeparam name="T5">The type of the fifth case.</typeparam>
    /// <param name="value">The value to wrap.</param>
    /// <returns>A union holding <paramref name="value"/> as the fifth case.</returns>
    public static OneOf<T1, T2, T3, T4, T5> Fifth<T1, T2, T3, T4, T5>(T5 value) => value;

    /// <summary>
    /// Creates a <see cref="OneOf{T1, T2, T3, T4, T5, T6}"/> in the first case.
    /// </summary>
    /// <typeparam name="T1">The type of the first case.</typeparam>
    /// <typeparam name="T2">The type of the second case.</typeparam>
    /// <typeparam name="T3">The type of the third case.</typeparam>
    /// <typeparam name="T4">The type of the fourth case.</typeparam>
    /// <typeparam name="T5">The type of the fifth case.</typeparam>
    /// <typeparam name="T6">The type of the sixth case.</typeparam>
    /// <param name="value">The value to wrap.</param>
    /// <returns>A union holding <paramref name="value"/> as the first case.</returns>
    public static OneOf<T1, T2, T3, T4, T5, T6> First<T1, T2, T3, T4, T5, T6>(T1 value) => value;
    /// <summary>
    /// Creates a <see cref="OneOf{T1, T2, T3, T4, T5, T6}"/> in the second case.
    /// </summary>
    /// <typeparam name="T1">The type of the first case.</typeparam>
    /// <typeparam name="T2">The type of the second case.</typeparam>
    /// <typeparam name="T3">The type of the third case.</typeparam>
    /// <typeparam name="T4">The type of the fourth case.</typeparam>
    /// <typeparam name="T5">The type of the fifth case.</typeparam>
    /// <typeparam name="T6">The type of the sixth case.</typeparam>
    /// <param name="value">The value to wrap.</param>
    /// <returns>A union holding <paramref name="value"/> as the second case.</returns>
    public static OneOf<T1, T2, T3, T4, T5, T6> Second<T1, T2, T3, T4, T5, T6>(T2 value) => value;
    /// <summary>
    /// Creates a <see cref="OneOf{T1, T2, T3, T4, T5, T6}"/> in the third case.
    /// </summary>
    /// <typeparam name="T1">The type of the first case.</typeparam>
    /// <typeparam name="T2">The type of the second case.</typeparam>
    /// <typeparam name="T3">The type of the third case.</typeparam>
    /// <typeparam name="T4">The type of the fourth case.</typeparam>
    /// <typeparam name="T5">The type of the fifth case.</typeparam>
    /// <typeparam name="T6">The type of the sixth case.</typeparam>
    /// <param name="value">The value to wrap.</param>
    /// <returns>A union holding <paramref name="value"/> as the third case.</returns>
    public static OneOf<T1, T2, T3, T4, T5, T6> Third<T1, T2, T3, T4, T5, T6>(T3 value) => value;
    /// <summary>
    /// Creates a <see cref="OneOf{T1, T2, T3, T4, T5, T6}"/> in the fourth case.
    /// </summary>
    /// <typeparam name="T1">The type of the first case.</typeparam>
    /// <typeparam name="T2">The type of the second case.</typeparam>
    /// <typeparam name="T3">The type of the third case.</typeparam>
    /// <typeparam name="T4">The type of the fourth case.</typeparam>
    /// <typeparam name="T5">The type of the fifth case.</typeparam>
    /// <typeparam name="T6">The type of the sixth case.</typeparam>
    /// <param name="value">The value to wrap.</param>
    /// <returns>A union holding <paramref name="value"/> as the fourth case.</returns>
    public static OneOf<T1, T2, T3, T4, T5, T6> Fourth<T1, T2, T3, T4, T5, T6>(T4 value) => value;
    /// <summary>
    /// Creates a <see cref="OneOf{T1, T2, T3, T4, T5, T6}"/> in the fifth case.
    /// </summary>
    /// <typeparam name="T1">The type of the first case.</typeparam>
    /// <typeparam name="T2">The type of the second case.</typeparam>
    /// <typeparam name="T3">The type of the third case.</typeparam>
    /// <typeparam name="T4">The type of the fourth case.</typeparam>
    /// <typeparam name="T5">The type of the fifth case.</typeparam>
    /// <typeparam name="T6">The type of the sixth case.</typeparam>
    /// <param name="value">The value to wrap.</param>
    /// <returns>A union holding <paramref name="value"/> as the fifth case.</returns>
    public static OneOf<T1, T2, T3, T4, T5, T6> Fifth<T1, T2, T3, T4, T5, T6>(T5 value) => value;
    /// <summary>
    /// Creates a <see cref="OneOf{T1, T2, T3, T4, T5, T6}"/> in the sixth case.
    /// </summary>
    /// <typeparam name="T1">The type of the first case.</typeparam>
    /// <typeparam name="T2">The type of the second case.</typeparam>
    /// <typeparam name="T3">The type of the third case.</typeparam>
    /// <typeparam name="T4">The type of the fourth case.</typeparam>
    /// <typeparam name="T5">The type of the fifth case.</typeparam>
    /// <typeparam name="T6">The type of the sixth case.</typeparam>
    /// <param name="value">The value to wrap.</param>
    /// <returns>A union holding <paramref name="value"/> as the sixth case.</returns>
    public static OneOf<T1, T2, T3, T4, T5, T6> Sixth<T1, T2, T3, T4, T5, T6>(T6 value) => value;

    /// <summary>
    /// Creates a <see cref="OneOf{T1, T2, T3, T4, T5, T6, T7}"/> in the first case.
    /// </summary>
    /// <typeparam name="T1">The type of the first case.</typeparam>
    /// <typeparam name="T2">The type of the second case.</typeparam>
    /// <typeparam name="T3">The type of the third case.</typeparam>
    /// <typeparam name="T4">The type of the fourth case.</typeparam>
    /// <typeparam name="T5">The type of the fifth case.</typeparam>
    /// <typeparam name="T6">The type of the sixth case.</typeparam>
    /// <typeparam name="T7">The type of the seventh case.</typeparam>
    /// <param name="value">The value to wrap.</param>
    /// <returns>A union holding <paramref name="value"/> as the first case.</returns>
    public static OneOf<T1, T2, T3, T4, T5, T6, T7> First<T1, T2, T3, T4, T5, T6, T7>(T1 value) => value;
    /// <summary>
    /// Creates a <see cref="OneOf{T1, T2, T3, T4, T5, T6, T7}"/> in the second case.
    /// </summary>
    /// <typeparam name="T1">The type of the first case.</typeparam>
    /// <typeparam name="T2">The type of the second case.</typeparam>
    /// <typeparam name="T3">The type of the third case.</typeparam>
    /// <typeparam name="T4">The type of the fourth case.</typeparam>
    /// <typeparam name="T5">The type of the fifth case.</typeparam>
    /// <typeparam name="T6">The type of the sixth case.</typeparam>
    /// <typeparam name="T7">The type of the seventh case.</typeparam>
    /// <param name="value">The value to wrap.</param>
    /// <returns>A union holding <paramref name="value"/> as the second case.</returns>
    public static OneOf<T1, T2, T3, T4, T5, T6, T7> Second<T1, T2, T3, T4, T5, T6, T7>(T2 value) => value;
    /// <summary>
    /// Creates a <see cref="OneOf{T1, T2, T3, T4, T5, T6, T7}"/> in the third case.
    /// </summary>
    /// <typeparam name="T1">The type of the first case.</typeparam>
    /// <typeparam name="T2">The type of the second case.</typeparam>
    /// <typeparam name="T3">The type of the third case.</typeparam>
    /// <typeparam name="T4">The type of the fourth case.</typeparam>
    /// <typeparam name="T5">The type of the fifth case.</typeparam>
    /// <typeparam name="T6">The type of the sixth case.</typeparam>
    /// <typeparam name="T7">The type of the seventh case.</typeparam>
    /// <param name="value">The value to wrap.</param>
    /// <returns>A union holding <paramref name="value"/> as the third case.</returns>
    public static OneOf<T1, T2, T3, T4, T5, T6, T7> Third<T1, T2, T3, T4, T5, T6, T7>(T3 value) => value;
    /// <summary>
    /// Creates a <see cref="OneOf{T1, T2, T3, T4, T5, T6, T7}"/> in the fourth case.
    /// </summary>
    /// <typeparam name="T1">The type of the first case.</typeparam>
    /// <typeparam name="T2">The type of the second case.</typeparam>
    /// <typeparam name="T3">The type of the third case.</typeparam>
    /// <typeparam name="T4">The type of the fourth case.</typeparam>
    /// <typeparam name="T5">The type of the fifth case.</typeparam>
    /// <typeparam name="T6">The type of the sixth case.</typeparam>
    /// <typeparam name="T7">The type of the seventh case.</typeparam>
    /// <param name="value">The value to wrap.</param>
    /// <returns>A union holding <paramref name="value"/> as the fourth case.</returns>
    public static OneOf<T1, T2, T3, T4, T5, T6, T7> Fourth<T1, T2, T3, T4, T5, T6, T7>(T4 value) => value;
    /// <summary>
    /// Creates a <see cref="OneOf{T1, T2, T3, T4, T5, T6, T7}"/> in the fifth case.
    /// </summary>
    /// <typeparam name="T1">The type of the first case.</typeparam>
    /// <typeparam name="T2">The type of the second case.</typeparam>
    /// <typeparam name="T3">The type of the third case.</typeparam>
    /// <typeparam name="T4">The type of the fourth case.</typeparam>
    /// <typeparam name="T5">The type of the fifth case.</typeparam>
    /// <typeparam name="T6">The type of the sixth case.</typeparam>
    /// <typeparam name="T7">The type of the seventh case.</typeparam>
    /// <param name="value">The value to wrap.</param>
    /// <returns>A union holding <paramref name="value"/> as the fifth case.</returns>
    public static OneOf<T1, T2, T3, T4, T5, T6, T7> Fifth<T1, T2, T3, T4, T5, T6, T7>(T5 value) => value;
    /// <summary>
    /// Creates a <see cref="OneOf{T1, T2, T3, T4, T5, T6, T7}"/> in the sixth case.
    /// </summary>
    /// <typeparam name="T1">The type of the first case.</typeparam>
    /// <typeparam name="T2">The type of the second case.</typeparam>
    /// <typeparam name="T3">The type of the third case.</typeparam>
    /// <typeparam name="T4">The type of the fourth case.</typeparam>
    /// <typeparam name="T5">The type of the fifth case.</typeparam>
    /// <typeparam name="T6">The type of the sixth case.</typeparam>
    /// <typeparam name="T7">The type of the seventh case.</typeparam>
    /// <param name="value">The value to wrap.</param>
    /// <returns>A union holding <paramref name="value"/> as the sixth case.</returns>
    public static OneOf<T1, T2, T3, T4, T5, T6, T7> Sixth<T1, T2, T3, T4, T5, T6, T7>(T6 value) => value;
    /// <summary>
    /// Creates a <see cref="OneOf{T1, T2, T3, T4, T5, T6, T7}"/> in the seventh case.
    /// </summary>
    /// <typeparam name="T1">The type of the first case.</typeparam>
    /// <typeparam name="T2">The type of the second case.</typeparam>
    /// <typeparam name="T3">The type of the third case.</typeparam>
    /// <typeparam name="T4">The type of the fourth case.</typeparam>
    /// <typeparam name="T5">The type of the fifth case.</typeparam>
    /// <typeparam name="T6">The type of the sixth case.</typeparam>
    /// <typeparam name="T7">The type of the seventh case.</typeparam>
    /// <param name="value">The value to wrap.</param>
    /// <returns>A union holding <paramref name="value"/> as the seventh case.</returns>
    public static OneOf<T1, T2, T3, T4, T5, T6, T7> Seventh<T1, T2, T3, T4, T5, T6, T7>(T7 value) => value;

    /// <summary>
    /// Creates a <see cref="OneOf{T1, T2, T3, T4, T5, T6, T7, T8}"/> in the first case.
    /// </summary>
    /// <typeparam name="T1">The type of the first case.</typeparam>
    /// <typeparam name="T2">The type of the second case.</typeparam>
    /// <typeparam name="T3">The type of the third case.</typeparam>
    /// <typeparam name="T4">The type of the fourth case.</typeparam>
    /// <typeparam name="T5">The type of the fifth case.</typeparam>
    /// <typeparam name="T6">The type of the sixth case.</typeparam>
    /// <typeparam name="T7">The type of the seventh case.</typeparam>
    /// <typeparam name="T8">The type of the eighth case.</typeparam>
    /// <param name="value">The value to wrap.</param>
    /// <returns>A union holding <paramref name="value"/> as the first case.</returns>
    public static OneOf<T1, T2, T3, T4, T5, T6, T7, T8> First<T1, T2, T3, T4, T5, T6, T7, T8>(T1 value) => value;
    /// <summary>
    /// Creates a <see cref="OneOf{T1, T2, T3, T4, T5, T6, T7, T8}"/> in the second case.
    /// </summary>
    /// <typeparam name="T1">The type of the first case.</typeparam>
    /// <typeparam name="T2">The type of the second case.</typeparam>
    /// <typeparam name="T3">The type of the third case.</typeparam>
    /// <typeparam name="T4">The type of the fourth case.</typeparam>
    /// <typeparam name="T5">The type of the fifth case.</typeparam>
    /// <typeparam name="T6">The type of the sixth case.</typeparam>
    /// <typeparam name="T7">The type of the seventh case.</typeparam>
    /// <typeparam name="T8">The type of the eighth case.</typeparam>
    /// <param name="value">The value to wrap.</param>
    /// <returns>A union holding <paramref name="value"/> as the second case.</returns>
    public static OneOf<T1, T2, T3, T4, T5, T6, T7, T8> Second<T1, T2, T3, T4, T5, T6, T7, T8>(T2 value) => value;
    /// <summary>
    /// Creates a <see cref="OneOf{T1, T2, T3, T4, T5, T6, T7, T8}"/> in the third case.
    /// </summary>
    /// <typeparam name="T1">The type of the first case.</typeparam>
    /// <typeparam name="T2">The type of the second case.</typeparam>
    /// <typeparam name="T3">The type of the third case.</typeparam>
    /// <typeparam name="T4">The type of the fourth case.</typeparam>
    /// <typeparam name="T5">The type of the fifth case.</typeparam>
    /// <typeparam name="T6">The type of the sixth case.</typeparam>
    /// <typeparam name="T7">The type of the seventh case.</typeparam>
    /// <typeparam name="T8">The type of the eighth case.</typeparam>
    /// <param name="value">The value to wrap.</param>
    /// <returns>A union holding <paramref name="value"/> as the third case.</returns>
    public static OneOf<T1, T2, T3, T4, T5, T6, T7, T8> Third<T1, T2, T3, T4, T5, T6, T7, T8>(T3 value) => value;
    /// <summary>
    /// Creates a <see cref="OneOf{T1, T2, T3, T4, T5, T6, T7, T8}"/> in the fourth case.
    /// </summary>
    /// <typeparam name="T1">The type of the first case.</typeparam>
    /// <typeparam name="T2">The type of the second case.</typeparam>
    /// <typeparam name="T3">The type of the third case.</typeparam>
    /// <typeparam name="T4">The type of the fourth case.</typeparam>
    /// <typeparam name="T5">The type of the fifth case.</typeparam>
    /// <typeparam name="T6">The type of the sixth case.</typeparam>
    /// <typeparam name="T7">The type of the seventh case.</typeparam>
    /// <typeparam name="T8">The type of the eighth case.</typeparam>
    /// <param name="value">The value to wrap.</param>
    /// <returns>A union holding <paramref name="value"/> as the fourth case.</returns>
    public static OneOf<T1, T2, T3, T4, T5, T6, T7, T8> Fourth<T1, T2, T3, T4, T5, T6, T7, T8>(T4 value) => value;
    /// <summary>
    /// Creates a <see cref="OneOf{T1, T2, T3, T4, T5, T6, T7, T8}"/> in the fifth case.
    /// </summary>
    /// <typeparam name="T1">The type of the first case.</typeparam>
    /// <typeparam name="T2">The type of the second case.</typeparam>
    /// <typeparam name="T3">The type of the third case.</typeparam>
    /// <typeparam name="T4">The type of the fourth case.</typeparam>
    /// <typeparam name="T5">The type of the fifth case.</typeparam>
    /// <typeparam name="T6">The type of the sixth case.</typeparam>
    /// <typeparam name="T7">The type of the seventh case.</typeparam>
    /// <typeparam name="T8">The type of the eighth case.</typeparam>
    /// <param name="value">The value to wrap.</param>
    /// <returns>A union holding <paramref name="value"/> as the fifth case.</returns>
    public static OneOf<T1, T2, T3, T4, T5, T6, T7, T8> Fifth<T1, T2, T3, T4, T5, T6, T7, T8>(T5 value) => value;
    /// <summary>
    /// Creates a <see cref="OneOf{T1, T2, T3, T4, T5, T6, T7, T8}"/> in the sixth case.
    /// </summary>
    /// <typeparam name="T1">The type of the first case.</typeparam>
    /// <typeparam name="T2">The type of the second case.</typeparam>
    /// <typeparam name="T3">The type of the third case.</typeparam>
    /// <typeparam name="T4">The type of the fourth case.</typeparam>
    /// <typeparam name="T5">The type of the fifth case.</typeparam>
    /// <typeparam name="T6">The type of the sixth case.</typeparam>
    /// <typeparam name="T7">The type of the seventh case.</typeparam>
    /// <typeparam name="T8">The type of the eighth case.</typeparam>
    /// <param name="value">The value to wrap.</param>
    /// <returns>A union holding <paramref name="value"/> as the sixth case.</returns>
    public static OneOf<T1, T2, T3, T4, T5, T6, T7, T8> Sixth<T1, T2, T3, T4, T5, T6, T7, T8>(T6 value) => value;
    /// <summary>
    /// Creates a <see cref="OneOf{T1, T2, T3, T4, T5, T6, T7, T8}"/> in the seventh case.
    /// </summary>
    /// <typeparam name="T1">The type of the first case.</typeparam>
    /// <typeparam name="T2">The type of the second case.</typeparam>
    /// <typeparam name="T3">The type of the third case.</typeparam>
    /// <typeparam name="T4">The type of the fourth case.</typeparam>
    /// <typeparam name="T5">The type of the fifth case.</typeparam>
    /// <typeparam name="T6">The type of the sixth case.</typeparam>
    /// <typeparam name="T7">The type of the seventh case.</typeparam>
    /// <typeparam name="T8">The type of the eighth case.</typeparam>
    /// <param name="value">The value to wrap.</param>
    /// <returns>A union holding <paramref name="value"/> as the seventh case.</returns>
    public static OneOf<T1, T2, T3, T4, T5, T6, T7, T8> Seventh<T1, T2, T3, T4, T5, T6, T7, T8>(T7 value) => value;
    /// <summary>
    /// Creates a <see cref="OneOf{T1, T2, T3, T4, T5, T6, T7, T8}"/> in the eighth case.
    /// </summary>
    /// <typeparam name="T1">The type of the first case.</typeparam>
    /// <typeparam name="T2">The type of the second case.</typeparam>
    /// <typeparam name="T3">The type of the third case.</typeparam>
    /// <typeparam name="T4">The type of the fourth case.</typeparam>
    /// <typeparam name="T5">The type of the fifth case.</typeparam>
    /// <typeparam name="T6">The type of the sixth case.</typeparam>
    /// <typeparam name="T7">The type of the seventh case.</typeparam>
    /// <typeparam name="T8">The type of the eighth case.</typeparam>
    /// <param name="value">The value to wrap.</param>
    /// <returns>A union holding <paramref name="value"/> as the eighth case.</returns>
    public static OneOf<T1, T2, T3, T4, T5, T6, T7, T8> Eighth<T1, T2, T3, T4, T5, T6, T7, T8>(T8 value) => value;
}

/// <summary>
/// Represents a discriminated union of 2 possible types.
/// Unlike Result, no case is inherently success or failure.
/// </summary>
/// <typeparam name="T1">The type of the first case.</typeparam>
/// <typeparam name="T2">The type of the second case.</typeparam>
public sealed record OneOf<T1, T2>
{
    private readonly object value;
    private readonly byte index;

    private OneOf(byte index, object value)
    {
        this.index = index;
        this.value = value;
    }

    /// <summary>
    /// Gets a value indicating whether this instance holds a <typeparamref name="T1"/> value.
    /// </summary>
    public bool IsT1 => index == 1;
    /// <summary>
    /// Gets a value indicating whether this instance holds a <typeparamref name="T2"/> value.
    /// </summary>
    public bool IsT2 => index == 2;

    /// <summary>
    /// Gets the value as <typeparamref name="T1"/>.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the active case is not T1.</exception>
    public T1 AsT1 => index == 1
        ? (T1)value
        : throw new InvalidOperationException("Value is not T1.");
    /// <summary>
    /// Gets the value as <typeparamref name="T2"/>.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the active case is not T2.</exception>
    public T2 AsT2 => index == 2
        ? (T2)value
        : throw new InvalidOperationException("Value is not T2.");

    /// <summary>
    /// Matches the union to one of 2 functions based on the active case.
    /// </summary>
    /// <typeparam name="TResult">The result type.</typeparam>
    /// <param name="t1">Handler for the first case.</param>
    /// <param name="t2">Handler for the second case.</param>
    /// <returns>The result of the matching handler.</returns>
    public TResult Match<TResult>(Func<T1, TResult> t1, Func<T2, TResult> t2) =>
        index switch
        {
            1 => t1((T1)value),
            2 => t2((T2)value),
            _ => throw new InvalidOperationException("Invalid OneOf state.")
        };

    /// <summary>
    /// Asynchronously matches the union to one of 2 functions based on the active case.
    /// </summary>
    /// <typeparam name="TResult">The result type.</typeparam>
    /// <param name="t1">Async handler for the first case.</param>
    /// <param name="t2">Async handler for the second case.</param>
    /// <returns>A task containing the result of the matching handler.</returns>
    public Task<TResult> MatchAsync<TResult>(Func<T1, Task<TResult>> t1, Func<T2, Task<TResult>> t2) =>
        index switch
        {
            1 => t1((T1)value),
            2 => t2((T2)value),
            _ => throw new InvalidOperationException("Invalid OneOf state.")
        };

    /// <summary>
    /// Transforms the first case using the provided function, leaving all other cases unchanged.
    /// </summary>
    /// <typeparam name="T1Result">The transformed type for the first case.</typeparam>
    /// <param name="mapper">Function to transform the first value.</param>
    /// <returns>A union with the first case transformed, or the original value if another case is active.</returns>
    public OneOf<T1Result, T2> MapFirst<T1Result>(Func<T1, T1Result> mapper) =>
        Match<OneOf<T1Result, T2>>(t1 => mapper(t1), t2 => t2);

    /// <summary>
    /// Transforms the second case using the provided function, leaving all other cases unchanged.
    /// </summary>
    /// <typeparam name="T2Result">The transformed type for the second case.</typeparam>
    /// <param name="mapper">Function to transform the second value.</param>
    /// <returns>A union with the second case transformed, or the original value if another case is active.</returns>
    public OneOf<T1, T2Result> MapSecond<T2Result>(Func<T2, T2Result> mapper) =>
        Match<OneOf<T1, T2Result>>(t1 => t1, t2 => mapper(t2));

    /// <summary>
    /// Implicitly converts a <typeparamref name="T1"/> value into the first case of the union.
    /// </summary>
    /// <param name="value">The value to wrap.</param>
    /// <returns>A union holding <paramref name="value"/> as the first case.</returns>
    public static implicit operator OneOf<T1, T2>(T1 value) => new(1, value!);
    /// <summary>
    /// Implicitly converts a <typeparamref name="T2"/> value into the second case of the union.
    /// </summary>
    /// <param name="value">The value to wrap.</param>
    /// <returns>A union holding <paramref name="value"/> as the second case.</returns>
    public static implicit operator OneOf<T1, T2>(T2 value) => new(2, value!);

    /// <summary>
    /// Projects the second case using the provided selector (LINQ select).
    /// Earlier cases short-circuit through unchanged.
    /// </summary>
    /// <typeparam name="TResult">The projected type.</typeparam>
    /// <param name="selector">Function to transform the second value.</param>
    /// <returns>A union with the second case projected, or the original value if another case is active.</returns>
    public OneOf<T1, TResult> Select<TResult>(Func<T2, TResult> selector) =>
        MapSecond(selector);

    /// <summary>
    /// Projects and flattens the second case (LINQ bind / flatMap).
    /// Earlier cases short-circuit through unchanged.
    /// </summary>
    /// <typeparam name="TResult">The projected type.</typeparam>
    /// <param name="selector">Function that returns a union from the second value.</param>
    /// <returns>The union returned by <paramref name="selector"/>, or the original value if another case is active.</returns>
    public OneOf<T1, TResult> SelectMany<TResult>(Func<T2, OneOf<T1, TResult>> selector) =>
        Match<OneOf<T1, TResult>>(t1 => t1, t2 => selector(t2));

    /// <summary>
    /// Projects and flattens the second case with an intermediate projection (LINQ query syntax).
    /// Earlier cases short-circuit through unchanged.
    /// </summary>
    /// <typeparam name="TIntermediate">The intermediate projected type.</typeparam>
    /// <typeparam name="TResult">The final projected type.</typeparam>
    /// <param name="selector">Function that returns a union from the second value.</param>
    /// <param name="projector">Function that combines the original and intermediate values.</param>
    /// <returns>A union with the projected result, or the original value if another case is active.</returns>
    public OneOf<T1, TResult> SelectMany<TIntermediate, TResult>(
        Func<T2, OneOf<T1, TIntermediate>> selector,
        Func<T2, TIntermediate, TResult> projector) =>
        SelectMany(last => selector(last).Select(intermediate => projector(last, intermediate)));
}

/// <summary>
/// Represents a discriminated union of 3 possible types.
/// Unlike Result, no case is inherently success or failure.
/// </summary>
/// <typeparam name="T1">The type of the first case.</typeparam>
/// <typeparam name="T2">The type of the second case.</typeparam>
/// <typeparam name="T3">The type of the third case.</typeparam>
public sealed record OneOf<T1, T2, T3>
{
    private readonly object value;
    private readonly byte index;

    private OneOf(byte index, object value)
    {
        this.index = index;
        this.value = value;
    }

    /// <summary>
    /// Gets a value indicating whether this instance holds a <typeparamref name="T1"/> value.
    /// </summary>
    public bool IsT1 => index == 1;
    /// <summary>
    /// Gets a value indicating whether this instance holds a <typeparamref name="T2"/> value.
    /// </summary>
    public bool IsT2 => index == 2;
    /// <summary>
    /// Gets a value indicating whether this instance holds a <typeparamref name="T3"/> value.
    /// </summary>
    public bool IsT3 => index == 3;

    /// <summary>
    /// Gets the value as <typeparamref name="T1"/>.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the active case is not T1.</exception>
    public T1 AsT1 => index == 1
        ? (T1)value
        : throw new InvalidOperationException("Value is not T1.");
    /// <summary>
    /// Gets the value as <typeparamref name="T2"/>.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the active case is not T2.</exception>
    public T2 AsT2 => index == 2
        ? (T2)value
        : throw new InvalidOperationException("Value is not T2.");
    /// <summary>
    /// Gets the value as <typeparamref name="T3"/>.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the active case is not T3.</exception>
    public T3 AsT3 => index == 3
        ? (T3)value
        : throw new InvalidOperationException("Value is not T3.");

    /// <summary>
    /// Matches the union to one of 3 functions based on the active case.
    /// </summary>
    /// <typeparam name="TResult">The result type.</typeparam>
    /// <param name="t1">Handler for the first case.</param>
    /// <param name="t2">Handler for the second case.</param>
    /// <param name="t3">Handler for the third case.</param>
    /// <returns>The result of the matching handler.</returns>
    public TResult Match<TResult>(Func<T1, TResult> t1, Func<T2, TResult> t2, Func<T3, TResult> t3) =>
        index switch
        {
            1 => t1((T1)value),
            2 => t2((T2)value),
            3 => t3((T3)value),
            _ => throw new InvalidOperationException("Invalid OneOf state.")
        };

    /// <summary>
    /// Asynchronously matches the union to one of 3 functions based on the active case.
    /// </summary>
    /// <typeparam name="TResult">The result type.</typeparam>
    /// <param name="t1">Async handler for the first case.</param>
    /// <param name="t2">Async handler for the second case.</param>
    /// <param name="t3">Async handler for the third case.</param>
    /// <returns>A task containing the result of the matching handler.</returns>
    public Task<TResult> MatchAsync<TResult>(Func<T1, Task<TResult>> t1, Func<T2, Task<TResult>> t2, Func<T3, Task<TResult>> t3) =>
        index switch
        {
            1 => t1((T1)value),
            2 => t2((T2)value),
            3 => t3((T3)value),
            _ => throw new InvalidOperationException("Invalid OneOf state.")
        };

    /// <summary>
    /// Transforms the first case using the provided function, leaving all other cases unchanged.
    /// </summary>
    /// <typeparam name="T1Result">The transformed type for the first case.</typeparam>
    /// <param name="mapper">Function to transform the first value.</param>
    /// <returns>A union with the first case transformed, or the original value if another case is active.</returns>
    public OneOf<T1Result, T2, T3> MapFirst<T1Result>(Func<T1, T1Result> mapper) =>
        Match<OneOf<T1Result, T2, T3>>(t1 => mapper(t1), t2 => t2, t3 => t3);

    /// <summary>
    /// Transforms the second case using the provided function, leaving all other cases unchanged.
    /// </summary>
    /// <typeparam name="T2Result">The transformed type for the second case.</typeparam>
    /// <param name="mapper">Function to transform the second value.</param>
    /// <returns>A union with the second case transformed, or the original value if another case is active.</returns>
    public OneOf<T1, T2Result, T3> MapSecond<T2Result>(Func<T2, T2Result> mapper) =>
        Match<OneOf<T1, T2Result, T3>>(t1 => t1, t2 => mapper(t2), t3 => t3);

    /// <summary>
    /// Transforms the third case using the provided function, leaving all other cases unchanged.
    /// </summary>
    /// <typeparam name="T3Result">The transformed type for the third case.</typeparam>
    /// <param name="mapper">Function to transform the third value.</param>
    /// <returns>A union with the third case transformed, or the original value if another case is active.</returns>
    public OneOf<T1, T2, T3Result> MapThird<T3Result>(Func<T3, T3Result> mapper) =>
        Match<OneOf<T1, T2, T3Result>>(t1 => t1, t2 => t2, t3 => mapper(t3));

    /// <summary>
    /// Collapses the first case into the remaining cases, producing a union with one fewer type parameter.
    /// </summary>
    /// <param name="reducer">Function that maps the first value into the smaller union.</param>
    /// <returns>A union of 2 types with the first case eliminated.</returns>
    public OneOf<T2, T3> ReduceFirst(Func<T1, OneOf<T2, T3>> reducer) =>
        Match<OneOf<T2, T3>>(t1 => reducer(t1), t2 => t2, t3 => t3);

    /// <summary>
    /// Collapses the second case into the remaining cases, producing a union with one fewer type parameter.
    /// </summary>
    /// <param name="reducer">Function that maps the second value into the smaller union.</param>
    /// <returns>A union of 2 types with the second case eliminated.</returns>
    public OneOf<T1, T3> ReduceSecond(Func<T2, OneOf<T1, T3>> reducer) =>
        Match<OneOf<T1, T3>>(t1 => t1, t2 => reducer(t2), t3 => t3);

    /// <summary>
    /// Collapses the third case into the remaining cases, producing a union with one fewer type parameter.
    /// </summary>
    /// <param name="reducer">Function that maps the third value into the smaller union.</param>
    /// <returns>A union of 2 types with the third case eliminated.</returns>
    public OneOf<T1, T2> ReduceThird(Func<T3, OneOf<T1, T2>> reducer) =>
        Match<OneOf<T1, T2>>(t1 => t1, t2 => t2, t3 => reducer(t3));

    /// <summary>
    /// Implicitly converts a <typeparamref name="T1"/> value into the first case of the union.
    /// </summary>
    /// <param name="value">The value to wrap.</param>
    /// <returns>A union holding <paramref name="value"/> as the first case.</returns>
    public static implicit operator OneOf<T1, T2, T3>(T1 value) => new(1, value!);
    /// <summary>
    /// Implicitly converts a <typeparamref name="T2"/> value into the second case of the union.
    /// </summary>
    /// <param name="value">The value to wrap.</param>
    /// <returns>A union holding <paramref name="value"/> as the second case.</returns>
    public static implicit operator OneOf<T1, T2, T3>(T2 value) => new(2, value!);
    /// <summary>
    /// Implicitly converts a <typeparamref name="T3"/> value into the third case of the union.
    /// </summary>
    /// <param name="value">The value to wrap.</param>
    /// <returns>A union holding <paramref name="value"/> as the third case.</returns>
    public static implicit operator OneOf<T1, T2, T3>(T3 value) => new(3, value!);

    /// <summary>
    /// Projects the third case using the provided selector (LINQ select).
    /// Earlier cases short-circuit through unchanged.
    /// </summary>
    /// <typeparam name="TResult">The projected type.</typeparam>
    /// <param name="selector">Function to transform the third value.</param>
    /// <returns>A union with the third case projected, or the original value if another case is active.</returns>
    public OneOf<T1, T2, TResult> Select<TResult>(Func<T3, TResult> selector) =>
        MapThird(selector);

    /// <summary>
    /// Projects and flattens the third case (LINQ bind / flatMap).
    /// Earlier cases short-circuit through unchanged.
    /// </summary>
    /// <typeparam name="TResult">The projected type.</typeparam>
    /// <param name="selector">Function that returns a union from the third value.</param>
    /// <returns>The union returned by <paramref name="selector"/>, or the original value if another case is active.</returns>
    public OneOf<T1, T2, TResult> SelectMany<TResult>(Func<T3, OneOf<T1, T2, TResult>> selector) =>
        Match<OneOf<T1, T2, TResult>>(t1 => t1, t2 => t2, t3 => selector(t3));

    /// <summary>
    /// Projects and flattens the third case with an intermediate projection (LINQ query syntax).
    /// Earlier cases short-circuit through unchanged.
    /// </summary>
    /// <typeparam name="TIntermediate">The intermediate projected type.</typeparam>
    /// <typeparam name="TResult">The final projected type.</typeparam>
    /// <param name="selector">Function that returns a union from the third value.</param>
    /// <param name="projector">Function that combines the original and intermediate values.</param>
    /// <returns>A union with the projected result, or the original value if another case is active.</returns>
    public OneOf<T1, T2, TResult> SelectMany<TIntermediate, TResult>(
        Func<T3, OneOf<T1, T2, TIntermediate>> selector,
        Func<T3, TIntermediate, TResult> projector) =>
        SelectMany(last => selector(last).Select(intermediate => projector(last, intermediate)));
}

/// <summary>
/// Represents a discriminated union of 4 possible types.
/// Unlike Result, no case is inherently success or failure.
/// </summary>
/// <typeparam name="T1">The type of the first case.</typeparam>
/// <typeparam name="T2">The type of the second case.</typeparam>
/// <typeparam name="T3">The type of the third case.</typeparam>
/// <typeparam name="T4">The type of the fourth case.</typeparam>
public sealed record OneOf<T1, T2, T3, T4>
{
    private readonly object value;
    private readonly byte index;

    private OneOf(byte index, object value)
    {
        this.index = index;
        this.value = value;
    }

    /// <summary>
    /// Gets a value indicating whether this instance holds a <typeparamref name="T1"/> value.
    /// </summary>
    public bool IsT1 => index == 1;
    /// <summary>
    /// Gets a value indicating whether this instance holds a <typeparamref name="T2"/> value.
    /// </summary>
    public bool IsT2 => index == 2;
    /// <summary>
    /// Gets a value indicating whether this instance holds a <typeparamref name="T3"/> value.
    /// </summary>
    public bool IsT3 => index == 3;
    /// <summary>
    /// Gets a value indicating whether this instance holds a <typeparamref name="T4"/> value.
    /// </summary>
    public bool IsT4 => index == 4;

    /// <summary>
    /// Gets the value as <typeparamref name="T1"/>.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the active case is not T1.</exception>
    public T1 AsT1 => index == 1
        ? (T1)value
        : throw new InvalidOperationException("Value is not T1.");
    /// <summary>
    /// Gets the value as <typeparamref name="T2"/>.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the active case is not T2.</exception>
    public T2 AsT2 => index == 2
        ? (T2)value
        : throw new InvalidOperationException("Value is not T2.");
    /// <summary>
    /// Gets the value as <typeparamref name="T3"/>.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the active case is not T3.</exception>
    public T3 AsT3 => index == 3
        ? (T3)value
        : throw new InvalidOperationException("Value is not T3.");
    /// <summary>
    /// Gets the value as <typeparamref name="T4"/>.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the active case is not T4.</exception>
    public T4 AsT4 => index == 4
        ? (T4)value
        : throw new InvalidOperationException("Value is not T4.");

    /// <summary>
    /// Matches the union to one of 4 functions based on the active case.
    /// </summary>
    /// <typeparam name="TResult">The result type.</typeparam>
    /// <param name="t1">Handler for the first case.</param>
    /// <param name="t2">Handler for the second case.</param>
    /// <param name="t3">Handler for the third case.</param>
    /// <param name="t4">Handler for the fourth case.</param>
    /// <returns>The result of the matching handler.</returns>
    public TResult Match<TResult>(Func<T1, TResult> t1, Func<T2, TResult> t2, Func<T3, TResult> t3, Func<T4, TResult> t4) =>
        index switch
        {
            1 => t1((T1)value),
            2 => t2((T2)value),
            3 => t3((T3)value),
            4 => t4((T4)value),
            _ => throw new InvalidOperationException("Invalid OneOf state.")
        };

    /// <summary>
    /// Asynchronously matches the union to one of 4 functions based on the active case.
    /// </summary>
    /// <typeparam name="TResult">The result type.</typeparam>
    /// <param name="t1">Async handler for the first case.</param>
    /// <param name="t2">Async handler for the second case.</param>
    /// <param name="t3">Async handler for the third case.</param>
    /// <param name="t4">Async handler for the fourth case.</param>
    /// <returns>A task containing the result of the matching handler.</returns>
    public Task<TResult> MatchAsync<TResult>(Func<T1, Task<TResult>> t1, Func<T2, Task<TResult>> t2, Func<T3, Task<TResult>> t3, Func<T4, Task<TResult>> t4) =>
        index switch
        {
            1 => t1((T1)value),
            2 => t2((T2)value),
            3 => t3((T3)value),
            4 => t4((T4)value),
            _ => throw new InvalidOperationException("Invalid OneOf state.")
        };

    /// <summary>
    /// Transforms the first case using the provided function, leaving all other cases unchanged.
    /// </summary>
    /// <typeparam name="T1Result">The transformed type for the first case.</typeparam>
    /// <param name="mapper">Function to transform the first value.</param>
    /// <returns>A union with the first case transformed, or the original value if another case is active.</returns>
    public OneOf<T1Result, T2, T3, T4> MapFirst<T1Result>(Func<T1, T1Result> mapper) =>
        Match<OneOf<T1Result, T2, T3, T4>>(t1 => mapper(t1), t2 => t2, t3 => t3, t4 => t4);

    /// <summary>
    /// Transforms the second case using the provided function, leaving all other cases unchanged.
    /// </summary>
    /// <typeparam name="T2Result">The transformed type for the second case.</typeparam>
    /// <param name="mapper">Function to transform the second value.</param>
    /// <returns>A union with the second case transformed, or the original value if another case is active.</returns>
    public OneOf<T1, T2Result, T3, T4> MapSecond<T2Result>(Func<T2, T2Result> mapper) =>
        Match<OneOf<T1, T2Result, T3, T4>>(t1 => t1, t2 => mapper(t2), t3 => t3, t4 => t4);

    /// <summary>
    /// Transforms the third case using the provided function, leaving all other cases unchanged.
    /// </summary>
    /// <typeparam name="T3Result">The transformed type for the third case.</typeparam>
    /// <param name="mapper">Function to transform the third value.</param>
    /// <returns>A union with the third case transformed, or the original value if another case is active.</returns>
    public OneOf<T1, T2, T3Result, T4> MapThird<T3Result>(Func<T3, T3Result> mapper) =>
        Match<OneOf<T1, T2, T3Result, T4>>(t1 => t1, t2 => t2, t3 => mapper(t3), t4 => t4);

    /// <summary>
    /// Transforms the fourth case using the provided function, leaving all other cases unchanged.
    /// </summary>
    /// <typeparam name="T4Result">The transformed type for the fourth case.</typeparam>
    /// <param name="mapper">Function to transform the fourth value.</param>
    /// <returns>A union with the fourth case transformed, or the original value if another case is active.</returns>
    public OneOf<T1, T2, T3, T4Result> MapFourth<T4Result>(Func<T4, T4Result> mapper) =>
        Match<OneOf<T1, T2, T3, T4Result>>(t1 => t1, t2 => t2, t3 => t3, t4 => mapper(t4));

    /// <summary>
    /// Collapses the first case into the remaining cases, producing a union with one fewer type parameter.
    /// </summary>
    /// <param name="reducer">Function that maps the first value into the smaller union.</param>
    /// <returns>A union of 3 types with the first case eliminated.</returns>
    public OneOf<T2, T3, T4> ReduceFirst(Func<T1, OneOf<T2, T3, T4>> reducer) =>
        Match<OneOf<T2, T3, T4>>(t1 => reducer(t1), t2 => t2, t3 => t3, t4 => t4);

    /// <summary>
    /// Collapses the second case into the remaining cases, producing a union with one fewer type parameter.
    /// </summary>
    /// <param name="reducer">Function that maps the second value into the smaller union.</param>
    /// <returns>A union of 3 types with the second case eliminated.</returns>
    public OneOf<T1, T3, T4> ReduceSecond(Func<T2, OneOf<T1, T3, T4>> reducer) =>
        Match<OneOf<T1, T3, T4>>(t1 => t1, t2 => reducer(t2), t3 => t3, t4 => t4);

    /// <summary>
    /// Collapses the third case into the remaining cases, producing a union with one fewer type parameter.
    /// </summary>
    /// <param name="reducer">Function that maps the third value into the smaller union.</param>
    /// <returns>A union of 3 types with the third case eliminated.</returns>
    public OneOf<T1, T2, T4> ReduceThird(Func<T3, OneOf<T1, T2, T4>> reducer) =>
        Match<OneOf<T1, T2, T4>>(t1 => t1, t2 => t2, t3 => reducer(t3), t4 => t4);

    /// <summary>
    /// Collapses the fourth case into the remaining cases, producing a union with one fewer type parameter.
    /// </summary>
    /// <param name="reducer">Function that maps the fourth value into the smaller union.</param>
    /// <returns>A union of 3 types with the fourth case eliminated.</returns>
    public OneOf<T1, T2, T3> ReduceFourth(Func<T4, OneOf<T1, T2, T3>> reducer) =>
        Match<OneOf<T1, T2, T3>>(t1 => t1, t2 => t2, t3 => t3, t4 => reducer(t4));

    /// <summary>
    /// Implicitly converts a <typeparamref name="T1"/> value into the first case of the union.
    /// </summary>
    /// <param name="value">The value to wrap.</param>
    /// <returns>A union holding <paramref name="value"/> as the first case.</returns>
    public static implicit operator OneOf<T1, T2, T3, T4>(T1 value) => new(1, value!);
    /// <summary>
    /// Implicitly converts a <typeparamref name="T2"/> value into the second case of the union.
    /// </summary>
    /// <param name="value">The value to wrap.</param>
    /// <returns>A union holding <paramref name="value"/> as the second case.</returns>
    public static implicit operator OneOf<T1, T2, T3, T4>(T2 value) => new(2, value!);
    /// <summary>
    /// Implicitly converts a <typeparamref name="T3"/> value into the third case of the union.
    /// </summary>
    /// <param name="value">The value to wrap.</param>
    /// <returns>A union holding <paramref name="value"/> as the third case.</returns>
    public static implicit operator OneOf<T1, T2, T3, T4>(T3 value) => new(3, value!);
    /// <summary>
    /// Implicitly converts a <typeparamref name="T4"/> value into the fourth case of the union.
    /// </summary>
    /// <param name="value">The value to wrap.</param>
    /// <returns>A union holding <paramref name="value"/> as the fourth case.</returns>
    public static implicit operator OneOf<T1, T2, T3, T4>(T4 value) => new(4, value!);

    /// <summary>
    /// Projects the fourth case using the provided selector (LINQ select).
    /// Earlier cases short-circuit through unchanged.
    /// </summary>
    /// <typeparam name="TResult">The projected type.</typeparam>
    /// <param name="selector">Function to transform the fourth value.</param>
    /// <returns>A union with the fourth case projected, or the original value if another case is active.</returns>
    public OneOf<T1, T2, T3, TResult> Select<TResult>(Func<T4, TResult> selector) =>
        MapFourth(selector);

    /// <summary>
    /// Projects and flattens the fourth case (LINQ bind / flatMap).
    /// Earlier cases short-circuit through unchanged.
    /// </summary>
    /// <typeparam name="TResult">The projected type.</typeparam>
    /// <param name="selector">Function that returns a union from the fourth value.</param>
    /// <returns>The union returned by <paramref name="selector"/>, or the original value if another case is active.</returns>
    public OneOf<T1, T2, T3, TResult> SelectMany<TResult>(Func<T4, OneOf<T1, T2, T3, TResult>> selector) =>
        Match<OneOf<T1, T2, T3, TResult>>(t1 => t1, t2 => t2, t3 => t3, t4 => selector(t4));

    /// <summary>
    /// Projects and flattens the fourth case with an intermediate projection (LINQ query syntax).
    /// Earlier cases short-circuit through unchanged.
    /// </summary>
    /// <typeparam name="TIntermediate">The intermediate projected type.</typeparam>
    /// <typeparam name="TResult">The final projected type.</typeparam>
    /// <param name="selector">Function that returns a union from the fourth value.</param>
    /// <param name="projector">Function that combines the original and intermediate values.</param>
    /// <returns>A union with the projected result, or the original value if another case is active.</returns>
    public OneOf<T1, T2, T3, TResult> SelectMany<TIntermediate, TResult>(
        Func<T4, OneOf<T1, T2, T3, TIntermediate>> selector,
        Func<T4, TIntermediate, TResult> projector) =>
        SelectMany(last => selector(last).Select(intermediate => projector(last, intermediate)));
}

/// <summary>
/// Represents a discriminated union of 5 possible types.
/// Unlike Result, no case is inherently success or failure.
/// </summary>
/// <typeparam name="T1">The type of the first case.</typeparam>
/// <typeparam name="T2">The type of the second case.</typeparam>
/// <typeparam name="T3">The type of the third case.</typeparam>
/// <typeparam name="T4">The type of the fourth case.</typeparam>
/// <typeparam name="T5">The type of the fifth case.</typeparam>
public sealed record OneOf<T1, T2, T3, T4, T5>
{
    private readonly object value;
    private readonly byte index;

    private OneOf(byte index, object value)
    {
        this.index = index;
        this.value = value;
    }

    /// <summary>
    /// Gets a value indicating whether this instance holds a <typeparamref name="T1"/> value.
    /// </summary>
    public bool IsT1 => index == 1;
    /// <summary>
    /// Gets a value indicating whether this instance holds a <typeparamref name="T2"/> value.
    /// </summary>
    public bool IsT2 => index == 2;
    /// <summary>
    /// Gets a value indicating whether this instance holds a <typeparamref name="T3"/> value.
    /// </summary>
    public bool IsT3 => index == 3;
    /// <summary>
    /// Gets a value indicating whether this instance holds a <typeparamref name="T4"/> value.
    /// </summary>
    public bool IsT4 => index == 4;
    /// <summary>
    /// Gets a value indicating whether this instance holds a <typeparamref name="T5"/> value.
    /// </summary>
    public bool IsT5 => index == 5;

    /// <summary>
    /// Gets the value as <typeparamref name="T1"/>.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the active case is not T1.</exception>
    public T1 AsT1 => index == 1
        ? (T1)value
        : throw new InvalidOperationException("Value is not T1.");
    /// <summary>
    /// Gets the value as <typeparamref name="T2"/>.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the active case is not T2.</exception>
    public T2 AsT2 => index == 2
        ? (T2)value
        : throw new InvalidOperationException("Value is not T2.");
    /// <summary>
    /// Gets the value as <typeparamref name="T3"/>.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the active case is not T3.</exception>
    public T3 AsT3 => index == 3
        ? (T3)value
        : throw new InvalidOperationException("Value is not T3.");
    /// <summary>
    /// Gets the value as <typeparamref name="T4"/>.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the active case is not T4.</exception>
    public T4 AsT4 => index == 4
        ? (T4)value
        : throw new InvalidOperationException("Value is not T4.");
    /// <summary>
    /// Gets the value as <typeparamref name="T5"/>.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the active case is not T5.</exception>
    public T5 AsT5 => index == 5
        ? (T5)value
        : throw new InvalidOperationException("Value is not T5.");

    /// <summary>
    /// Matches the union to one of 5 functions based on the active case.
    /// </summary>
    /// <typeparam name="TResult">The result type.</typeparam>
    /// <param name="t1">Handler for the first case.</param>
    /// <param name="t2">Handler for the second case.</param>
    /// <param name="t3">Handler for the third case.</param>
    /// <param name="t4">Handler for the fourth case.</param>
    /// <param name="t5">Handler for the fifth case.</param>
    /// <returns>The result of the matching handler.</returns>
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

    /// <summary>
    /// Asynchronously matches the union to one of 5 functions based on the active case.
    /// </summary>
    /// <typeparam name="TResult">The result type.</typeparam>
    /// <param name="t1">Async handler for the first case.</param>
    /// <param name="t2">Async handler for the second case.</param>
    /// <param name="t3">Async handler for the third case.</param>
    /// <param name="t4">Async handler for the fourth case.</param>
    /// <param name="t5">Async handler for the fifth case.</param>
    /// <returns>A task containing the result of the matching handler.</returns>
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

    /// <summary>
    /// Transforms the first case using the provided function, leaving all other cases unchanged.
    /// </summary>
    /// <typeparam name="T1Result">The transformed type for the first case.</typeparam>
    /// <param name="mapper">Function to transform the first value.</param>
    /// <returns>A union with the first case transformed, or the original value if another case is active.</returns>
    public OneOf<T1Result, T2, T3, T4, T5> MapFirst<T1Result>(Func<T1, T1Result> mapper) =>
        Match<OneOf<T1Result, T2, T3, T4, T5>>(t1 => mapper(t1), t2 => t2, t3 => t3, t4 => t4, t5 => t5);

    /// <summary>
    /// Transforms the second case using the provided function, leaving all other cases unchanged.
    /// </summary>
    /// <typeparam name="T2Result">The transformed type for the second case.</typeparam>
    /// <param name="mapper">Function to transform the second value.</param>
    /// <returns>A union with the second case transformed, or the original value if another case is active.</returns>
    public OneOf<T1, T2Result, T3, T4, T5> MapSecond<T2Result>(Func<T2, T2Result> mapper) =>
        Match<OneOf<T1, T2Result, T3, T4, T5>>(t1 => t1, t2 => mapper(t2), t3 => t3, t4 => t4, t5 => t5);

    /// <summary>
    /// Transforms the third case using the provided function, leaving all other cases unchanged.
    /// </summary>
    /// <typeparam name="T3Result">The transformed type for the third case.</typeparam>
    /// <param name="mapper">Function to transform the third value.</param>
    /// <returns>A union with the third case transformed, or the original value if another case is active.</returns>
    public OneOf<T1, T2, T3Result, T4, T5> MapThird<T3Result>(Func<T3, T3Result> mapper) =>
        Match<OneOf<T1, T2, T3Result, T4, T5>>(t1 => t1, t2 => t2, t3 => mapper(t3), t4 => t4, t5 => t5);

    /// <summary>
    /// Transforms the fourth case using the provided function, leaving all other cases unchanged.
    /// </summary>
    /// <typeparam name="T4Result">The transformed type for the fourth case.</typeparam>
    /// <param name="mapper">Function to transform the fourth value.</param>
    /// <returns>A union with the fourth case transformed, or the original value if another case is active.</returns>
    public OneOf<T1, T2, T3, T4Result, T5> MapFourth<T4Result>(Func<T4, T4Result> mapper) =>
        Match<OneOf<T1, T2, T3, T4Result, T5>>(t1 => t1, t2 => t2, t3 => t3, t4 => mapper(t4), t5 => t5);

    /// <summary>
    /// Transforms the fifth case using the provided function, leaving all other cases unchanged.
    /// </summary>
    /// <typeparam name="T5Result">The transformed type for the fifth case.</typeparam>
    /// <param name="mapper">Function to transform the fifth value.</param>
    /// <returns>A union with the fifth case transformed, or the original value if another case is active.</returns>
    public OneOf<T1, T2, T3, T4, T5Result> MapFifth<T5Result>(Func<T5, T5Result> mapper) =>
        Match<OneOf<T1, T2, T3, T4, T5Result>>(t1 => t1, t2 => t2, t3 => t3, t4 => t4, t5 => mapper(t5));

    /// <summary>
    /// Collapses the first case into the remaining cases, producing a union with one fewer type parameter.
    /// </summary>
    /// <param name="reducer">Function that maps the first value into the smaller union.</param>
    /// <returns>A union of 4 types with the first case eliminated.</returns>
    public OneOf<T2, T3, T4, T5> ReduceFirst(Func<T1, OneOf<T2, T3, T4, T5>> reducer) =>
        Match<OneOf<T2, T3, T4, T5>>(t1 => reducer(t1), t2 => t2, t3 => t3, t4 => t4, t5 => t5);

    /// <summary>
    /// Collapses the second case into the remaining cases, producing a union with one fewer type parameter.
    /// </summary>
    /// <param name="reducer">Function that maps the second value into the smaller union.</param>
    /// <returns>A union of 4 types with the second case eliminated.</returns>
    public OneOf<T1, T3, T4, T5> ReduceSecond(Func<T2, OneOf<T1, T3, T4, T5>> reducer) =>
        Match<OneOf<T1, T3, T4, T5>>(t1 => t1, t2 => reducer(t2), t3 => t3, t4 => t4, t5 => t5);

    /// <summary>
    /// Collapses the third case into the remaining cases, producing a union with one fewer type parameter.
    /// </summary>
    /// <param name="reducer">Function that maps the third value into the smaller union.</param>
    /// <returns>A union of 4 types with the third case eliminated.</returns>
    public OneOf<T1, T2, T4, T5> ReduceThird(Func<T3, OneOf<T1, T2, T4, T5>> reducer) =>
        Match<OneOf<T1, T2, T4, T5>>(t1 => t1, t2 => t2, t3 => reducer(t3), t4 => t4, t5 => t5);

    /// <summary>
    /// Collapses the fourth case into the remaining cases, producing a union with one fewer type parameter.
    /// </summary>
    /// <param name="reducer">Function that maps the fourth value into the smaller union.</param>
    /// <returns>A union of 4 types with the fourth case eliminated.</returns>
    public OneOf<T1, T2, T3, T5> ReduceFourth(Func<T4, OneOf<T1, T2, T3, T5>> reducer) =>
        Match<OneOf<T1, T2, T3, T5>>(t1 => t1, t2 => t2, t3 => t3, t4 => reducer(t4), t5 => t5);

    /// <summary>
    /// Collapses the fifth case into the remaining cases, producing a union with one fewer type parameter.
    /// </summary>
    /// <param name="reducer">Function that maps the fifth value into the smaller union.</param>
    /// <returns>A union of 4 types with the fifth case eliminated.</returns>
    public OneOf<T1, T2, T3, T4> ReduceFifth(Func<T5, OneOf<T1, T2, T3, T4>> reducer) =>
        Match<OneOf<T1, T2, T3, T4>>(t1 => t1, t2 => t2, t3 => t3, t4 => t4, t5 => reducer(t5));

    /// <summary>
    /// Implicitly converts a <typeparamref name="T1"/> value into the first case of the union.
    /// </summary>
    /// <param name="value">The value to wrap.</param>
    /// <returns>A union holding <paramref name="value"/> as the first case.</returns>
    public static implicit operator OneOf<T1, T2, T3, T4, T5>(T1 value) => new(1, value!);
    /// <summary>
    /// Implicitly converts a <typeparamref name="T2"/> value into the second case of the union.
    /// </summary>
    /// <param name="value">The value to wrap.</param>
    /// <returns>A union holding <paramref name="value"/> as the second case.</returns>
    public static implicit operator OneOf<T1, T2, T3, T4, T5>(T2 value) => new(2, value!);
    /// <summary>
    /// Implicitly converts a <typeparamref name="T3"/> value into the third case of the union.
    /// </summary>
    /// <param name="value">The value to wrap.</param>
    /// <returns>A union holding <paramref name="value"/> as the third case.</returns>
    public static implicit operator OneOf<T1, T2, T3, T4, T5>(T3 value) => new(3, value!);
    /// <summary>
    /// Implicitly converts a <typeparamref name="T4"/> value into the fourth case of the union.
    /// </summary>
    /// <param name="value">The value to wrap.</param>
    /// <returns>A union holding <paramref name="value"/> as the fourth case.</returns>
    public static implicit operator OneOf<T1, T2, T3, T4, T5>(T4 value) => new(4, value!);
    /// <summary>
    /// Implicitly converts a <typeparamref name="T5"/> value into the fifth case of the union.
    /// </summary>
    /// <param name="value">The value to wrap.</param>
    /// <returns>A union holding <paramref name="value"/> as the fifth case.</returns>
    public static implicit operator OneOf<T1, T2, T3, T4, T5>(T5 value) => new(5, value!);

    /// <summary>
    /// Projects the fifth case using the provided selector (LINQ select).
    /// Earlier cases short-circuit through unchanged.
    /// </summary>
    /// <typeparam name="TResult">The projected type.</typeparam>
    /// <param name="selector">Function to transform the fifth value.</param>
    /// <returns>A union with the fifth case projected, or the original value if another case is active.</returns>
    public OneOf<T1, T2, T3, T4, TResult> Select<TResult>(Func<T5, TResult> selector) =>
        MapFifth(selector);

    /// <summary>
    /// Projects and flattens the fifth case (LINQ bind / flatMap).
    /// Earlier cases short-circuit through unchanged.
    /// </summary>
    /// <typeparam name="TResult">The projected type.</typeparam>
    /// <param name="selector">Function that returns a union from the fifth value.</param>
    /// <returns>The union returned by <paramref name="selector"/>, or the original value if another case is active.</returns>
    public OneOf<T1, T2, T3, T4, TResult> SelectMany<TResult>(Func<T5, OneOf<T1, T2, T3, T4, TResult>> selector) =>
        Match<OneOf<T1, T2, T3, T4, TResult>>(t1 => t1, t2 => t2, t3 => t3, t4 => t4, t5 => selector(t5));

    /// <summary>
    /// Projects and flattens the fifth case with an intermediate projection (LINQ query syntax).
    /// Earlier cases short-circuit through unchanged.
    /// </summary>
    /// <typeparam name="TIntermediate">The intermediate projected type.</typeparam>
    /// <typeparam name="TResult">The final projected type.</typeparam>
    /// <param name="selector">Function that returns a union from the fifth value.</param>
    /// <param name="projector">Function that combines the original and intermediate values.</param>
    /// <returns>A union with the projected result, or the original value if another case is active.</returns>
    public OneOf<T1, T2, T3, T4, TResult> SelectMany<TIntermediate, TResult>(
        Func<T5, OneOf<T1, T2, T3, T4, TIntermediate>> selector,
        Func<T5, TIntermediate, TResult> projector) =>
        SelectMany(last => selector(last).Select(intermediate => projector(last, intermediate)));
}

/// <summary>
/// Represents a discriminated union of 6 possible types.
/// Unlike Result, no case is inherently success or failure.
/// </summary>
/// <typeparam name="T1">The type of the first case.</typeparam>
/// <typeparam name="T2">The type of the second case.</typeparam>
/// <typeparam name="T3">The type of the third case.</typeparam>
/// <typeparam name="T4">The type of the fourth case.</typeparam>
/// <typeparam name="T5">The type of the fifth case.</typeparam>
/// <typeparam name="T6">The type of the sixth case.</typeparam>
public sealed record OneOf<T1, T2, T3, T4, T5, T6>
{
    private readonly object value;
    private readonly byte index;

    private OneOf(byte index, object value)
    {
        this.index = index;
        this.value = value;
    }

    /// <summary>
    /// Gets a value indicating whether this instance holds a <typeparamref name="T1"/> value.
    /// </summary>
    public bool IsT1 => index == 1;
    /// <summary>
    /// Gets a value indicating whether this instance holds a <typeparamref name="T2"/> value.
    /// </summary>
    public bool IsT2 => index == 2;
    /// <summary>
    /// Gets a value indicating whether this instance holds a <typeparamref name="T3"/> value.
    /// </summary>
    public bool IsT3 => index == 3;
    /// <summary>
    /// Gets a value indicating whether this instance holds a <typeparamref name="T4"/> value.
    /// </summary>
    public bool IsT4 => index == 4;
    /// <summary>
    /// Gets a value indicating whether this instance holds a <typeparamref name="T5"/> value.
    /// </summary>
    public bool IsT5 => index == 5;
    /// <summary>
    /// Gets a value indicating whether this instance holds a <typeparamref name="T6"/> value.
    /// </summary>
    public bool IsT6 => index == 6;

    /// <summary>
    /// Gets the value as <typeparamref name="T1"/>.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the active case is not T1.</exception>
    public T1 AsT1 => index == 1
        ? (T1)value
        : throw new InvalidOperationException("Value is not T1.");
    /// <summary>
    /// Gets the value as <typeparamref name="T2"/>.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the active case is not T2.</exception>
    public T2 AsT2 => index == 2
        ? (T2)value
        : throw new InvalidOperationException("Value is not T2.");
    /// <summary>
    /// Gets the value as <typeparamref name="T3"/>.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the active case is not T3.</exception>
    public T3 AsT3 => index == 3
        ? (T3)value
        : throw new InvalidOperationException("Value is not T3.");
    /// <summary>
    /// Gets the value as <typeparamref name="T4"/>.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the active case is not T4.</exception>
    public T4 AsT4 => index == 4
        ? (T4)value
        : throw new InvalidOperationException("Value is not T4.");
    /// <summary>
    /// Gets the value as <typeparamref name="T5"/>.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the active case is not T5.</exception>
    public T5 AsT5 => index == 5
        ? (T5)value
        : throw new InvalidOperationException("Value is not T5.");
    /// <summary>
    /// Gets the value as <typeparamref name="T6"/>.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the active case is not T6.</exception>
    public T6 AsT6 => index == 6
        ? (T6)value
        : throw new InvalidOperationException("Value is not T6.");

    /// <summary>
    /// Matches the union to one of 6 functions based on the active case.
    /// </summary>
    /// <typeparam name="TResult">The result type.</typeparam>
    /// <param name="t1">Handler for the first case.</param>
    /// <param name="t2">Handler for the second case.</param>
    /// <param name="t3">Handler for the third case.</param>
    /// <param name="t4">Handler for the fourth case.</param>
    /// <param name="t5">Handler for the fifth case.</param>
    /// <param name="t6">Handler for the sixth case.</param>
    /// <returns>The result of the matching handler.</returns>
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

    /// <summary>
    /// Asynchronously matches the union to one of 6 functions based on the active case.
    /// </summary>
    /// <typeparam name="TResult">The result type.</typeparam>
    /// <param name="t1">Async handler for the first case.</param>
    /// <param name="t2">Async handler for the second case.</param>
    /// <param name="t3">Async handler for the third case.</param>
    /// <param name="t4">Async handler for the fourth case.</param>
    /// <param name="t5">Async handler for the fifth case.</param>
    /// <param name="t6">Async handler for the sixth case.</param>
    /// <returns>A task containing the result of the matching handler.</returns>
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

    /// <summary>
    /// Transforms the first case using the provided function, leaving all other cases unchanged.
    /// </summary>
    /// <typeparam name="T1Result">The transformed type for the first case.</typeparam>
    /// <param name="mapper">Function to transform the first value.</param>
    /// <returns>A union with the first case transformed, or the original value if another case is active.</returns>
    public OneOf<T1Result, T2, T3, T4, T5, T6> MapFirst<T1Result>(Func<T1, T1Result> mapper) =>
        Match<OneOf<T1Result, T2, T3, T4, T5, T6>>(t1 => mapper(t1), t2 => t2, t3 => t3, t4 => t4, t5 => t5, t6 => t6);

    /// <summary>
    /// Transforms the second case using the provided function, leaving all other cases unchanged.
    /// </summary>
    /// <typeparam name="T2Result">The transformed type for the second case.</typeparam>
    /// <param name="mapper">Function to transform the second value.</param>
    /// <returns>A union with the second case transformed, or the original value if another case is active.</returns>
    public OneOf<T1, T2Result, T3, T4, T5, T6> MapSecond<T2Result>(Func<T2, T2Result> mapper) =>
        Match<OneOf<T1, T2Result, T3, T4, T5, T6>>(t1 => t1, t2 => mapper(t2), t3 => t3, t4 => t4, t5 => t5, t6 => t6);

    /// <summary>
    /// Transforms the third case using the provided function, leaving all other cases unchanged.
    /// </summary>
    /// <typeparam name="T3Result">The transformed type for the third case.</typeparam>
    /// <param name="mapper">Function to transform the third value.</param>
    /// <returns>A union with the third case transformed, or the original value if another case is active.</returns>
    public OneOf<T1, T2, T3Result, T4, T5, T6> MapThird<T3Result>(Func<T3, T3Result> mapper) =>
        Match<OneOf<T1, T2, T3Result, T4, T5, T6>>(t1 => t1, t2 => t2, t3 => mapper(t3), t4 => t4, t5 => t5, t6 => t6);

    /// <summary>
    /// Transforms the fourth case using the provided function, leaving all other cases unchanged.
    /// </summary>
    /// <typeparam name="T4Result">The transformed type for the fourth case.</typeparam>
    /// <param name="mapper">Function to transform the fourth value.</param>
    /// <returns>A union with the fourth case transformed, or the original value if another case is active.</returns>
    public OneOf<T1, T2, T3, T4Result, T5, T6> MapFourth<T4Result>(Func<T4, T4Result> mapper) =>
        Match<OneOf<T1, T2, T3, T4Result, T5, T6>>(t1 => t1, t2 => t2, t3 => t3, t4 => mapper(t4), t5 => t5, t6 => t6);

    /// <summary>
    /// Transforms the fifth case using the provided function, leaving all other cases unchanged.
    /// </summary>
    /// <typeparam name="T5Result">The transformed type for the fifth case.</typeparam>
    /// <param name="mapper">Function to transform the fifth value.</param>
    /// <returns>A union with the fifth case transformed, or the original value if another case is active.</returns>
    public OneOf<T1, T2, T3, T4, T5Result, T6> MapFifth<T5Result>(Func<T5, T5Result> mapper) =>
        Match<OneOf<T1, T2, T3, T4, T5Result, T6>>(t1 => t1, t2 => t2, t3 => t3, t4 => t4, t5 => mapper(t5), t6 => t6);

    /// <summary>
    /// Transforms the sixth case using the provided function, leaving all other cases unchanged.
    /// </summary>
    /// <typeparam name="T6Result">The transformed type for the sixth case.</typeparam>
    /// <param name="mapper">Function to transform the sixth value.</param>
    /// <returns>A union with the sixth case transformed, or the original value if another case is active.</returns>
    public OneOf<T1, T2, T3, T4, T5, T6Result> MapSixth<T6Result>(Func<T6, T6Result> mapper) =>
        Match<OneOf<T1, T2, T3, T4, T5, T6Result>>(t1 => t1, t2 => t2, t3 => t3, t4 => t4, t5 => t5, t6 => mapper(t6));

    /// <summary>
    /// Collapses the first case into the remaining cases, producing a union with one fewer type parameter.
    /// </summary>
    /// <param name="reducer">Function that maps the first value into the smaller union.</param>
    /// <returns>A union of 5 types with the first case eliminated.</returns>
    public OneOf<T2, T3, T4, T5, T6> ReduceFirst(Func<T1, OneOf<T2, T3, T4, T5, T6>> reducer) =>
        Match<OneOf<T2, T3, T4, T5, T6>>(t1 => reducer(t1), t2 => t2, t3 => t3, t4 => t4, t5 => t5, t6 => t6);

    /// <summary>
    /// Collapses the second case into the remaining cases, producing a union with one fewer type parameter.
    /// </summary>
    /// <param name="reducer">Function that maps the second value into the smaller union.</param>
    /// <returns>A union of 5 types with the second case eliminated.</returns>
    public OneOf<T1, T3, T4, T5, T6> ReduceSecond(Func<T2, OneOf<T1, T3, T4, T5, T6>> reducer) =>
        Match<OneOf<T1, T3, T4, T5, T6>>(t1 => t1, t2 => reducer(t2), t3 => t3, t4 => t4, t5 => t5, t6 => t6);

    /// <summary>
    /// Collapses the third case into the remaining cases, producing a union with one fewer type parameter.
    /// </summary>
    /// <param name="reducer">Function that maps the third value into the smaller union.</param>
    /// <returns>A union of 5 types with the third case eliminated.</returns>
    public OneOf<T1, T2, T4, T5, T6> ReduceThird(Func<T3, OneOf<T1, T2, T4, T5, T6>> reducer) =>
        Match<OneOf<T1, T2, T4, T5, T6>>(t1 => t1, t2 => t2, t3 => reducer(t3), t4 => t4, t5 => t5, t6 => t6);

    /// <summary>
    /// Collapses the fourth case into the remaining cases, producing a union with one fewer type parameter.
    /// </summary>
    /// <param name="reducer">Function that maps the fourth value into the smaller union.</param>
    /// <returns>A union of 5 types with the fourth case eliminated.</returns>
    public OneOf<T1, T2, T3, T5, T6> ReduceFourth(Func<T4, OneOf<T1, T2, T3, T5, T6>> reducer) =>
        Match<OneOf<T1, T2, T3, T5, T6>>(t1 => t1, t2 => t2, t3 => t3, t4 => reducer(t4), t5 => t5, t6 => t6);

    /// <summary>
    /// Collapses the fifth case into the remaining cases, producing a union with one fewer type parameter.
    /// </summary>
    /// <param name="reducer">Function that maps the fifth value into the smaller union.</param>
    /// <returns>A union of 5 types with the fifth case eliminated.</returns>
    public OneOf<T1, T2, T3, T4, T6> ReduceFifth(Func<T5, OneOf<T1, T2, T3, T4, T6>> reducer) =>
        Match<OneOf<T1, T2, T3, T4, T6>>(t1 => t1, t2 => t2, t3 => t3, t4 => t4, t5 => reducer(t5), t6 => t6);

    /// <summary>
    /// Collapses the sixth case into the remaining cases, producing a union with one fewer type parameter.
    /// </summary>
    /// <param name="reducer">Function that maps the sixth value into the smaller union.</param>
    /// <returns>A union of 5 types with the sixth case eliminated.</returns>
    public OneOf<T1, T2, T3, T4, T5> ReduceSixth(Func<T6, OneOf<T1, T2, T3, T4, T5>> reducer) =>
        Match<OneOf<T1, T2, T3, T4, T5>>(t1 => t1, t2 => t2, t3 => t3, t4 => t4, t5 => t5, t6 => reducer(t6));

    /// <summary>
    /// Implicitly converts a <typeparamref name="T1"/> value into the first case of the union.
    /// </summary>
    /// <param name="value">The value to wrap.</param>
    /// <returns>A union holding <paramref name="value"/> as the first case.</returns>
    public static implicit operator OneOf<T1, T2, T3, T4, T5, T6>(T1 value) => new(1, value!);
    /// <summary>
    /// Implicitly converts a <typeparamref name="T2"/> value into the second case of the union.
    /// </summary>
    /// <param name="value">The value to wrap.</param>
    /// <returns>A union holding <paramref name="value"/> as the second case.</returns>
    public static implicit operator OneOf<T1, T2, T3, T4, T5, T6>(T2 value) => new(2, value!);
    /// <summary>
    /// Implicitly converts a <typeparamref name="T3"/> value into the third case of the union.
    /// </summary>
    /// <param name="value">The value to wrap.</param>
    /// <returns>A union holding <paramref name="value"/> as the third case.</returns>
    public static implicit operator OneOf<T1, T2, T3, T4, T5, T6>(T3 value) => new(3, value!);
    /// <summary>
    /// Implicitly converts a <typeparamref name="T4"/> value into the fourth case of the union.
    /// </summary>
    /// <param name="value">The value to wrap.</param>
    /// <returns>A union holding <paramref name="value"/> as the fourth case.</returns>
    public static implicit operator OneOf<T1, T2, T3, T4, T5, T6>(T4 value) => new(4, value!);
    /// <summary>
    /// Implicitly converts a <typeparamref name="T5"/> value into the fifth case of the union.
    /// </summary>
    /// <param name="value">The value to wrap.</param>
    /// <returns>A union holding <paramref name="value"/> as the fifth case.</returns>
    public static implicit operator OneOf<T1, T2, T3, T4, T5, T6>(T5 value) => new(5, value!);
    /// <summary>
    /// Implicitly converts a <typeparamref name="T6"/> value into the sixth case of the union.
    /// </summary>
    /// <param name="value">The value to wrap.</param>
    /// <returns>A union holding <paramref name="value"/> as the sixth case.</returns>
    public static implicit operator OneOf<T1, T2, T3, T4, T5, T6>(T6 value) => new(6, value!);

    /// <summary>
    /// Projects the sixth case using the provided selector (LINQ select).
    /// Earlier cases short-circuit through unchanged.
    /// </summary>
    /// <typeparam name="TResult">The projected type.</typeparam>
    /// <param name="selector">Function to transform the sixth value.</param>
    /// <returns>A union with the sixth case projected, or the original value if another case is active.</returns>
    public OneOf<T1, T2, T3, T4, T5, TResult> Select<TResult>(Func<T6, TResult> selector) =>
        MapSixth(selector);

    /// <summary>
    /// Projects and flattens the sixth case (LINQ bind / flatMap).
    /// Earlier cases short-circuit through unchanged.
    /// </summary>
    /// <typeparam name="TResult">The projected type.</typeparam>
    /// <param name="selector">Function that returns a union from the sixth value.</param>
    /// <returns>The union returned by <paramref name="selector"/>, or the original value if another case is active.</returns>
    public OneOf<T1, T2, T3, T4, T5, TResult> SelectMany<TResult>(Func<T6, OneOf<T1, T2, T3, T4, T5, TResult>> selector) =>
        Match<OneOf<T1, T2, T3, T4, T5, TResult>>(t1 => t1, t2 => t2, t3 => t3, t4 => t4, t5 => t5, t6 => selector(t6));

    /// <summary>
    /// Projects and flattens the sixth case with an intermediate projection (LINQ query syntax).
    /// Earlier cases short-circuit through unchanged.
    /// </summary>
    /// <typeparam name="TIntermediate">The intermediate projected type.</typeparam>
    /// <typeparam name="TResult">The final projected type.</typeparam>
    /// <param name="selector">Function that returns a union from the sixth value.</param>
    /// <param name="projector">Function that combines the original and intermediate values.</param>
    /// <returns>A union with the projected result, or the original value if another case is active.</returns>
    public OneOf<T1, T2, T3, T4, T5, TResult> SelectMany<TIntermediate, TResult>(
        Func<T6, OneOf<T1, T2, T3, T4, T5, TIntermediate>> selector,
        Func<T6, TIntermediate, TResult> projector) =>
        SelectMany(last => selector(last).Select(intermediate => projector(last, intermediate)));
}

/// <summary>
/// Represents a discriminated union of 7 possible types.
/// Unlike Result, no case is inherently success or failure.
/// </summary>
/// <typeparam name="T1">The type of the first case.</typeparam>
/// <typeparam name="T2">The type of the second case.</typeparam>
/// <typeparam name="T3">The type of the third case.</typeparam>
/// <typeparam name="T4">The type of the fourth case.</typeparam>
/// <typeparam name="T5">The type of the fifth case.</typeparam>
/// <typeparam name="T6">The type of the sixth case.</typeparam>
/// <typeparam name="T7">The type of the seventh case.</typeparam>
public sealed record OneOf<T1, T2, T3, T4, T5, T6, T7>
{
    private readonly object value;
    private readonly byte index;

    private OneOf(byte index, object value)
    {
        this.index = index;
        this.value = value;
    }

    /// <summary>
    /// Gets a value indicating whether this instance holds a <typeparamref name="T1"/> value.
    /// </summary>
    public bool IsT1 => index == 1;
    /// <summary>
    /// Gets a value indicating whether this instance holds a <typeparamref name="T2"/> value.
    /// </summary>
    public bool IsT2 => index == 2;
    /// <summary>
    /// Gets a value indicating whether this instance holds a <typeparamref name="T3"/> value.
    /// </summary>
    public bool IsT3 => index == 3;
    /// <summary>
    /// Gets a value indicating whether this instance holds a <typeparamref name="T4"/> value.
    /// </summary>
    public bool IsT4 => index == 4;
    /// <summary>
    /// Gets a value indicating whether this instance holds a <typeparamref name="T5"/> value.
    /// </summary>
    public bool IsT5 => index == 5;
    /// <summary>
    /// Gets a value indicating whether this instance holds a <typeparamref name="T6"/> value.
    /// </summary>
    public bool IsT6 => index == 6;
    /// <summary>
    /// Gets a value indicating whether this instance holds a <typeparamref name="T7"/> value.
    /// </summary>
    public bool IsT7 => index == 7;

    /// <summary>
    /// Gets the value as <typeparamref name="T1"/>.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the active case is not T1.</exception>
    public T1 AsT1 => index == 1
        ? (T1)value
        : throw new InvalidOperationException("Value is not T1.");
    /// <summary>
    /// Gets the value as <typeparamref name="T2"/>.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the active case is not T2.</exception>
    public T2 AsT2 => index == 2
        ? (T2)value
        : throw new InvalidOperationException("Value is not T2.");
    /// <summary>
    /// Gets the value as <typeparamref name="T3"/>.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the active case is not T3.</exception>
    public T3 AsT3 => index == 3
        ? (T3)value
        : throw new InvalidOperationException("Value is not T3.");
    /// <summary>
    /// Gets the value as <typeparamref name="T4"/>.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the active case is not T4.</exception>
    public T4 AsT4 => index == 4
        ? (T4)value
        : throw new InvalidOperationException("Value is not T4.");
    /// <summary>
    /// Gets the value as <typeparamref name="T5"/>.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the active case is not T5.</exception>
    public T5 AsT5 => index == 5
        ? (T5)value
        : throw new InvalidOperationException("Value is not T5.");
    /// <summary>
    /// Gets the value as <typeparamref name="T6"/>.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the active case is not T6.</exception>
    public T6 AsT6 => index == 6
        ? (T6)value
        : throw new InvalidOperationException("Value is not T6.");
    /// <summary>
    /// Gets the value as <typeparamref name="T7"/>.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the active case is not T7.</exception>
    public T7 AsT7 => index == 7
        ? (T7)value
        : throw new InvalidOperationException("Value is not T7.");

    /// <summary>
    /// Matches the union to one of 7 functions based on the active case.
    /// </summary>
    /// <typeparam name="TResult">The result type.</typeparam>
    /// <param name="t1">Handler for the first case.</param>
    /// <param name="t2">Handler for the second case.</param>
    /// <param name="t3">Handler for the third case.</param>
    /// <param name="t4">Handler for the fourth case.</param>
    /// <param name="t5">Handler for the fifth case.</param>
    /// <param name="t6">Handler for the sixth case.</param>
    /// <param name="t7">Handler for the seventh case.</param>
    /// <returns>The result of the matching handler.</returns>
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

    /// <summary>
    /// Asynchronously matches the union to one of 7 functions based on the active case.
    /// </summary>
    /// <typeparam name="TResult">The result type.</typeparam>
    /// <param name="t1">Async handler for the first case.</param>
    /// <param name="t2">Async handler for the second case.</param>
    /// <param name="t3">Async handler for the third case.</param>
    /// <param name="t4">Async handler for the fourth case.</param>
    /// <param name="t5">Async handler for the fifth case.</param>
    /// <param name="t6">Async handler for the sixth case.</param>
    /// <param name="t7">Async handler for the seventh case.</param>
    /// <returns>A task containing the result of the matching handler.</returns>
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

    /// <summary>
    /// Transforms the first case using the provided function, leaving all other cases unchanged.
    /// </summary>
    /// <typeparam name="T1Result">The transformed type for the first case.</typeparam>
    /// <param name="mapper">Function to transform the first value.</param>
    /// <returns>A union with the first case transformed, or the original value if another case is active.</returns>
    public OneOf<T1Result, T2, T3, T4, T5, T6, T7> MapFirst<T1Result>(Func<T1, T1Result> mapper) =>
        Match<OneOf<T1Result, T2, T3, T4, T5, T6, T7>>(t1 => mapper(t1), t2 => t2, t3 => t3, t4 => t4, t5 => t5, t6 => t6, t7 => t7);

    /// <summary>
    /// Transforms the second case using the provided function, leaving all other cases unchanged.
    /// </summary>
    /// <typeparam name="T2Result">The transformed type for the second case.</typeparam>
    /// <param name="mapper">Function to transform the second value.</param>
    /// <returns>A union with the second case transformed, or the original value if another case is active.</returns>
    public OneOf<T1, T2Result, T3, T4, T5, T6, T7> MapSecond<T2Result>(Func<T2, T2Result> mapper) =>
        Match<OneOf<T1, T2Result, T3, T4, T5, T6, T7>>(t1 => t1, t2 => mapper(t2), t3 => t3, t4 => t4, t5 => t5, t6 => t6, t7 => t7);

    /// <summary>
    /// Transforms the third case using the provided function, leaving all other cases unchanged.
    /// </summary>
    /// <typeparam name="T3Result">The transformed type for the third case.</typeparam>
    /// <param name="mapper">Function to transform the third value.</param>
    /// <returns>A union with the third case transformed, or the original value if another case is active.</returns>
    public OneOf<T1, T2, T3Result, T4, T5, T6, T7> MapThird<T3Result>(Func<T3, T3Result> mapper) =>
        Match<OneOf<T1, T2, T3Result, T4, T5, T6, T7>>(t1 => t1, t2 => t2, t3 => mapper(t3), t4 => t4, t5 => t5, t6 => t6, t7 => t7);

    /// <summary>
    /// Transforms the fourth case using the provided function, leaving all other cases unchanged.
    /// </summary>
    /// <typeparam name="T4Result">The transformed type for the fourth case.</typeparam>
    /// <param name="mapper">Function to transform the fourth value.</param>
    /// <returns>A union with the fourth case transformed, or the original value if another case is active.</returns>
    public OneOf<T1, T2, T3, T4Result, T5, T6, T7> MapFourth<T4Result>(Func<T4, T4Result> mapper) =>
        Match<OneOf<T1, T2, T3, T4Result, T5, T6, T7>>(t1 => t1, t2 => t2, t3 => t3, t4 => mapper(t4), t5 => t5, t6 => t6, t7 => t7);

    /// <summary>
    /// Transforms the fifth case using the provided function, leaving all other cases unchanged.
    /// </summary>
    /// <typeparam name="T5Result">The transformed type for the fifth case.</typeparam>
    /// <param name="mapper">Function to transform the fifth value.</param>
    /// <returns>A union with the fifth case transformed, or the original value if another case is active.</returns>
    public OneOf<T1, T2, T3, T4, T5Result, T6, T7> MapFifth<T5Result>(Func<T5, T5Result> mapper) =>
        Match<OneOf<T1, T2, T3, T4, T5Result, T6, T7>>(t1 => t1, t2 => t2, t3 => t3, t4 => t4, t5 => mapper(t5), t6 => t6, t7 => t7);

    /// <summary>
    /// Transforms the sixth case using the provided function, leaving all other cases unchanged.
    /// </summary>
    /// <typeparam name="T6Result">The transformed type for the sixth case.</typeparam>
    /// <param name="mapper">Function to transform the sixth value.</param>
    /// <returns>A union with the sixth case transformed, or the original value if another case is active.</returns>
    public OneOf<T1, T2, T3, T4, T5, T6Result, T7> MapSixth<T6Result>(Func<T6, T6Result> mapper) =>
        Match<OneOf<T1, T2, T3, T4, T5, T6Result, T7>>(t1 => t1, t2 => t2, t3 => t3, t4 => t4, t5 => t5, t6 => mapper(t6), t7 => t7);

    /// <summary>
    /// Transforms the seventh case using the provided function, leaving all other cases unchanged.
    /// </summary>
    /// <typeparam name="T7Result">The transformed type for the seventh case.</typeparam>
    /// <param name="mapper">Function to transform the seventh value.</param>
    /// <returns>A union with the seventh case transformed, or the original value if another case is active.</returns>
    public OneOf<T1, T2, T3, T4, T5, T6, T7Result> MapSeventh<T7Result>(Func<T7, T7Result> mapper) =>
        Match<OneOf<T1, T2, T3, T4, T5, T6, T7Result>>(t1 => t1, t2 => t2, t3 => t3, t4 => t4, t5 => t5, t6 => t6, t7 => mapper(t7));

    /// <summary>
    /// Collapses the first case into the remaining cases, producing a union with one fewer type parameter.
    /// </summary>
    /// <param name="reducer">Function that maps the first value into the smaller union.</param>
    /// <returns>A union of 6 types with the first case eliminated.</returns>
    public OneOf<T2, T3, T4, T5, T6, T7> ReduceFirst(Func<T1, OneOf<T2, T3, T4, T5, T6, T7>> reducer) =>
        Match<OneOf<T2, T3, T4, T5, T6, T7>>(t1 => reducer(t1), t2 => t2, t3 => t3, t4 => t4, t5 => t5, t6 => t6, t7 => t7);

    /// <summary>
    /// Collapses the second case into the remaining cases, producing a union with one fewer type parameter.
    /// </summary>
    /// <param name="reducer">Function that maps the second value into the smaller union.</param>
    /// <returns>A union of 6 types with the second case eliminated.</returns>
    public OneOf<T1, T3, T4, T5, T6, T7> ReduceSecond(Func<T2, OneOf<T1, T3, T4, T5, T6, T7>> reducer) =>
        Match<OneOf<T1, T3, T4, T5, T6, T7>>(t1 => t1, t2 => reducer(t2), t3 => t3, t4 => t4, t5 => t5, t6 => t6, t7 => t7);

    /// <summary>
    /// Collapses the third case into the remaining cases, producing a union with one fewer type parameter.
    /// </summary>
    /// <param name="reducer">Function that maps the third value into the smaller union.</param>
    /// <returns>A union of 6 types with the third case eliminated.</returns>
    public OneOf<T1, T2, T4, T5, T6, T7> ReduceThird(Func<T3, OneOf<T1, T2, T4, T5, T6, T7>> reducer) =>
        Match<OneOf<T1, T2, T4, T5, T6, T7>>(t1 => t1, t2 => t2, t3 => reducer(t3), t4 => t4, t5 => t5, t6 => t6, t7 => t7);

    /// <summary>
    /// Collapses the fourth case into the remaining cases, producing a union with one fewer type parameter.
    /// </summary>
    /// <param name="reducer">Function that maps the fourth value into the smaller union.</param>
    /// <returns>A union of 6 types with the fourth case eliminated.</returns>
    public OneOf<T1, T2, T3, T5, T6, T7> ReduceFourth(Func<T4, OneOf<T1, T2, T3, T5, T6, T7>> reducer) =>
        Match<OneOf<T1, T2, T3, T5, T6, T7>>(t1 => t1, t2 => t2, t3 => t3, t4 => reducer(t4), t5 => t5, t6 => t6, t7 => t7);

    /// <summary>
    /// Collapses the fifth case into the remaining cases, producing a union with one fewer type parameter.
    /// </summary>
    /// <param name="reducer">Function that maps the fifth value into the smaller union.</param>
    /// <returns>A union of 6 types with the fifth case eliminated.</returns>
    public OneOf<T1, T2, T3, T4, T6, T7> ReduceFifth(Func<T5, OneOf<T1, T2, T3, T4, T6, T7>> reducer) =>
        Match<OneOf<T1, T2, T3, T4, T6, T7>>(t1 => t1, t2 => t2, t3 => t3, t4 => t4, t5 => reducer(t5), t6 => t6, t7 => t7);

    /// <summary>
    /// Collapses the sixth case into the remaining cases, producing a union with one fewer type parameter.
    /// </summary>
    /// <param name="reducer">Function that maps the sixth value into the smaller union.</param>
    /// <returns>A union of 6 types with the sixth case eliminated.</returns>
    public OneOf<T1, T2, T3, T4, T5, T7> ReduceSixth(Func<T6, OneOf<T1, T2, T3, T4, T5, T7>> reducer) =>
        Match<OneOf<T1, T2, T3, T4, T5, T7>>(t1 => t1, t2 => t2, t3 => t3, t4 => t4, t5 => t5, t6 => reducer(t6), t7 => t7);

    /// <summary>
    /// Collapses the seventh case into the remaining cases, producing a union with one fewer type parameter.
    /// </summary>
    /// <param name="reducer">Function that maps the seventh value into the smaller union.</param>
    /// <returns>A union of 6 types with the seventh case eliminated.</returns>
    public OneOf<T1, T2, T3, T4, T5, T6> ReduceSeventh(Func<T7, OneOf<T1, T2, T3, T4, T5, T6>> reducer) =>
        Match<OneOf<T1, T2, T3, T4, T5, T6>>(t1 => t1, t2 => t2, t3 => t3, t4 => t4, t5 => t5, t6 => t6, t7 => reducer(t7));

    /// <summary>
    /// Implicitly converts a <typeparamref name="T1"/> value into the first case of the union.
    /// </summary>
    /// <param name="value">The value to wrap.</param>
    /// <returns>A union holding <paramref name="value"/> as the first case.</returns>
    public static implicit operator OneOf<T1, T2, T3, T4, T5, T6, T7>(T1 value) => new(1, value!);
    /// <summary>
    /// Implicitly converts a <typeparamref name="T2"/> value into the second case of the union.
    /// </summary>
    /// <param name="value">The value to wrap.</param>
    /// <returns>A union holding <paramref name="value"/> as the second case.</returns>
    public static implicit operator OneOf<T1, T2, T3, T4, T5, T6, T7>(T2 value) => new(2, value!);
    /// <summary>
    /// Implicitly converts a <typeparamref name="T3"/> value into the third case of the union.
    /// </summary>
    /// <param name="value">The value to wrap.</param>
    /// <returns>A union holding <paramref name="value"/> as the third case.</returns>
    public static implicit operator OneOf<T1, T2, T3, T4, T5, T6, T7>(T3 value) => new(3, value!);
    /// <summary>
    /// Implicitly converts a <typeparamref name="T4"/> value into the fourth case of the union.
    /// </summary>
    /// <param name="value">The value to wrap.</param>
    /// <returns>A union holding <paramref name="value"/> as the fourth case.</returns>
    public static implicit operator OneOf<T1, T2, T3, T4, T5, T6, T7>(T4 value) => new(4, value!);
    /// <summary>
    /// Implicitly converts a <typeparamref name="T5"/> value into the fifth case of the union.
    /// </summary>
    /// <param name="value">The value to wrap.</param>
    /// <returns>A union holding <paramref name="value"/> as the fifth case.</returns>
    public static implicit operator OneOf<T1, T2, T3, T4, T5, T6, T7>(T5 value) => new(5, value!);
    /// <summary>
    /// Implicitly converts a <typeparamref name="T6"/> value into the sixth case of the union.
    /// </summary>
    /// <param name="value">The value to wrap.</param>
    /// <returns>A union holding <paramref name="value"/> as the sixth case.</returns>
    public static implicit operator OneOf<T1, T2, T3, T4, T5, T6, T7>(T6 value) => new(6, value!);
    /// <summary>
    /// Implicitly converts a <typeparamref name="T7"/> value into the seventh case of the union.
    /// </summary>
    /// <param name="value">The value to wrap.</param>
    /// <returns>A union holding <paramref name="value"/> as the seventh case.</returns>
    public static implicit operator OneOf<T1, T2, T3, T4, T5, T6, T7>(T7 value) => new(7, value!);

    /// <summary>
    /// Projects the seventh case using the provided selector (LINQ select).
    /// Earlier cases short-circuit through unchanged.
    /// </summary>
    /// <typeparam name="TResult">The projected type.</typeparam>
    /// <param name="selector">Function to transform the seventh value.</param>
    /// <returns>A union with the seventh case projected, or the original value if another case is active.</returns>
    public OneOf<T1, T2, T3, T4, T5, T6, TResult> Select<TResult>(Func<T7, TResult> selector) =>
        MapSeventh(selector);

    /// <summary>
    /// Projects and flattens the seventh case (LINQ bind / flatMap).
    /// Earlier cases short-circuit through unchanged.
    /// </summary>
    /// <typeparam name="TResult">The projected type.</typeparam>
    /// <param name="selector">Function that returns a union from the seventh value.</param>
    /// <returns>The union returned by <paramref name="selector"/>, or the original value if another case is active.</returns>
    public OneOf<T1, T2, T3, T4, T5, T6, TResult> SelectMany<TResult>(Func<T7, OneOf<T1, T2, T3, T4, T5, T6, TResult>> selector) =>
        Match<OneOf<T1, T2, T3, T4, T5, T6, TResult>>(t1 => t1, t2 => t2, t3 => t3, t4 => t4, t5 => t5, t6 => t6, t7 => selector(t7));

    /// <summary>
    /// Projects and flattens the seventh case with an intermediate projection (LINQ query syntax).
    /// Earlier cases short-circuit through unchanged.
    /// </summary>
    /// <typeparam name="TIntermediate">The intermediate projected type.</typeparam>
    /// <typeparam name="TResult">The final projected type.</typeparam>
    /// <param name="selector">Function that returns a union from the seventh value.</param>
    /// <param name="projector">Function that combines the original and intermediate values.</param>
    /// <returns>A union with the projected result, or the original value if another case is active.</returns>
    public OneOf<T1, T2, T3, T4, T5, T6, TResult> SelectMany<TIntermediate, TResult>(
        Func<T7, OneOf<T1, T2, T3, T4, T5, T6, TIntermediate>> selector,
        Func<T7, TIntermediate, TResult> projector) =>
        SelectMany(last => selector(last).Select(intermediate => projector(last, intermediate)));
}

/// <summary>
/// Represents a discriminated union of 8 possible types.
/// Unlike Result, no case is inherently success or failure.
/// </summary>
/// <typeparam name="T1">The type of the first case.</typeparam>
/// <typeparam name="T2">The type of the second case.</typeparam>
/// <typeparam name="T3">The type of the third case.</typeparam>
/// <typeparam name="T4">The type of the fourth case.</typeparam>
/// <typeparam name="T5">The type of the fifth case.</typeparam>
/// <typeparam name="T6">The type of the sixth case.</typeparam>
/// <typeparam name="T7">The type of the seventh case.</typeparam>
/// <typeparam name="T8">The type of the eighth case.</typeparam>
public sealed record OneOf<T1, T2, T3, T4, T5, T6, T7, T8>
{
    private readonly object value;
    private readonly byte index;

    private OneOf(byte index, object value)
    {
        this.index = index;
        this.value = value;
    }

    /// <summary>
    /// Gets a value indicating whether this instance holds a <typeparamref name="T1"/> value.
    /// </summary>
    public bool IsT1 => index == 1;
    /// <summary>
    /// Gets a value indicating whether this instance holds a <typeparamref name="T2"/> value.
    /// </summary>
    public bool IsT2 => index == 2;
    /// <summary>
    /// Gets a value indicating whether this instance holds a <typeparamref name="T3"/> value.
    /// </summary>
    public bool IsT3 => index == 3;
    /// <summary>
    /// Gets a value indicating whether this instance holds a <typeparamref name="T4"/> value.
    /// </summary>
    public bool IsT4 => index == 4;
    /// <summary>
    /// Gets a value indicating whether this instance holds a <typeparamref name="T5"/> value.
    /// </summary>
    public bool IsT5 => index == 5;
    /// <summary>
    /// Gets a value indicating whether this instance holds a <typeparamref name="T6"/> value.
    /// </summary>
    public bool IsT6 => index == 6;
    /// <summary>
    /// Gets a value indicating whether this instance holds a <typeparamref name="T7"/> value.
    /// </summary>
    public bool IsT7 => index == 7;
    /// <summary>
    /// Gets a value indicating whether this instance holds a <typeparamref name="T8"/> value.
    /// </summary>
    public bool IsT8 => index == 8;

    /// <summary>
    /// Gets the value as <typeparamref name="T1"/>.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the active case is not T1.</exception>
    public T1 AsT1 => index == 1
        ? (T1)value
        : throw new InvalidOperationException("Value is not T1.");
    /// <summary>
    /// Gets the value as <typeparamref name="T2"/>.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the active case is not T2.</exception>
    public T2 AsT2 => index == 2
        ? (T2)value
        : throw new InvalidOperationException("Value is not T2.");
    /// <summary>
    /// Gets the value as <typeparamref name="T3"/>.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the active case is not T3.</exception>
    public T3 AsT3 => index == 3
        ? (T3)value
        : throw new InvalidOperationException("Value is not T3.");
    /// <summary>
    /// Gets the value as <typeparamref name="T4"/>.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the active case is not T4.</exception>
    public T4 AsT4 => index == 4
        ? (T4)value
        : throw new InvalidOperationException("Value is not T4.");
    /// <summary>
    /// Gets the value as <typeparamref name="T5"/>.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the active case is not T5.</exception>
    public T5 AsT5 => index == 5
        ? (T5)value
        : throw new InvalidOperationException("Value is not T5.");
    /// <summary>
    /// Gets the value as <typeparamref name="T6"/>.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the active case is not T6.</exception>
    public T6 AsT6 => index == 6
        ? (T6)value
        : throw new InvalidOperationException("Value is not T6.");
    /// <summary>
    /// Gets the value as <typeparamref name="T7"/>.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the active case is not T7.</exception>
    public T7 AsT7 => index == 7
        ? (T7)value
        : throw new InvalidOperationException("Value is not T7.");
    /// <summary>
    /// Gets the value as <typeparamref name="T8"/>.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the active case is not T8.</exception>
    public T8 AsT8 => index == 8
        ? (T8)value
        : throw new InvalidOperationException("Value is not T8.");

    /// <summary>
    /// Matches the union to one of 8 functions based on the active case.
    /// </summary>
    /// <typeparam name="TResult">The result type.</typeparam>
    /// <param name="t1">Handler for the first case.</param>
    /// <param name="t2">Handler for the second case.</param>
    /// <param name="t3">Handler for the third case.</param>
    /// <param name="t4">Handler for the fourth case.</param>
    /// <param name="t5">Handler for the fifth case.</param>
    /// <param name="t6">Handler for the sixth case.</param>
    /// <param name="t7">Handler for the seventh case.</param>
    /// <param name="t8">Handler for the eighth case.</param>
    /// <returns>The result of the matching handler.</returns>
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

    /// <summary>
    /// Asynchronously matches the union to one of 8 functions based on the active case.
    /// </summary>
    /// <typeparam name="TResult">The result type.</typeparam>
    /// <param name="t1">Async handler for the first case.</param>
    /// <param name="t2">Async handler for the second case.</param>
    /// <param name="t3">Async handler for the third case.</param>
    /// <param name="t4">Async handler for the fourth case.</param>
    /// <param name="t5">Async handler for the fifth case.</param>
    /// <param name="t6">Async handler for the sixth case.</param>
    /// <param name="t7">Async handler for the seventh case.</param>
    /// <param name="t8">Async handler for the eighth case.</param>
    /// <returns>A task containing the result of the matching handler.</returns>
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

    /// <summary>
    /// Transforms the first case using the provided function, leaving all other cases unchanged.
    /// </summary>
    /// <typeparam name="T1Result">The transformed type for the first case.</typeparam>
    /// <param name="mapper">Function to transform the first value.</param>
    /// <returns>A union with the first case transformed, or the original value if another case is active.</returns>
    public OneOf<T1Result, T2, T3, T4, T5, T6, T7, T8> MapFirst<T1Result>(Func<T1, T1Result> mapper) =>
        Match<OneOf<T1Result, T2, T3, T4, T5, T6, T7, T8>>(t1 => mapper(t1), t2 => t2, t3 => t3, t4 => t4, t5 => t5, t6 => t6, t7 => t7, t8 => t8);

    /// <summary>
    /// Transforms the second case using the provided function, leaving all other cases unchanged.
    /// </summary>
    /// <typeparam name="T2Result">The transformed type for the second case.</typeparam>
    /// <param name="mapper">Function to transform the second value.</param>
    /// <returns>A union with the second case transformed, or the original value if another case is active.</returns>
    public OneOf<T1, T2Result, T3, T4, T5, T6, T7, T8> MapSecond<T2Result>(Func<T2, T2Result> mapper) =>
        Match<OneOf<T1, T2Result, T3, T4, T5, T6, T7, T8>>(t1 => t1, t2 => mapper(t2), t3 => t3, t4 => t4, t5 => t5, t6 => t6, t7 => t7, t8 => t8);

    /// <summary>
    /// Transforms the third case using the provided function, leaving all other cases unchanged.
    /// </summary>
    /// <typeparam name="T3Result">The transformed type for the third case.</typeparam>
    /// <param name="mapper">Function to transform the third value.</param>
    /// <returns>A union with the third case transformed, or the original value if another case is active.</returns>
    public OneOf<T1, T2, T3Result, T4, T5, T6, T7, T8> MapThird<T3Result>(Func<T3, T3Result> mapper) =>
        Match<OneOf<T1, T2, T3Result, T4, T5, T6, T7, T8>>(t1 => t1, t2 => t2, t3 => mapper(t3), t4 => t4, t5 => t5, t6 => t6, t7 => t7, t8 => t8);

    /// <summary>
    /// Transforms the fourth case using the provided function, leaving all other cases unchanged.
    /// </summary>
    /// <typeparam name="T4Result">The transformed type for the fourth case.</typeparam>
    /// <param name="mapper">Function to transform the fourth value.</param>
    /// <returns>A union with the fourth case transformed, or the original value if another case is active.</returns>
    public OneOf<T1, T2, T3, T4Result, T5, T6, T7, T8> MapFourth<T4Result>(Func<T4, T4Result> mapper) =>
        Match<OneOf<T1, T2, T3, T4Result, T5, T6, T7, T8>>(t1 => t1, t2 => t2, t3 => t3, t4 => mapper(t4), t5 => t5, t6 => t6, t7 => t7, t8 => t8);

    /// <summary>
    /// Transforms the fifth case using the provided function, leaving all other cases unchanged.
    /// </summary>
    /// <typeparam name="T5Result">The transformed type for the fifth case.</typeparam>
    /// <param name="mapper">Function to transform the fifth value.</param>
    /// <returns>A union with the fifth case transformed, or the original value if another case is active.</returns>
    public OneOf<T1, T2, T3, T4, T5Result, T6, T7, T8> MapFifth<T5Result>(Func<T5, T5Result> mapper) =>
        Match<OneOf<T1, T2, T3, T4, T5Result, T6, T7, T8>>(t1 => t1, t2 => t2, t3 => t3, t4 => t4, t5 => mapper(t5), t6 => t6, t7 => t7, t8 => t8);

    /// <summary>
    /// Transforms the sixth case using the provided function, leaving all other cases unchanged.
    /// </summary>
    /// <typeparam name="T6Result">The transformed type for the sixth case.</typeparam>
    /// <param name="mapper">Function to transform the sixth value.</param>
    /// <returns>A union with the sixth case transformed, or the original value if another case is active.</returns>
    public OneOf<T1, T2, T3, T4, T5, T6Result, T7, T8> MapSixth<T6Result>(Func<T6, T6Result> mapper) =>
        Match<OneOf<T1, T2, T3, T4, T5, T6Result, T7, T8>>(t1 => t1, t2 => t2, t3 => t3, t4 => t4, t5 => t5, t6 => mapper(t6), t7 => t7, t8 => t8);

    /// <summary>
    /// Transforms the seventh case using the provided function, leaving all other cases unchanged.
    /// </summary>
    /// <typeparam name="T7Result">The transformed type for the seventh case.</typeparam>
    /// <param name="mapper">Function to transform the seventh value.</param>
    /// <returns>A union with the seventh case transformed, or the original value if another case is active.</returns>
    public OneOf<T1, T2, T3, T4, T5, T6, T7Result, T8> MapSeventh<T7Result>(Func<T7, T7Result> mapper) =>
        Match<OneOf<T1, T2, T3, T4, T5, T6, T7Result, T8>>(t1 => t1, t2 => t2, t3 => t3, t4 => t4, t5 => t5, t6 => t6, t7 => mapper(t7), t8 => t8);

    /// <summary>
    /// Transforms the eighth case using the provided function, leaving all other cases unchanged.
    /// </summary>
    /// <typeparam name="T8Result">The transformed type for the eighth case.</typeparam>
    /// <param name="mapper">Function to transform the eighth value.</param>
    /// <returns>A union with the eighth case transformed, or the original value if another case is active.</returns>
    public OneOf<T1, T2, T3, T4, T5, T6, T7, T8Result> MapEighth<T8Result>(Func<T8, T8Result> mapper) =>
        Match<OneOf<T1, T2, T3, T4, T5, T6, T7, T8Result>>(t1 => t1, t2 => t2, t3 => t3, t4 => t4, t5 => t5, t6 => t6, t7 => t7, t8 => mapper(t8));

    /// <summary>
    /// Collapses the first case into the remaining cases, producing a union with one fewer type parameter.
    /// </summary>
    /// <param name="reducer">Function that maps the first value into the smaller union.</param>
    /// <returns>A union of 7 types with the first case eliminated.</returns>
    public OneOf<T2, T3, T4, T5, T6, T7, T8> ReduceFirst(Func<T1, OneOf<T2, T3, T4, T5, T6, T7, T8>> reducer) =>
        Match<OneOf<T2, T3, T4, T5, T6, T7, T8>>(t1 => reducer(t1), t2 => t2, t3 => t3, t4 => t4, t5 => t5, t6 => t6, t7 => t7, t8 => t8);

    /// <summary>
    /// Collapses the second case into the remaining cases, producing a union with one fewer type parameter.
    /// </summary>
    /// <param name="reducer">Function that maps the second value into the smaller union.</param>
    /// <returns>A union of 7 types with the second case eliminated.</returns>
    public OneOf<T1, T3, T4, T5, T6, T7, T8> ReduceSecond(Func<T2, OneOf<T1, T3, T4, T5, T6, T7, T8>> reducer) =>
        Match<OneOf<T1, T3, T4, T5, T6, T7, T8>>(t1 => t1, t2 => reducer(t2), t3 => t3, t4 => t4, t5 => t5, t6 => t6, t7 => t7, t8 => t8);

    /// <summary>
    /// Collapses the third case into the remaining cases, producing a union with one fewer type parameter.
    /// </summary>
    /// <param name="reducer">Function that maps the third value into the smaller union.</param>
    /// <returns>A union of 7 types with the third case eliminated.</returns>
    public OneOf<T1, T2, T4, T5, T6, T7, T8> ReduceThird(Func<T3, OneOf<T1, T2, T4, T5, T6, T7, T8>> reducer) =>
        Match<OneOf<T1, T2, T4, T5, T6, T7, T8>>(t1 => t1, t2 => t2, t3 => reducer(t3), t4 => t4, t5 => t5, t6 => t6, t7 => t7, t8 => t8);

    /// <summary>
    /// Collapses the fourth case into the remaining cases, producing a union with one fewer type parameter.
    /// </summary>
    /// <param name="reducer">Function that maps the fourth value into the smaller union.</param>
    /// <returns>A union of 7 types with the fourth case eliminated.</returns>
    public OneOf<T1, T2, T3, T5, T6, T7, T8> ReduceFourth(Func<T4, OneOf<T1, T2, T3, T5, T6, T7, T8>> reducer) =>
        Match<OneOf<T1, T2, T3, T5, T6, T7, T8>>(t1 => t1, t2 => t2, t3 => t3, t4 => reducer(t4), t5 => t5, t6 => t6, t7 => t7, t8 => t8);

    /// <summary>
    /// Collapses the fifth case into the remaining cases, producing a union with one fewer type parameter.
    /// </summary>
    /// <param name="reducer">Function that maps the fifth value into the smaller union.</param>
    /// <returns>A union of 7 types with the fifth case eliminated.</returns>
    public OneOf<T1, T2, T3, T4, T6, T7, T8> ReduceFifth(Func<T5, OneOf<T1, T2, T3, T4, T6, T7, T8>> reducer) =>
        Match<OneOf<T1, T2, T3, T4, T6, T7, T8>>(t1 => t1, t2 => t2, t3 => t3, t4 => t4, t5 => reducer(t5), t6 => t6, t7 => t7, t8 => t8);

    /// <summary>
    /// Collapses the sixth case into the remaining cases, producing a union with one fewer type parameter.
    /// </summary>
    /// <param name="reducer">Function that maps the sixth value into the smaller union.</param>
    /// <returns>A union of 7 types with the sixth case eliminated.</returns>
    public OneOf<T1, T2, T3, T4, T5, T7, T8> ReduceSixth(Func<T6, OneOf<T1, T2, T3, T4, T5, T7, T8>> reducer) =>
        Match<OneOf<T1, T2, T3, T4, T5, T7, T8>>(t1 => t1, t2 => t2, t3 => t3, t4 => t4, t5 => t5, t6 => reducer(t6), t7 => t7, t8 => t8);

    /// <summary>
    /// Collapses the seventh case into the remaining cases, producing a union with one fewer type parameter.
    /// </summary>
    /// <param name="reducer">Function that maps the seventh value into the smaller union.</param>
    /// <returns>A union of 7 types with the seventh case eliminated.</returns>
    public OneOf<T1, T2, T3, T4, T5, T6, T8> ReduceSeventh(Func<T7, OneOf<T1, T2, T3, T4, T5, T6, T8>> reducer) =>
        Match<OneOf<T1, T2, T3, T4, T5, T6, T8>>(t1 => t1, t2 => t2, t3 => t3, t4 => t4, t5 => t5, t6 => t6, t7 => reducer(t7), t8 => t8);

    /// <summary>
    /// Collapses the eighth case into the remaining cases, producing a union with one fewer type parameter.
    /// </summary>
    /// <param name="reducer">Function that maps the eighth value into the smaller union.</param>
    /// <returns>A union of 7 types with the eighth case eliminated.</returns>
    public OneOf<T1, T2, T3, T4, T5, T6, T7> ReduceEighth(Func<T8, OneOf<T1, T2, T3, T4, T5, T6, T7>> reducer) =>
        Match<OneOf<T1, T2, T3, T4, T5, T6, T7>>(t1 => t1, t2 => t2, t3 => t3, t4 => t4, t5 => t5, t6 => t6, t7 => t7, t8 => reducer(t8));

    /// <summary>
    /// Implicitly converts a <typeparamref name="T1"/> value into the first case of the union.
    /// </summary>
    /// <param name="value">The value to wrap.</param>
    /// <returns>A union holding <paramref name="value"/> as the first case.</returns>
    public static implicit operator OneOf<T1, T2, T3, T4, T5, T6, T7, T8>(T1 value) => new(1, value!);
    /// <summary>
    /// Implicitly converts a <typeparamref name="T2"/> value into the second case of the union.
    /// </summary>
    /// <param name="value">The value to wrap.</param>
    /// <returns>A union holding <paramref name="value"/> as the second case.</returns>
    public static implicit operator OneOf<T1, T2, T3, T4, T5, T6, T7, T8>(T2 value) => new(2, value!);
    /// <summary>
    /// Implicitly converts a <typeparamref name="T3"/> value into the third case of the union.
    /// </summary>
    /// <param name="value">The value to wrap.</param>
    /// <returns>A union holding <paramref name="value"/> as the third case.</returns>
    public static implicit operator OneOf<T1, T2, T3, T4, T5, T6, T7, T8>(T3 value) => new(3, value!);
    /// <summary>
    /// Implicitly converts a <typeparamref name="T4"/> value into the fourth case of the union.
    /// </summary>
    /// <param name="value">The value to wrap.</param>
    /// <returns>A union holding <paramref name="value"/> as the fourth case.</returns>
    public static implicit operator OneOf<T1, T2, T3, T4, T5, T6, T7, T8>(T4 value) => new(4, value!);
    /// <summary>
    /// Implicitly converts a <typeparamref name="T5"/> value into the fifth case of the union.
    /// </summary>
    /// <param name="value">The value to wrap.</param>
    /// <returns>A union holding <paramref name="value"/> as the fifth case.</returns>
    public static implicit operator OneOf<T1, T2, T3, T4, T5, T6, T7, T8>(T5 value) => new(5, value!);
    /// <summary>
    /// Implicitly converts a <typeparamref name="T6"/> value into the sixth case of the union.
    /// </summary>
    /// <param name="value">The value to wrap.</param>
    /// <returns>A union holding <paramref name="value"/> as the sixth case.</returns>
    public static implicit operator OneOf<T1, T2, T3, T4, T5, T6, T7, T8>(T6 value) => new(6, value!);
    /// <summary>
    /// Implicitly converts a <typeparamref name="T7"/> value into the seventh case of the union.
    /// </summary>
    /// <param name="value">The value to wrap.</param>
    /// <returns>A union holding <paramref name="value"/> as the seventh case.</returns>
    public static implicit operator OneOf<T1, T2, T3, T4, T5, T6, T7, T8>(T7 value) => new(7, value!);
    /// <summary>
    /// Implicitly converts a <typeparamref name="T8"/> value into the eighth case of the union.
    /// </summary>
    /// <param name="value">The value to wrap.</param>
    /// <returns>A union holding <paramref name="value"/> as the eighth case.</returns>
    public static implicit operator OneOf<T1, T2, T3, T4, T5, T6, T7, T8>(T8 value) => new(8, value!);

    /// <summary>
    /// Projects the eighth case using the provided selector (LINQ select).
    /// Earlier cases short-circuit through unchanged.
    /// </summary>
    /// <typeparam name="TResult">The projected type.</typeparam>
    /// <param name="selector">Function to transform the eighth value.</param>
    /// <returns>A union with the eighth case projected, or the original value if another case is active.</returns>
    public OneOf<T1, T2, T3, T4, T5, T6, T7, TResult> Select<TResult>(Func<T8, TResult> selector) =>
        MapEighth(selector);

    /// <summary>
    /// Projects and flattens the eighth case (LINQ bind / flatMap).
    /// Earlier cases short-circuit through unchanged.
    /// </summary>
    /// <typeparam name="TResult">The projected type.</typeparam>
    /// <param name="selector">Function that returns a union from the eighth value.</param>
    /// <returns>The union returned by <paramref name="selector"/>, or the original value if another case is active.</returns>
    public OneOf<T1, T2, T3, T4, T5, T6, T7, TResult> SelectMany<TResult>(Func<T8, OneOf<T1, T2, T3, T4, T5, T6, T7, TResult>> selector) =>
        Match<OneOf<T1, T2, T3, T4, T5, T6, T7, TResult>>(t1 => t1, t2 => t2, t3 => t3, t4 => t4, t5 => t5, t6 => t6, t7 => t7, t8 => selector(t8));

    /// <summary>
    /// Projects and flattens the eighth case with an intermediate projection (LINQ query syntax).
    /// Earlier cases short-circuit through unchanged.
    /// </summary>
    /// <typeparam name="TIntermediate">The intermediate projected type.</typeparam>
    /// <typeparam name="TResult">The final projected type.</typeparam>
    /// <param name="selector">Function that returns a union from the eighth value.</param>
    /// <param name="projector">Function that combines the original and intermediate values.</param>
    /// <returns>A union with the projected result, or the original value if another case is active.</returns>
    public OneOf<T1, T2, T3, T4, T5, T6, T7, TResult> SelectMany<TIntermediate, TResult>(
        Func<T8, OneOf<T1, T2, T3, T4, T5, T6, T7, TIntermediate>> selector,
        Func<T8, TIntermediate, TResult> projector) =>
        SelectMany(last => selector(last).Select(intermediate => projector(last, intermediate)));
}
