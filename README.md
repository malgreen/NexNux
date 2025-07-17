# NexNux

A cross-platform mod manager built with [C#](https://docs.microsoft.com/en-us/dotnet/csharp/) and the [Avalonia](https://www.avaloniaui.net/) framework, using the [MVVM](https://docs.microsoft.com/en-us/xamarin/xamarin-forms/enterprise-application-patterns/mvvm) pattern.

## TODO

[] Use the json helper wherever we can...
[] Do we actually need this service -> repository pattern? I find it pretty needless
[] I think repos are fine for consolidating database/disk interactions, but maybe a service isn't needed, and we should just inject the repository directly?
[] We just move away from JSON-as-database, SQLite is much faster. SQLite can also be used for unit-testing by just creating in-memory database. Should be fine.
[] Main purpose for testing is stuff like the plugin reordering stuff in the mod repos, not so important for the actual game selection stuff.
[] Deployment stuff is hard to test

