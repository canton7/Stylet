StyletIoC IoC Container
=======================

Introduction
------------

StyletIoC is a very lightweight (~500 LOC), fully unit tested, very very fast IoC container.
It was written for use in medical software (where third-party libraries are a concern), but you might decide to use it for its simplicity, performance, or some of its nice features.


Quick Start
-----------

You can't create a container directly.
Instead, you create a builder, register bindings on that builder, then create an immutable container from that builder (the rationale behind this is description in a technical section later).

A simple example:

```csharp
// Some types... 
interface I1 { }
class C1 : I1 { }
class C2 { }

// Create a builder
var builder = new StyletIoCBuilder();

// Bind C1 to I1
builder.Bind<I1>().To<C1>();

// Bind C2 to itself, as a singleton
builder.Bind<C2>().ToSelf().InSingletonScope();

// Create an IContainer
var ioc = builder.BuildContainer();

// Right! Now we can start fetching things

// Fetches a new instance of C1
var c1 = ioc.Get<I1>();

// Fetches the singleton instance of C2
var c2 = ioc.Get<C2>();

// Error! We haven't bound C1 to itself
var error = ioc.Get<C1>();
```


Bindings in Detail
------------------


Technical / Behind the Scenes
-----------------------------
