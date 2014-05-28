Stylet
======

Introduction
------------

Stylet is a small but powerful ViewModel-first MVVM framework for WPF, which allows you to write maintainable and extensible code in a way which is easy to test.
Stylet's aims to:

 - Solve the blockers, niggles, and annoyances which hamper MVVM development without a framework using simple but powerful concepts.
 - Be obvious to people picking up your project for the first time: there's very little magic
 - Be easy to verify/validate. The LOC count is low, and it comes with a very comprehensive test suite. The code is well-written and well-documented.
 - Be flexible while providing sensible defaults. Almost any part of the framework can be overridden if you wish, but you probably won't want to.


It is inspired by [Caliburn.Micro](http://www.caliburnproject.org/), and shares many of its concepts, but removes most of the magic (replacing it with more powerful alternatives), and simplifies parts considerably by targeting only MVVM, WPF and .NET 4.5.


Documentation
-------------

[The wiki is the official documentation source](https://github.com/canton7/Stylet/wiki).
There's a lot of documentation there (it was longer than my dissertation last time I checked), and it's being added to all the time.
Go check it out!


Installation
------------

You can either grab Stylet through NuGet, or build it from source yourself.
Stylet does rely on .NET 4.5 (Visual Studio 2012 or higher).

### NuGet

[Stylet is available on NuGet](https://www.nuget.org/packages/Stylet).

Either open the package console and type:

```
PM> Install-Package Stylet
```

Or right-click your project -> Manage NuGet Packages... -> Online -> search for Stylet in the top right.

Don't forget to right-click your solution, and click "Enable NuGet package restore"!

I also publish symbols on [SymbolSource](http://www.symbolsource.org/Public), so you can use the NuGet package but still have access to Stylet's source when debugging. If you haven't yet set up Visual Studio to use SymbolSource, do that now:

In Visual Studio, go to Debug -> Options and Settings, and make the following changes:

 - Under General, turn **off** "Enable Just My Code"
 - Under General, turn **on** "Enable source server support". You may have to Ok a security warning.
 - Under Symbols, add "http://srv.symbolsource.org/pdb/Public" to the list. 

### Source

I maintain a subtree split of just the Stylet project, [called Stylet-Core](https://github.com/canton7/Stylet-Core).
Head over there, clone/download the repo, and add the .csproj to your solution.


Contributing
------------

Contributions are always welcome.
If you've got a problem or a question, [raise an issue](https://github.com/canton7/Stylet/issues).
If you've got code you want to contribute, create a feature branch off the `develop` branch, add your changes there, and submit it as a pull request.
