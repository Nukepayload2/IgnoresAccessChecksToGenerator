# IgnoresAccessChecksTo VB Source Generator (MSBuild)

[![NuGet](https://img.shields.io/nuget/v/Nukepayload2.SourceGenerators.IgnoresAccessChecksTo.svg?style=flat-square)](https://www.nuget.org/packages/Nukepayload2.SourceGenerators.IgnoresAccessChecksTo)

Generates `IgnoresAccessChecksTo` attributes in VB projects and reference assemblies to allow compile-time access to `Friend` members. 
We recommend using `PrivateAssets="all"` in the package reference to prevent affecting other projects.

Since there's currently no compiler support for this attribute, this package can be used as a workaround. It **generates reference assemblies where all the internal types & members become public**, and adds a VB file with the attribute and its instances.

## Usage

Just add the package and define `InternalsAssemblyName` items with the assemblies you need access to.

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <ItemGroup>
    <InternalsAssemblyName Include="AssemblyToGrantAccessTo1" />
    <InternalsAssemblyName Include="AssemblyToGrantAccessTo2" />
    <InternalsAssemblyExcludeTypeName Include="Namespace.TypeName" />
  </ItemGroup>

  <ItemGroup>
    <PackageReference Include="Nukepayload2.SourceGenerators.IgnoresAccessChecksTo" Version="*" PrivateAssets="All" />
  </ItemGroup>

</Project>
```
