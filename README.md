[![Paypal](https://www.buymeacoffee.com/assets/img/custom_images/orange_img.png)](https://www.paypal.com/donate/?hosted_button_id=MC59U7TDE3KCQ)

[![Paypal](https://www.buymeacoffee.com/assets/img/custom_images/orange_img.png)]([https://www.paypal.com/donate/?hosted_button_id=MC59U7TDE3KCQ](https://png.pngtree.com/png-vector/20220603/ourmid/pngtree-donate-button-png-image_4813535.png))


# UltraMapper
[![Build status](https://ci.appveyor.com/api/projects/status/github/maurosampietro/UltraMapper?svg=true)](https://ci.appveyor.com/project/maurosampietro/ultramapper/branch/master)
[![NuGet](http://img.shields.io/nuget/v/UltraMapper.svg)](https://www.nuget.org/packages/UltraMapper/)


A nicely coded object-mapper for .NET 



What is UltraMapper?
--------------------------------

UltraMapper is a .NET mapper, that is, a tool that avoids you the need to write the code needed to copy values from a source object to a target object. It avoids you the need to manually write the (boring) code that reads the value from the source and instantiate/assign the relative member on the target object.

It can be used to get deep copies of an object or map an object to another type.

Consider this simple class:

````c#
public class Person
{
    public DateTime Birthday { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string EmailAddress { get; set; }
}
````

If you wanted a copy of an instance of the above class your should write something like this:

````c#
var clone = new Person();
clone.Birthday = person.Birthday
clone.FirstName = person.FirstName
clone.LastName = person.LastName
clone.EmailAddress = person.EmailAddress
````

What if you had hundreds of simple objects like the one above to copy? What if the object was more complex, contained references to other complex objects or collections of other complex objects? 

Would you still map it manually!?
With UltraMapper you can solve this problem efficiently like this:

````c#
Mapper ultraMapper = new Mapper();
Person clone = ultraMapper.Map<Person>( person );
````

Getting started
--------------------------------

Check out the [wiki](https://github.com/maurosampietro/UltraMapper/wiki/Getting-started) for more information and advanced scenarios

Why should I use UltraMapper instead of well-known alternatives?
--------------------------------

The answer is ReferenceTracking, Reliability, Performance and Maintainability.

The ReferenceTracking mechanism of UltraMapper guarantees that the cloned or mapped object **preserve the same reference structure of the source object**: if an instance is referenced twice in the source object, we will create only one new instance for the target, and assign it twice.

This is something theorically simple but crucial, yet **uncommon among mappers**; in facts other mappers tipically will create new instances on the target even if the same instance is being referenced twice in the source.

With UltraMapper, any reference object is cached and before creating any new reference a cache lookup is performed to check if that instance has already been mapped. If the reference has already been mapped, the mapped instance is used.   

This technique allows self-references anywhere down the hierarchical tree of the objects involved in the mapping process, avoids StackOverflows and **guarantees that the target object is actually a deep copy or a mapped version of the source and not just a similar object with identical values.**

ReferenceTracking mechanism is so important that cannot be disabled and offers a huge performance boost in real-world scenarios. 

UltraMapper is just ~1100 lines of code and generates and compiles minimal mapping expressions.
MappingExpressionBuilders are very well structured in a simple object-oriented way.


Key features
--------------------------------

Implemented features:

- Powerful reference tracking mechanism
- Powerful type-to-type, type-to-member and member-to-member configuration override mechanism, with configuration inheritance
- Supports self-references and circular references, anywhere down the object hierarchy
- Supports object inheritance
- Supports abstract classes
- Supports interfaces 
- Supports mapping by convention
- Supports flattening/projections by convention
- Supports manual flattening/unflattening/projections.
- Supports arrays
- Supports collections (Dictionary, HashSet, List, LinkedList, ObservableCollection, SortedSet, Stack, Queue)
- Supports collection merging/updating

Moreover UltraMapper is:
- very fast in any scenario (faster than any other .NET mapper i tried). See the [benchmarks](https://github.com/maurosampietro/UltraMapper/wiki/Benchmarks).
- developer-friendly (should be easy to contribute, extend and maintain)

**ANY FEEDBACK IS WELCOME**

