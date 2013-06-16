![Icon](https://raw.github.com/Fody/Stamp/master/Icons/package_icon.png)

## This is an add-in for [Fody](https://github.com/Fody/Fody/) 

Stamps an assembly with git data.

[Introduction to Fody](http://github.com/Fody/Fody/wiki/SampleUsage)

## Nuget package http://nuget.org/packages/Stamp.Fody 

## What it does 

Extracts the git information from disk, combines it with the assembly version, and places it in the `AssemblyInformationalVersionAttribute`.

So if your assembly version is 1.0.0.0, the working branch is "master" and the last commit is 759e9ddb53271dfa9335a3b27e452749a9b22280 then the following attribute will be added to the assembly.

    [assembly: AssemblyInformationalVersion("1.0.0.0 Head:'master' Sha:759e9ddb53271dfa9335a3b27e452749a9b22280")]

        
## Icon

Icon courtesy of [The Noun Project](http://thenounproject.com)
