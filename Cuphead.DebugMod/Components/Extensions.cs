
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace BepInEx.CupheadDebugMod.Components;

internal static class VectorExtensions {
    public static string ToSimpleString(this Vector2 vector2, int decimals = 2) {
        return $"{vector2.x.ToFormattedString(decimals)}, {vector2.y.ToFormattedString(decimals)}";
    }

    public static string ToSimpleString(this Vector3 vector3, int decimals = 2) {
        return $"{vector3.x.ToFormattedString(decimals)}, {vector3.y.ToFormattedString(decimals)}";
    }
}

internal static class NumberExtensions {

    const int FRAMERATE = 60;
    public static string ToFormattedString(this float value, int decimals = 2) {
        return ((double) value).ToFormattedString(decimals);
    }

    public static string ToFormattedString(this double value, int decimals = 2) {
        return value.ToString($"F{decimals}");
    }

    public static int ToCeilingFrames(this float seconds) {
        return (int) Math.Ceiling(seconds * FRAMERATE);
    }

    public static int ToFloorFrames(this float seconds) {
        return (int) Math.Floor(seconds * FRAMERATE);
    }
}




internal static class EnumerableExtensions {
    public static bool IsEmpty<T>(this IEnumerable<T> enumerable) {
        return !enumerable.Any();
    }

    public static bool IsNullOrEmpty<T>(this IEnumerable<T> enumerable) {
        return enumerable == null || !enumerable.Any();
    }

    public static bool IsNotEmpty<T>(this IEnumerable<T> enumerable) {
        return !enumerable.IsEmpty();
    }

    public static bool IsNotNullOrEmpty<T>(this IEnumerable<T> enumerable) {
        return !enumerable.IsNullOrEmpty();
    }

    public static IEnumerable<T> SkipLast<T>(this IEnumerable<T> source, int n = 1) {
        var it = source.GetEnumerator();
        bool hasRemainingItems = false;
        var cache = new Queue<T>(n + 1);

        do {
            if (hasRemainingItems = it.MoveNext()) {
                cache.Enqueue(it.Current);
                if (cache.Count > n)
                    yield return cache.Dequeue();
            }
        } while (hasRemainingItems);
    }
}