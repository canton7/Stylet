StyletIoC
=========

Introduction
------------

Blah blah


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


Documentation
-------------

[The wiki is the official documentation source](https://github.com/canton7/Stylet/wiki).
There's a lot of documentation there (it was longer than my dissertation last time I checked), and it's being added to all the time.
Go check it out!


Contributing
------------

Contributions are always welcome.
If you've got a problem or a question, [raise an issue](https://github.com/canton7/Stylet/issues).
If you've got code you want to contribute, create a feature branch off the `develop` branch, add your changes there, and submit it as a pull request.