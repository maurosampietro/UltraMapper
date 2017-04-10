# UltraMapper
A nicely coded object-mapper for .NET


What is UltraMapper?
--------------------------------

UltraMapper is a tool that allows you to map one object of type T to another object of type V.

It avoids you the need to manually write the (boring) code that instantiate/assign all the members of the object.


Getting started
--------------------------------

https://github.com/maurosampietro/UltraMapper/wiki/Getting-started


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
