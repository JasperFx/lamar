using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JasperFx.Core;
using Shouldly;

namespace CodegenTests;

public static class SpecificationExtensions
{
    public static void ShouldContain<T>(this IEnumerable<T> actual, Func<T, bool> expected)
    {
        actual.Count().ShouldBeGreaterThan(0);
        actual.Any(expected).ShouldBeTrue();
    }


    public static void ShouldHaveTheSameElementsAs<T>(this IEnumerable<T> actual, IEnumerable<T> expected)
    {
        var actualList = actual is IList ? (IList)actual : actual.ToList();
        var expectedList = expected is IList ? (IList)expected : expected.ToList();

        ShouldHaveTheSameElementsAs(actualList, expectedList);
    }

    public static void ShouldHaveTheSameElementsAs<T>(this IEnumerable<T> actual, params T[] expected)
    {
        var actualList = actual is IList ? (IList)actual : actual.ToList();
        var expectedList = expected is IList ? (IList)expected : expected.ToList();

        ShouldHaveTheSameElementsAs(actualList, expectedList);
    }

    public static void ShouldHaveTheSameElementsAs(this IList actual, IList expected)
    {
        actual.ShouldNotBeNull();
        expected.ShouldNotBeNull();

        try
        {
            actual.Count.ShouldBe(expected.Count);

            for (var i = 0; i < actual.Count; i++)
            {
                actual[i].ShouldBe(expected[i]);
            }
        }
        catch (Exception)
        {
            Debug.WriteLine("ACTUAL:");
            foreach (var o in actual) Debug.WriteLine(o);
            throw;
        }
    }


    public static T IsType<T>(this object actual)
    {
        actual.ShouldBeOfType(typeof(T));
        return (T)actual;
    }

    public static object ShouldNotBeTheSameAs(this object actual, object expected)
    {
        ReferenceEquals(actual, expected).ShouldBeFalse();
        return expected;
    }

    public static void ShouldNotBeOfType<T>(this object actual)
    {
        actual.ShouldNotBeOfType(typeof(T));
    }

    public static void ShouldNotBeOfType(this object actual, Type expected)
    {
        actual.GetType().ShouldNotBe(expected);
    }


    public static IComparable ShouldBeGreaterThan(this IComparable arg1, IComparable arg2)
    {
        (arg1.CompareTo(arg2) > 0).ShouldBeTrue();

        return arg2;
    }

    public static string ShouldNotBeEmpty(this string aString)
    {
        aString.IsNotEmpty().ShouldBeTrue();

        return aString;
    }

    public static void ShouldContain(this string actual, string expected)
    {
        actual.Contains(expected).ShouldBeTrue($"Actual: {actual}{Environment.NewLine}Expected: {expected}");
    }

    public static string ShouldNotContain(this string actual, string expected)
    {
        actual.Contains(expected).ShouldBeFalse($"Actual: {actual}{Environment.NewLine}Expected: {expected}");
        return actual;
    }


    public static void ShouldStartWith(this string actual, string expected)
    {
        actual.StartsWith(expected).ShouldBeTrue();
    }
}