# UltraMapper
A nicely coded object-mapper for .NET 

[![Build status](https://ci.appveyor.com/api/projects/status/github/UltraMapper)](https://ci.appveyor.com/project/maurosampietro/ultramapper/branch/master)
[![NuGet](http://img.shields.io/nuget/v/UltraMapper.svg)](https://www.nuget.org/packages/UltraMapper/)

What is UltraMapper?
--------------------------------

UltraMapper is a tool that allows you to map one object of type T to another object of type V.

It avoids you the need to manually write the (boring) code that instantiate/assign all the members of the object.
It can be used to get deep copies of an object.

Why should I use UltraMapper instead of known alternatives like AutoMapper, ExpressMapper or TinyMapper?
--------------------------------

The answer is ReferenceTracking, Reliability, Performance and Maintainability.

The ReferenceTracking mechanism of UltraMapper guarantees that the cloned or mapped object **preserve the same reference structure of the source object**: if an instance is referenced twice in the source object, we will create only one new instance for the target, and assign it twice.

This is something theorically simple but crucial, yet **uncommon among mappers**; in facts other mappers tipically will create new instances on the target even if the same instance is being referenced twice in the source.

With UltraMapper, any reference object is cached and before creating any new reference a cache lookup is performed to check if that instance has already been mapped. If the reference has already been mapped, the mapped instance is used.   

This technique allows self-references anywhere down the hierarchical tree of the objects involved in the mapping process, avoids StackOverflows and **guarantees that the target object is actually a deep copy or a mapped version of the source and not just a similar object with identical values.**

ReferenceTracking mechanism is so important that cannot be disabled and offers a huge performance boost in real-world scenarios. 

UltraMapper is just ~1100 lines of code and generates and compiles minimal mapping expressions.
MappingExpressionBuilders are very well structured in a simple object-oriented way.


Getting started
--------------------------------

https://github.com/maurosampietro/UltraMapper/wiki/Getting-started


Key features
--------------------------------

**Please note that UltraMapper at the time is published solely in order to be reviewed by the community and receive feedbacks.
This early version may lack some of the feature you would expect from a mapper**

Implemented features:

- Full reference tracking
- Supports self-references
- Supports object inheritance
- Supports mapping by convention
- Supports object flattening
- Supports manual flattening.
- Supports collections (Dictionary, HashSet, List, LinkedList, ObservableCollection, SortedSet, Stack, Queue)
- Support type/members configuration override mechanism

Moreover UltraMapper is:
- very fast in any scenario (faster than any other .NET mapper i tried, just let me know if otherwise)
- just ~1100 lines of code (more easily maintainable and understandable)
- developer-friendly (should be easy to contribute)

What's missing?
--------------------------------

- Flattening by convention
- Dynamic mapper
- Arrays, Multidimensional/Jagged arrays
- Proper documentation
- Examples
- Benchmarks
