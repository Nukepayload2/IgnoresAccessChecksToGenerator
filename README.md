# IgnoresAccessChecksTo Generator (MSBuild)

[![NuGet](https://img.shields.io/nuget/v/IgnoresAccessChecksToGenerator.svg?style=flat-square)](https://www.nuget.org/packages/IgnoresAccessChecksToGenerator)

The `IgnoresAccessChecksToAttribute` is the reverse of the `InternalsVisibleToAttribute` - it allows an assembly to declare assemblies whose internals would be visible to it. The attribute class isn't declared in the BCL but is recognized by the CLR (Desktop >= 4.6 and Core), i.e. you can declare it in your code and it would work.

Since there's currently no compiler support for this attribute (I've [submitted a PR](https://github.com/dotnet/roslyn/pull/20870) to Roslyn), this package can be used as a workaround. It **generates reference assemblies where all the internal types & members become public**, and adds a C# file with the attribute and its instances.

## Usage

Just add the package and define the `InternalsAssemblyNames` property with a semicolon-delimited list of assemblies you need access to.

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <ItemGroup>
    <InternalsAssemblyName Include="AssemblyToGrantAccessTo1" />
    <InternalsAssemblyName Include="AssemblyToGrantAccessTo2" />
  </ItemGroup>

  <ItemGroup>
    <PackageReference Include="IgnoresAccessChecksToGenerator" Version="0.6.0" PrivateAssets="All" />
  </ItemGroup>

</Project>
```

By default, the build tasks replaces all method bodies with `throw null;`. To keep the original bodies, you can specify:

```xml
  <PropertyGroup>
    <InternalsAssemblyUseEmptyMethodBodies>false</InternalsAssemblyUseEmptyMethodBodies>
  </PropertyGroup>
```
