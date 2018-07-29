using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions.Collections;

namespace FluentAssertions
{
    public static class EqualityExtentions
    {
        public static AndConstraint<GenericCollectionAssertions<T>> EqualWithComparer<T>(
                this GenericCollectionAssertions<T> should,
                IEnumerable<T> expected,
                string because = null,
                params object[] becauseArgs)
            => EqualWithComparer(should, expected, ComparersCache.GetComparer<T>(), because, becauseArgs);

        public static AndConstraint<GenericCollectionAssertions<T>> EqualWithComparer<T>(
                this GenericCollectionAssertions<T> should,
                IEnumerable<T> expected,
                IEqualityComparer<T> comparer,
                string because = null,
                params object[] becauseArgs)
            => should.Equal(expected, comparer.Equals, because, becauseArgs);

        public static void RegisterComparer<T, TEqualityComparer>() where TEqualityComparer: IEqualityComparer<T>, new()
            => ComparersCache.RegisterComparer<T, TEqualityComparer>();

        public static void RegisterComparer<TEqualityComparer>() where TEqualityComparer: new()
            => ComparersCache.RegisterComparer<TEqualityComparer>();

        private static class ComparersCache
        {
            private static readonly object ComparersLock = new object();
            private static readonly Dictionary<Type, object> Comparers = new Dictionary<Type, object>();

            public static void RegisterComparer<T, TEqualityComparer>() where TEqualityComparer: IEqualityComparer<T>, new()
                => RegisterComparer(typeof(T), new TEqualityComparer());

            public static void RegisterComparer<TEqualityComparer>() where TEqualityComparer: new()
            {
                var comparerInterface = typeof(TEqualityComparer).GetInterfaces().FirstOrDefault(x =>
                    x.IsGenericType &&
                    x.GetGenericTypeDefinition() == typeof(IEqualityComparer<>));
                if(comparerInterface == null)
                {
                    throw new ArgumentException($"{typeof(TEqualityComparer).Name} must implement IEqualityComparer<T>.");
                }

                var typeToCompare = comparerInterface.GetGenericArguments().First();
                RegisterComparer(typeToCompare, new TEqualityComparer());
            }

            private static void RegisterComparer(Type typeToCompare, object equalityComparer)
            {
                lock(ComparersLock)
                {
                    Comparers[typeToCompare] = equalityComparer;
                }
            }

            public static IEqualityComparer<T> GetComparer<T>()
            {
                lock(ComparersLock)
                {
                    if(Comparers.TryGetValue(typeof(T), out var comparer))
                    {
                        return (IEqualityComparer<T>)comparer;
                    }
                }
                throw new Exception($"IEqualityComparer<{typeof(T).Name}> was not registered. Use {nameof(EqualityExtentions)}.{nameof(RegisterComparer)} method to register a comparer first or use {nameof(EqualityExtentions)}.EqualWithComparer that accepts a comparer as a parameter.");
            }
        }
    }
}