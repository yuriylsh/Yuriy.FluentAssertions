using System;
using System.Collections.Generic;
using FluentAssertions;
using Xunit;
using Xunit.Sdk;

namespace Yuriy.FluentAssertions.Tests
{
    public class EqualsWithComparerTests
    {
        [Fact]
        public void Equal_NonRefEqualCollection_ProducesFalsyResult()
        {
            var collection = new [] {Item.ItemOne, Item.ItemTwo};
            var similarCollection = new [] {Item.ItemOne, Item.ItemTwo};

            collection.Should().Equal(similarCollection, (x, y) => x.IntProp == y.IntProp && x.StringProp == y.StringProp);

            Action act = () => collection.Should().Equal(similarCollection);

            act.Should().Throw<XunitException>().Where(ex => ex.Message.StartsWith("Expected collection to be equal to"));
        }


        [Fact]
        public void EqualWithComparer_UsesProvidedComparer()
        {
            var collection = new [] {Item.ItemOne, Item.ItemTwo};
            var similarCollection = new [] {Item.ItemOne, Item.ItemTwo};
            
            collection.Should().EqualWithComparer(similarCollection, new ByAllPropsComparer());
        }

        [Fact]
        public void EqualWithComparer_RegisteredComparer_UsesRegisteredComparer()
        {
            var collection = new [] {Item.ItemOne, Item.ItemTwo};
            var similarCollection = new [] {Item.ItemOne, Item.ItemTwo};

            EqualityExtentions.RegisterComparer<ByAllPropsComparer>();

            collection.Should().EqualWithComparer(similarCollection);
        }

        [Fact]
        public void EqualWithComparer_RegisteredComparerAndProvided_UsesProvidedComparer()
        {
            var collection = new [] {Item.ItemOne, Item.ItemTwo};
            var similarCollection = new [] {Item.ItemOne, Item.ItemTwo};

            EqualityExtentions.RegisterComparer<AlwaysFalsyComparer>();

            collection.Should().EqualWithComparer(similarCollection, new ByAllPropsComparer());
        }

        private class Item
        {
            public int IntProp { get; set; }

            public string StringProp { get; set; }

            public static Item ItemOne => new Item{IntProp = 1, StringProp = "One"};
            public static Item ItemTwo => new Item{IntProp = 2, StringProp = "Two"};
            
        }

        private sealed class ByAllPropsComparer : IEqualityComparer<Item>
        {
            public bool Equals(Item x, Item y) => x.IntProp == y.IntProp && string.Equals(x.StringProp, y.StringProp);

            public int GetHashCode(Item obj) => throw new Exception("not used in tests");
        }
        private sealed class AlwaysFalsyComparer : IEqualityComparer<Item>
        {
            public bool Equals(Item x, Item y) => false;

            public int GetHashCode(Item obj) => throw new Exception("not used in tests");
        }
    }
}