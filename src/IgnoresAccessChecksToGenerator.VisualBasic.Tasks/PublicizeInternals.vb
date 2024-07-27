Imports System.IO
Imports Microsoft.Build.Framework
Imports Microsoft.Build.Utilities
Imports Mono.Cecil

Public Class PublicizeInternals
	Inherits Task

	Private ReadOnly _resolver As New AssemblyResolver

	<Required>
	Public Property SourceReferences As ITaskItem()

	<Required>
	Public Property AssemblyNames As ITaskItem()

	<Required>
	Public Property IntermediateOutputPath As String

	<Required>
	Public Property GeneratedCodeFilePath() As String

	Public Property ExcludeTypeNames() As ITaskItem()

	Public Overrides Function Execute() As Boolean
		If SourceReferences Is Nothing Then
			Throw New ArgumentNullException(NameOf(SourceReferences))
		End If

		Dim assemblyNameSet As New HashSet(Of String)(AssemblyNames.Select(Function(t) t.ItemSpec), StringComparer.OrdinalIgnoreCase)

		If assemblyNameSet.Count = 0 Then
			Return True
		End If

		Dim excludedTypeNames As HashSet(Of String)
		If ExcludeTypeNames IsNot Nothing Then
			excludedTypeNames = New HashSet(Of String)(ExcludeTypeNames.Select(Function(t) t.ItemSpec), StringComparer.OrdinalIgnoreCase)
		Else
			excludedTypeNames = New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
		End If

		Dim targetPath = IntermediateOutputPath
		Directory.CreateDirectory(targetPath)

		GenerateAttributes(targetPath, assemblyNameSet)

		For Each assemblyPath In SourceReferences.Select(Function(a) Path.GetDirectoryName(GetFullFilePath(targetPath, a.ItemSpec)))
			_resolver.AddSearchDirectory(assemblyPath)
		Next

		For Each assembly In SourceReferences
			Dim assemblyPath = GetFullFilePath(targetPath, assembly.ItemSpec)
			Dim assemblyName = Path.GetFileNameWithoutExtension(assemblyPath)
			If assemblyNameSet.Contains(assemblyName) Then
				Dim targetAssemblyPath = Path.Combine(targetPath, Path.GetFileName(assemblyPath))

				CreatePublicAssembly(assemblyPath, targetAssemblyPath, excludedTypeNames)
				Log.LogMessageFromText("Created publicized assembly at " & targetAssemblyPath, MessageImportance.Normal)
			End If
		Next assembly

		Return True
	End Function

	Private Sub GenerateAttributes(path As String, asmNames As IEnumerable(Of String))
		Dim attributes = String.Join(Environment.NewLine, asmNames.Select(Function(a) $"<Assembly: System.Runtime.CompilerServices.IgnoresAccessChecksTo(""{a}"")>"))

		Dim content = attributes & "
Namespace Global.System.Runtime.CompilerServices
	<AttributeUsage(AttributeTargets.Assembly, AllowMultiple := True)>
	Friend NotInheritable Class IgnoresAccessChecksToAttribute
		Inherits Attribute

		Sub New(assemblyName As String)
		End Sub
	End Class
End Namespace
"
		File.WriteAllText(GeneratedCodeFilePath, content)

		Log.LogMessageFromText("Generated IgnoresAccessChecksTo attributes", MessageImportance.Low)
	End Sub

	Private Sub CreatePublicAssembly(source As String, target As String, excludedTypeNames As HashSet(Of String))
		Dim assembly = AssemblyDefinition.ReadAssembly(source, New ReaderParameters With {.AssemblyResolver = _resolver})

		For Each [module] In assembly.Modules
			For Each type In [module].GetTypes().Where(Function(type1) Not excludedTypeNames.Contains(type1.FullName))

				If Not type.IsNested AndAlso type.IsNotPublic Then
					type.IsPublic = True
				ElseIf type.IsNestedAssembly OrElse type.IsNestedFamilyOrAssembly OrElse type.IsNestedFamilyAndAssembly Then
					type.IsNestedPublic = True
				End If

				For Each field In type.Fields
					If field.IsAssembly OrElse field.IsFamilyOrAssembly OrElse field.IsFamilyAndAssembly Then
						field.IsPublic = True
					End If
				Next field

				For Each method In type.Methods
					If method.IsAssembly OrElse method.IsFamilyOrAssembly OrElse method.IsFamilyAndAssembly Then
						method.IsPublic = True
					End If
				Next method
			Next type
		Next [module]

		assembly.Write(target)
	End Sub

	Private Function GetFullFilePath(basePath As String, path As String) As String
		Return If(System.IO.Path.IsPathRooted(path), System.IO.Path.GetFullPath(path), System.IO.Path.Combine(basePath, path))
	End Function

	Private Class AssemblyResolver
		Implements IAssemblyResolver

		Private ReadOnly _directories As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)

		Public Sub AddSearchDirectory(directory As String)
			_directories.Add(directory)
		End Sub

		Public Function Resolve(name As AssemblyNameReference) As AssemblyDefinition Implements IAssemblyResolver.Resolve
			Return Resolve(name, New ReaderParameters)
		End Function

		Public Function Resolve(name As AssemblyNameReference, parameters As ReaderParameters) As AssemblyDefinition Implements IAssemblyResolver.Resolve
			Dim assembly = SearchDirectory(name, _directories, parameters)
			If assembly IsNot Nothing Then
				Return assembly
			End If

			Throw New AssemblyResolutionException(name)
		End Function

		Public Sub Dispose() Implements IDisposable.Dispose
		End Sub

		Private Function SearchDirectory(name As AssemblyNameReference, directories As IEnumerable(Of String), parameters As ReaderParameters) As AssemblyDefinition
			Dim extensions = If(name.IsWindowsRuntime, {".winmd", ".dll"}, {".exe", ".dll"})
			For Each directory In directories
				For Each extension In extensions
					Dim file = Path.Combine(directory, name.Name & extension)
					If Not System.IO.File.Exists(file) Then
						Continue For
					End If
					Try
						Return GetAssembly(file, parameters)
					Catch e1 As BadImageFormatException
					End Try
				Next extension
			Next directory

			Return Nothing
		End Function

		Private Function GetAssembly(file As String, parameters As ReaderParameters) As AssemblyDefinition
			If parameters.AssemblyResolver Is Nothing Then
				parameters.AssemblyResolver = Me
			End If

			Return ModuleDefinition.ReadModule(file, parameters).Assembly
		End Function
	End Class
End Class
