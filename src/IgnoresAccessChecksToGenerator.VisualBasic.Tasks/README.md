# IgnoresAccessChecksTo VB Source Generator (MSBuild)

The `IgnoresAccessChecksToAttribute` is the reverse of the `InternalsVisibleToAttribute` - it allows an assembly to declare assemblies whose internals would be visible to it. The attribute class isn't declared in the BCL but is recognized by the CLR (Desktop >= 4.6 and Core), i.e. you can declare it in your code and it would work.

Since there's currently no compiler support for this attribute, this package can be used as a workaround. It **generates reference assemblies where all the internal types & members become public**, and adds a VB file with the attribute and its instances.

## Usage

Just add the package and define `InternalsAssemblyName` items with the assemblies you need access to.

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <ItemGroup>
    <InternalsAssemblyName Include="AssemblyToGrantAccessTo1" />
    <InternalsAssemblyName Include="AssemblyToGrantAccessTo2" />
  </ItemGroup>

  <ItemGroup>
    <PackageReference Include="Nukepayload2.SourceGenerators.IgnoresAccessChecksTo" Version="*" PrivateAssets="All" />
  </ItemGroup>

</Project>
```

By default, the build tasks replaces all method bodies with `Throw New NullReferenceException`. To keep the original bodies, you can specify:

```xml
  <PropertyGroup>
    <InternalsAssemblyUseEmptyMethodBodies>false</InternalsAssemblyUseEmptyMethodBodies>
  </PropertyGroup>
```
