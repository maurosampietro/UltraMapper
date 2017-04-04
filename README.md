# UltraMapper
A nicely coded object-mapper for .NET


What is UltraMapper?
--------------------------------

UltraMapper is a tool that allows you to map one object of type T to another object of type V.

It avoids you the need to manually write the (boring) code that instantiate/assign all the members of the object.


Why UltraMapper?
--------------------------------

UltraMapper was coded to solve some major problems of 'AutoMapper', the most famous mapper available at the time (early 2017).

I found 'AutoMapper' to be a great tool, as long as things remained simple but had nightmares as soon as I started facing the real world because of bugs (self-references^, reference tracking^, ignoring members), the complexity of its options, configuration and registration mechanism and the messy codebase (yes, i tried to fix some of the problems i found. Headaches, no luck).

I filed bugs on github, tried to solve them by myself and tried to work around them, but the tool at one point became the ruler of my architectual choices, and so I started to code UltraMapper.

^Automapper is subject to throw StackOverflow exceptions when the hierarchy of the object being mapped involves self-references anywhere down the tree. That's due to problems in the reference tracking.

Getting started
--------------------------------

```c#
var mapper = new UltraMapper();
mapper.Map( source, target );
```

```c#


var mapper = new UltraMapper();
mapper.Map( source, target );
```


More examples and documentation will be ready soon.


Key features
--------------------------------

**Please note that UltraMapper at the time is published solely in order to be reviewed by the community and receive feedbacks.
This early version is not considered stable, and may lack some of the feature you would expect from a mapper**

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
- just 1300 lines of code (more easily maintainable and understandable)
- developer-friendly (should be easy to contribute)

What's missing?
--------------------------------

- Flattening by convention
- Dynamic mapper
- Arrays, Multidimensional/Jagged arrays
- Proper documentation
- Examples
- Benchmarks
