![Project Icon](StyletIcon.png) Stylet
======================================

[![NuGet](https://img.shields.io/nuget/v/Stylet.svg)](https://www.nuget.org/packages/Stylet/)
[![Build status](https://ci.appveyor.com/api/projects/status/nqucthach0x6gkil?svg=true)](https://ci.appveyor.com/project/canton7/stylet)

Introduction
------------

Stylet is a small but powerful ViewModel-first MVVM framework for WPF, which allows you to write maintainable and extensible code in a way which is easy to test.
Stylet's aims to:

 - Solve the blockers, niggles, and annoyances which hamper MVVM development without a framework, using simple but powerful concepts.
 - Be obvious to people picking up your project for the first time: there's very little magic
 - Be easy to verify/validate. The LOC count is low, and it comes with a very comprehensive test suite. The code is well-written and well-documented.
 - Be flexible while providing sensible defaults. Almost any part of the framework can be overridden if you wish, but you probably won't want to.


It is inspired by [Caliburn.Micro](http://www.caliburnproject.org/), and shares many of its concepts, but removes most of the magic (replacing it with more powerful alternatives), and simplifies parts considerably by targeting only MVVM, WPF and .NET 4.5.


Documentation
-------------

[The wiki is the official documentation source](https://github.com/canton7/Stylet/wiki).
There's a lot of documentation there, and it's being added to all the time.
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

Or right-click your project -> Manage NuGet Packages -> Search for Stylet

The source is also available when you are debugging, using [GitLink](https://github.com/GitTools/GitLink).
Go to Debug -> Options and Settings -> General, and make the following changes:

 - Turn **off** "Enable Just My Code"
 - Turn **off** "Enable .NET Framework source stepping". Yes, it is misleading, but if you don't, then Visual Studio will ignore your custom server order and only use its own servers.
 - Turn **on** "Enable source server support". You may have to OK a security warning.

See also [GitLink troubleshooting](https://github.com/GitTools/GitLink#troubleshooting).


Contributing
------------

Contributions are always welcome.
If you've got a problem or a question, [raise an issue](https://github.com/canton7/Stylet/issues).
If you've got code you want to contribute, please read [the Contributing guidelines](https://github.com/canton7/Stylet/wiki/Contributing) first of all.
Create a feature branch off the `develop` branch, add your changes there, and submit it as a pull request.
