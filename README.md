# Yuriy.FluentAssertions
Provides additional methods to use whith FluentAssertions.

Avalable as [Yuriy.FluentAssertions](https://www.nuget.org/packages/Yuriy.FluentAssertions/ "nuget package url") nuget package.

## Collection comparison with custom EqualityComparer
Let's assume there is an Item class defined like this:
```cs
public class Item
{
    public int IntProp { get; set; }

    public string StringProp { get; set; }

    public static Item ItemOne => new Item{IntProp = 1, StringProp = "One"};
    public static Item ItemTwo => new Item{IntProp = 2, StringProp = "Two"};    
}
```
By default FluentAssertions rely on Equals method of collection elements when comparing two collections for equality.
For example, if class Item does not overload the Equals method, the following test will fail:
```cs
var collection = new [] {Item.ItemOne, Item.ItemTwo};
var similarCollection = new [] {Item.ItemOne, Item.ItemTwo};

collection.Should().Equal(similarCollection);
```
One way to solve it is to override the Item.Equals method in a way that works for your test. The disadvantage of this approach is that your test-only logic now added to a class from your business domain. Also, you might want different ways to compare items in different tests, but there is only one Equals method. You might also do not have an access to the source code of the Item class.

Another way to solve this problem is to use one of FluentAssertion's Equal method overloads like this:
```cs
collection.Should().Equal(similarCollection, (x, y) => x.IntProp == y.IntProp && string.Equals(x.StringProp => y.StringProp));
```

This is typically what I use in this situation. The only downside is that it might be a bit repetetive even if I extract the equality comparison logic in a static method. It might be a bit more convinient to write your comparison logic once and use it in your tests everywhere. Enter ```EqualWithComparer``` method.

Let's define our comparison logic that we want to use in unit tests (you might even have it already defined in your domain logic and if it suits the tests than you can just use it):
```cs
private sealed class ByAllPropsComparer : IEqualityComparer<Item>
{
    public bool Equals(Item x, Item y) => x.IntProp == y.IntProp && string.Equals(x.StringProp, y.StringProp);

    public int GetHashCode(Item obj) => throw new Exception("not used in tests");
}
```
Now we can register it once and use in all our tests:
```cs
FluentAssertions.EqualityExtentions.RegisterComparer<ByAllPropsComparer>();
```
And the following test should pass as we use ```EqualWithComparer``` method:
```cs
var collection = new [] {Item.ItemOne, Item.ItemTwo};
var similarCollection = new [] {Item.ItemOne, Item.ItemTwo};

collection.Should().EqualWithComparer(similarCollection);
```

What if for a particula test we need to some other comparison logic? Use ```EqualWithComparer``` method overload that accepts a comparer. It will be used instead of any previously registered comparers:
```cs
var collection = new [] {Item.ItemOne, Item.ItemTwo};
var similarCollection = new [] {Item.ItemOne, Item.ItemTwo};

collection.Should().EqualWithComparer(similarCollection, new SomeOtherComparer());
```