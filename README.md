# Snitch

A tool that help you find transitive package references that can be removed.

## Example

```
> snitch --tfm net462

Analyzing Foo.sln
Analyzing Foo (net462)...
Analyzing Bar (net462)...
Analyzing Baz (net462)...
Analyzing Qux (net462)...
Analyzing Zap (net462)...

╭──────────────────────────────────────────────────────────────╮
│  Packages that can be removed from Bar:                      │
│ ┌─────────────────────┬────────────────────────────────────┐ │
│ │ Package             │ Referenced by                      │ │
│ ├─────────────────────┼────────────────────────────────────┤ │
│ │ Autofac             │ Foo                                │ │
│ └─────────────────────┴────────────────────────────────────┘ │
│                                                              │
│  Packages that can be removed from Baz:                      │
│ ┌─────────────────────┬────────────────────────────────────┐ │
│ │ Package             │ Referenced by                      │ │
│ ├─────────────────────┼────────────────────────────────────┤ │
│ │ Autofac             │ Foo                                │ │
│ └─────────────────────┴────────────────────────────────────┘ │
│                                                              │
│  Packages that might be removed from Qux:                    │
│ ┌───────────┬───────────┬──────────────────────────────────┐ │
│ │ Package   │ Version   │ Reason                           │ │
│ ├───────────┼───────────┼──────────────────────────────────┤ │
│ │ Autofac   │ 4.9.3     │ Downgraded from 4.9.4 in Foo     │ │
│ └───────────┴───────────┴──────────────────────────────────┘ │
│                                                              │
│  Packages that might be removed from Zap:                    │
│ ┌─────────────────┬─────────┬──────────────────────────────┐ │
│ │ Package         │ Version │ Reason                       │ │
│ ├─────────────────┼─────────┼──────────────────────────────┤ │
│ │ Newtonsoft.Json │ 12.0.3  │ Updated from 12.0.1 in Foo   │ │
│ │ Autofac         │ 4.9.3   │ Downgraded from 4.9.4 in Foo │ │
│ └─────────────────┴─────────┴──────────────────────────────┘ │
╰──────────────────────────────────────────────────────────────╯
```

## Installation

```
> dotnet tool install -g snitch
```

## Usage

_Examine a specific project or solution using the first built 
target framework._

```
> snitch MyProject.csproj
```

_Examine a specific project using a specific
target framework moniker._

```
> snitch MyProject.csproj --tfm net462
```

_Examine a specific project using a specific target framework moniker
and return exit code 0 only if there was no transitive package collisions.
Useful for continuous integration._

```
> snitch MyProject.csproj --tfm net462 --strict
```

_Examine a specific project using a specific target framework moniker
and make sure that the packages Foo and Bar are excluded from the result._

```
> snitch MyProject.csproj --tfm net462 --exclude Foo --exclude Bar
```

_Examine a specific project using a specific target framework moniker
and exclude the project OtherProject from analysis._

```
> snitch MyProject.csproj --tfm net462 --skip OtherProject
```

## Building Snitch from source

```
> dotnet tool restore
> dotnet cake
```