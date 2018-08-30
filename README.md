
[![NuGet Status](http://img.shields.io/nuget/v/Stamp.Fody.svg?style=flat)](https://www.nuget.org/packages/Stamp.Fody/)

[![Build status](https://ci.appveyor.com/api/projects/status/aiih15lkmhy56dv2/branch/master?svg=true)](https://ci.appveyor.com/project/304NotModified/fody-stamp/branch/master)

![Icon](https://raw.github.com/304NotModified/Fody.Stamp/master/Icons/package_icon.png)


### This is an add-in for [Fody](https://github.com/Fody/Fody/) 

Stamps an assembly with git data.


## The nuget package

https://nuget.org/packages/Stamp.Fody/

    PM> Install-Package Stamp.Fody


## What it does 

Extracts the git information from disk, combines it with the assembly version, and places it in the `AssemblyInformationalVersionAttribute`.

So if your assembly version is 1.0.0.0, the working branch is "master" and the last commit is 759e9ddb53271dfa9335a3b27e452749a9b22280 then the following attribute will be added to the assembly.

```c#
[assembly: AssemblyInformationalVersion("1.0.0.0 Head:'master' Sha:759e9ddb53271dfa9335a3b27e452749a9b22280")]
```


## Templating the version

You can customize the string used in the `AssemblyInformationalVersionAttribute` by adding some tokens to the string, which Stamp will replace.

For example, if you add `[assembly: AssemblyInformationalVersion("%version% Branch=%branch%")]` then Stamp will change it to `[assembly: AssemblyInformationalVersion("1.0.0.0 Branch=master")]`

The tokens are:
- `%version%` is replaced with the version (1.0.0.0)
- `%version1%` is replaced with the major version only (1)
- `%version2%` is replaced with the major and minor version (1.0)
- `%version3%` is replaced with the major, minor, and revision version (1.0.0)
- `%version4%` is replaced with the major, minor, revision, and build version (1.0.0.0)
- `%githash%` is replaced with the SHA1 hash of the branch tip of the repository
- `%shorthash%` is replaced with the first eight characters of %githash%
- `%branch%` is replaced with the branch name of the repository
- `%haschanges%` is replaced with the string defined in the ChangeString attribute in the configuration, see below.

> NOTE: if you already have an AssemblyInformationalVersion attribute and it doesn't use replacement tokens, it will not be modified at all.


## Configuration

All config options are attributes of Stamp in FodyWeavers.xml


### ChangeString

Define the string used to indicate that the code was built from a non clean repository.

*Default is `HasChanges`*

```xml
<Fody ChangeString="New text" />
```


### UseProjectGit

Define if you want to start Stamp to start searching for the Git repository in the ProjectDir (true) or the SolutionDir (false).

*Default is `false`*

```xml
<Fody UseProjectGit='true' />
```


## Icon

<a href="http://thenounproject.com/noun/stamp/#icon-No8787" target="_blank">Stamp</a> designed by <a href="http://thenounproject.com/rohithdezinr" target="_blank">Rohith M S</a> from The Noun Project.
