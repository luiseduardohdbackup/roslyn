' Copyright (c) Microsoft Open Technologies, Inc.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports System.Collections.Immutable
Imports System.Collections.ObjectModel
Imports System.Globalization
Imports System.IO
Imports System.Runtime.Serialization.Formatters.Binary
Imports Microsoft.CodeAnalysis.VisualBasic
Imports Roslyn.Test.Utilities

Namespace Microsoft.CodeAnalysis.VisualBasic.UnitTests
    Public Class VisualBasicCompilationOptionsTests
        Inherits BasicTestBase

        Private Sub TestProperty(Of T)(factory As Func(Of VBCompilationOptions, T, VBCompilationOptions),
                                       getter As Func(Of VBCompilationOptions, T),
                                       validNonDefaultValue As T)

            Dim oldOpt1 = New VBCompilationOptions(OutputKind.ConsoleApplication)

            Dim validDefaultValue = getter(oldOpt1)

            '  we need non-default value to test Equals And GetHashCode
            Assert.NotEqual(validNonDefaultValue, validDefaultValue)

            ' check that the assigned value can be read
            Dim newOpt1 = factory(oldOpt1, validNonDefaultValue)
            Assert.Equal(validNonDefaultValue, getter(newOpt1))
            Assert.Equal(0, newOpt1.Errors.Length)

            'check that creating new options with the same value yields the same options instance
            Dim newOpt1_alias = factory(newOpt1, validNonDefaultValue)
            Assert.Same(newOpt1_alias, newOpt1)

            ' check that Equals And GetHashCode work
            Dim newOpt2 = factory(oldOpt1, validNonDefaultValue)
            Assert.False(newOpt1.Equals(oldOpt1))
            Assert.True(newOpt1.Equals(newOpt2))

            Assert.Equal(newOpt1.GetHashCode(), newOpt2.GetHashCode())

            ' test Nothing:
            Assert.NotNull(factory(oldOpt1, Nothing))
        End Sub

        <Fact>
        Public Sub Invariants()
            TestProperty(Function(old, value) old.WithOutputKind(value), Function(opt) opt.OutputKind, OutputKind.DynamicallyLinkedLibrary)
            TestProperty(Function(old, value) old.WithModuleName(value), Function(opt) opt.ModuleName, "foo.dll")
            TestProperty(Function(old, value) old.WithMainTypeName(value), Function(opt) opt.MainTypeName, "Foo.Bar")
            TestProperty(Function(old, value) old.WithScriptClassName(value), Function(opt) opt.ScriptClassName, "<Script>")

            TestProperty(Function(old, value) old.WithGlobalImports(value), Function(opt) opt.GlobalImports,
                ImmutableArray.Create(GlobalImport.Parse("Foo.Bar"), GlobalImport.Parse("Baz")))

            TestProperty(Function(old, value) old.WithRootNamespace(value), Function(opt) opt.RootNamespace, "A.B.C")
            TestProperty(Function(old, value) old.WithOptionStrict(value), Function(opt) opt.OptionStrict, OptionStrict.On)
            TestProperty(Function(old, value) old.WithOptionInfer(value), Function(opt) opt.OptionInfer, False)
            TestProperty(Function(old, value) old.WithOptionExplicit(value), Function(opt) opt.OptionExplicit, False)
            TestProperty(Function(old, value) old.WithOptionCompareText(value), Function(opt) opt.OptionCompareText, True)

            TestProperty(Function(old, value) old.WithParseOptions(value), Function(opt) opt.ParseOptions,
                         New VBParseOptions(kind:=SourceCodeKind.Interactive))

            TestProperty(Function(old, value) old.WithEmbedVbCoreRuntime(value), Function(opt) opt.EmbedVbCoreRuntime, True)
            TestProperty(Function(old, value) old.WithOptimizationLevel(value), Function(opt) opt.OptimizationLevel, OptimizationLevel.Release)
            TestProperty(Function(old, value) old.WithOverflowChecks(value), Function(opt) opt.CheckOverflow, False)
            TestProperty(Function(old, value) old.WithCryptoKeyContainer(value), Function(opt) opt.CryptoKeyContainer, "foo")
            TestProperty(Function(old, value) old.WithCryptoKeyFile(value), Function(opt) opt.CryptoKeyFile, "foo")
            TestProperty(Function(old, value) old.WithDelaySign(value), Function(opt) opt.DelaySign, True)
            TestProperty(Function(old, value) old.WithBaseAddress(value), Function(opt) opt.BaseAddress, 100UL)
            TestProperty(Function(old, value) old.WithFileAlignment(value), Function(opt) opt.FileAlignment, 2048)
            TestProperty(Function(old, value) old.WithPlatform(value), Function(opt) opt.Platform, Platform.X64)
            TestProperty(Function(old, value) old.WithGeneralDiagnosticOption(value), Function(opt) opt.GeneralDiagnosticOption, ReportDiagnostic.Suppress)

            TestProperty(Function(old, value) old.WithSpecificDiagnosticOptions(value), Function(opt) opt.SpecificDiagnosticOptions,
                New Dictionary(Of String, ReportDiagnostic) From {{"VB0001", ReportDiagnostic.Error}}.ToImmutableDictionary())

            TestProperty(Function(old, value) old.WithHighEntropyVirtualAddressSpace(value), Function(opt) opt.HighEntropyVirtualAddressSpace, True)
            TestProperty(Function(old, value) old.WithSubsystemVersion(value), Function(opt) opt.SubsystemVersion, SubsystemVersion.Windows2000)
            TestProperty(Function(old, value) old.WithConcurrentBuild(value), Function(opt) opt.ConcurrentBuild, False)

            TestProperty(Function(old, value) old.WithXmlReferenceResolver(value), Function(opt) opt.XmlReferenceResolver, New XmlFileResolver(Nothing))
            TestProperty(Function(old, value) old.WithSourceReferenceResolver(value), Function(opt) opt.SourceReferenceResolver, New SourceFileResolver(ImmutableArray(Of String).Empty, Nothing))
            TestProperty(Function(old, value) old.WithMetadataReferenceResolver(value), Function(opt) opt.MetadataReferenceResolver, New MetadataFileReferenceResolver({}, Nothing))
            TestProperty(Function(old, value) old.WithMetadataReferenceProvider(value), Function(opt) opt.MetadataReferenceProvider, New MetadataFileReferenceProvider())
            TestProperty(Function(old, value) old.WithAssemblyIdentityComparer(value), Function(opt) opt.AssemblyIdentityComparer, New DesktopAssemblyIdentityComparer(New AssemblyPortabilityPolicy()))
            TestProperty(Function(old, value) old.WithStrongNameProvider(value), Function(opt) opt.StrongNameProvider, New DesktopStrongNameProvider())
        End Sub

        Sub WithXxx()
            AssertTheseDiagnostics(New VBCompilationOptions(OutputKind.ConsoleApplication).WithScriptClassName(Nothing).Errors,
<expected>
BC2014: the value 'Nothing' is invalid for option 'ScriptClassName'
</expected>)

            AssertTheseDiagnostics(New VBCompilationOptions(OutputKind.ConsoleApplication).WithScriptClassName("blah" & ChrW(0) & "foo").Errors,
<expected>
BC2014: the value '<%= "blah" & ChrW(0) & "foo" %>' is invalid for option 'ScriptClassName'
</expected>)

            AssertTheseDiagnostics(New VBCompilationOptions(OutputKind.ConsoleApplication).WithScriptClassName("").Errors,
<expected>
BC2014: the value '' is invalid for option 'ScriptClassName'
</expected>)

            Assert.True(New VBCompilationOptions(OutputKind.ConsoleApplication).WithMainTypeName(Nothing).Errors.IsEmpty)
            AssertTheseDiagnostics(New VBCompilationOptions(OutputKind.ConsoleApplication).WithMainTypeName("blah" & ChrW(0) & "foo").Errors,
<expected>
BC2014: the value '<%= "blah" & ChrW(0) & "foo" %>' is invalid for option 'MainTypeName'
</expected>)

            AssertTheseDiagnostics(New VBCompilationOptions(OutputKind.ConsoleApplication).WithMainTypeName("").Errors,
<expected>
BC2014: the value '' is invalid for option 'MainTypeName'
</expected>)

            Assert.True(New VBCompilationOptions(OutputKind.ConsoleApplication).WithRootNamespace(Nothing).Errors.IsEmpty)
            AssertTheseDiagnostics(New VBCompilationOptions(OutputKind.ConsoleApplication).WithRootNamespace("blah" & ChrW(0) & "foo").Errors,
<expected>
BC2014: the value '<%= "blah" & ChrW(0) & "foo" %>' is invalid for option 'RootNamespace'
</expected>)

            Assert.True(New VBCompilationOptions(OutputKind.ConsoleApplication).WithRootNamespace("").Errors.IsEmpty)

            Assert.Equal(0, New VBCompilationOptions(OutputKind.ConsoleApplication).WithGlobalImports(GlobalImport.Parse("Foo.Bar")).WithGlobalImports(DirectCast(Nothing, IEnumerable(Of GlobalImport))).GlobalImports.Count)
            Assert.Equal(0, New VBCompilationOptions(OutputKind.ConsoleApplication).WithGlobalImports(GlobalImport.Parse("Foo.Bar")).WithGlobalImports(DirectCast(Nothing, GlobalImport())).GlobalImports.Count)

            AssertTheseDiagnostics(New VBCompilationOptions(OutputKind.ConsoleApplication).WithOutputKind(CType(Int32.MaxValue, OutputKind)).Errors,
<expected>
BC2014: the value '<%= Int32.MaxValue %>' is invalid for option 'OutputKind'
</expected>)

            AssertTheseDiagnostics(New VBCompilationOptions(OutputKind.ConsoleApplication).WithOutputKind(CType(Int32.MinValue, OutputKind)).Errors,
<expected>
BC2014: the value '<%= Int32.MinValue %>' is invalid for option 'OutputKind'
</expected>)

            AssertTheseDiagnostics(New VBCompilationOptions(OutputKind.ConsoleApplication).WithOptimizationLevel(CType(Int32.MaxValue, OptimizationLevel)).Errors,
<expected>
BC2014: the value '<%= Int32.MaxValue %>' is invalid for option 'DebugInformationKind'
</expected>)

            AssertTheseDiagnostics(New VBCompilationOptions(OutputKind.ConsoleApplication).WithOptimizationLevel(CType(Int32.MinValue, OptimizationLevel)).Errors,
<expected>
BC2014: the value '<%= Int32.MinValue %>' is invalid for option 'DebugInformationKind'
</expected>)

            AssertTheseDiagnostics(New VBCompilationOptions(OutputKind.ConsoleApplication).WithOptionStrict(CType(3, OptionStrict)).Errors,
<expected>
BC2014: the value '3' is invalid for option 'OptionStrict'
</expected>)

            AssertTheseDiagnostics(New VBCompilationOptions(OutputKind.ConsoleApplication).WithFileAlignment(513).Errors,
<expected>
BC2014: the value '513' is invalid for option 'FileAlignment'
</expected>)

            AssertTheseDiagnostics(New VBCompilationOptions(OutputKind.ConsoleApplication).WithPlatform(CType(Int32.MaxValue, Platform)).Errors,
<expected>
BC2014: the value '<%= Int32.MaxValue %>' is invalid for option 'Platform'
</expected>)

            AssertTheseDiagnostics(New VBCompilationOptions(OutputKind.ConsoleApplication).WithPlatform(CType(Int32.MinValue, Platform)).Errors,
<expected>
BC2014: the value '<%= Int32.MinValue %>' is invalid for option 'Platform'
</expected>)

            Assert.Equal(Nothing, TestOptions.ReleaseDll.WithModuleName("foo").WithModuleName(Nothing).ModuleName)
            AssertTheseDiagnostics(TestOptions.ReleaseDll.WithModuleName("").Errors,
<expected>
BC37206: Name cannot be empty.
Parameter name: ModuleName
</expected>)

            AssertTheseDiagnostics(TestOptions.ReleaseDll.WithModuleName("a\0a").Errors,
<expected>
BC37206: Name contains invalid characters.
Parameter name: ModuleName
</expected>)

            AssertTheseDiagnostics(TestOptions.ReleaseDll.WithModuleName("a\uD800b").Errors,
<expected>
BC37206: Name contains invalid characters.
Parameter name: ModuleName
</expected>)

            AssertTheseDiagnostics(TestOptions.ReleaseDll.WithModuleName("a\\b").Errors,
<expected>
BC37206: Name contains invalid characters.
Parameter name: ModuleName
</expected>)

            AssertTheseDiagnostics(TestOptions.ReleaseDll.WithModuleName("a/b").Errors,
<expected>
BC37206: Name contains invalid characters.
Parameter name: ModuleName
</expected>)

            AssertTheseDiagnostics(TestOptions.ReleaseDll.WithModuleName("a:b").Errors,
<expected>
BC37206: Name contains invalid characters.
Parameter name: ModuleName
</expected>)
        End Sub

        <Fact>
        Public Sub ConstructorValidation()
            AssertTheseDiagnostics(New VBCompilationOptions(OutputKind.ConsoleApplication, scriptClassName:=Nothing).Errors,
<expected>
BC2014: the value 'Nothing' is invalid for option 'ScriptClassName'
</expected>)

            AssertTheseDiagnostics(New VBCompilationOptions(OutputKind.ConsoleApplication, scriptClassName:="blah" & ChrW(0) & "foo").Errors,
<expected>
BC2014: the value '<%= "blah" & ChrW(0) & "foo" %>' is invalid for option 'ScriptClassName'
</expected>)

            AssertTheseDiagnostics(New VBCompilationOptions(OutputKind.ConsoleApplication, scriptClassName:="").Errors,
<expected>
BC2014: the value '' is invalid for option 'ScriptClassName'
</expected>)


            Assert.True(New VBCompilationOptions(OutputKind.ConsoleApplication, mainTypeName:=Nothing).Errors.IsEmpty)

            AssertTheseDiagnostics(New VBCompilationOptions(OutputKind.ConsoleApplication, mainTypeName:=("blah" & ChrW(0) & "foo")).Errors,
<expected>
BC2014: the value '<%= "blah" & ChrW(0) & "foo" %>' is invalid for option 'MainTypeName'
</expected>)

            AssertTheseDiagnostics(New VBCompilationOptions(OutputKind.ConsoleApplication, mainTypeName:="").Errors,
<expected>
BC2014: the value '' is invalid for option 'MainTypeName'
</expected>)


            Assert.True(New VBCompilationOptions(OutputKind.ConsoleApplication, rootNamespace:=Nothing).Errors.IsEmpty)

            AssertTheseDiagnostics(New VBCompilationOptions(OutputKind.ConsoleApplication, rootNamespace:=("blah" & ChrW(0) & "foo")).Errors,
<expected>
BC2014: the value '<%= "blah" & ChrW(0) & "foo" %>' is invalid for option 'RootNamespace'
</expected>)

            Assert.True(New VBCompilationOptions(OutputKind.ConsoleApplication, rootNamespace:="").Errors.IsEmpty)

            AssertTheseDiagnostics(New VBCompilationOptions(outputKind:=CType(Int32.MaxValue, OutputKind)).Errors,
<expected>
BC2014: the value '<%= Int32.MaxValue %>' is invalid for option 'OutputKind'
</expected>)

            AssertTheseDiagnostics(New VBCompilationOptions(outputKind:=CType(Int32.MinValue, OutputKind)).Errors,
<expected>
BC2014: the value '<%= Int32.MinValue %>' is invalid for option 'OutputKind'
</expected>)


            AssertTheseDiagnostics(New VBCompilationOptions(OutputKind.ConsoleApplication, optimizationLevel:=CType(Int32.MaxValue, OptimizationLevel)).Errors,
<expected>
BC2014: the value '<%= Int32.MaxValue %>' is invalid for option 'OptimizationLevel'
</expected>)

            AssertTheseDiagnostics(New VBCompilationOptions(OutputKind.ConsoleApplication, optimizationLevel:=CType(Int32.MinValue, OptimizationLevel)).Errors,
<expected>
BC2014: the value '<%= Int32.MinValue %>' is invalid for option 'OptimizationLevel'
</expected>)

            AssertTheseDiagnostics(New VBCompilationOptions(OutputKind.ConsoleApplication, optionStrict:=CType(3, OptionStrict)).Errors,
<expected>
BC2014: the value '3' is invalid for option 'OptionStrict'
</expected>)

            AssertTheseDiagnostics(New VBCompilationOptions(OutputKind.ConsoleApplication, fileAlignment:=513).Errors,
<expected>
BC2014: the value '513' is invalid for option 'FileAlignment'
</expected>)

            AssertTheseDiagnostics(New VBCompilationOptions(OutputKind.ConsoleApplication, platform:=CType(Int32.MaxValue, Platform)).Errors,
<expected>
BC2014: the value '<%= Int32.MaxValue %>' is invalid for option 'Platform'
</expected>)

            AssertTheseDiagnostics(New VBCompilationOptions(OutputKind.ConsoleApplication, platform:=CType(Int32.MinValue, Platform)).Errors,
<expected>
BC2014: the value '<%= Int32.MinValue %>' is invalid for option 'Platform'
</expected>)
        End Sub

        ' Make sure the given root namespace is good and parses as expected
        Private Sub CheckRootNamespaceIsGood(rootNs As String, rootNsArray As String())
            Dim options = New VBCompilationOptions(OutputKind.ConsoleApplication).WithRootNamespace(rootNs)

            Assert.Equal(options.RootNamespace, rootNs)
            Assert.True(options.Errors.IsEmpty)
        End Sub

        ' Make sure the given root namespace is bad, the correct error is generated, and
        ' we have an empty root namespace as a result.
        Private Sub CheckRootNamespaceIsBad(rootNs As String)
            If rootNs Is Nothing Then
                Assert.True(New VBCompilationOptions(OutputKind.ConsoleApplication).WithRootNamespace(rootNs).Errors.IsEmpty)
            Else
                AssertTheseDiagnostics(New VBCompilationOptions(OutputKind.ConsoleApplication).WithRootNamespace(rootNs).Errors,
<expected>
BC2014: the value '<%= rootNs %>' is invalid for option 'RootNamespace'
</expected>)
            End If
        End Sub

        <Fact>
        Public Sub TestRootNamespace()
            CheckRootNamespaceIsGood("", {})
            CheckRootNamespaceIsGood("Foo", {"Foo"})
            CheckRootNamespaceIsGood("Foo.Bar", {"Foo", "Bar"})
            CheckRootNamespaceIsGood("Foo.Bar.q9", {"Foo", "Bar", "q9"})

            CheckRootNamespaceIsBad(Nothing)
            CheckRootNamespaceIsBad(" ")
            CheckRootNamespaceIsBad(".")
            CheckRootNamespaceIsBad("Foo.")
            CheckRootNamespaceIsBad("Foo. Bar")
            CheckRootNamespaceIsBad(".Foo")
            CheckRootNamespaceIsBad("X.7Y")
            CheckRootNamespaceIsBad("#")
            CheckRootNamespaceIsBad("A.$B")
        End Sub

        Private Sub CheckImportsAreGood(importStrings As String())
            Dim opt = New VBCompilationOptions(OutputKind.ConsoleApplication).WithGlobalImports(GlobalImport.Parse(importStrings))

            Assert.Equal(importStrings.Length, opt.GlobalImports.Count)
            For i = 0 To importStrings.Length - 1
                Assert.Equal(importStrings(i).Trim(), opt.GlobalImports(i).Clause.ToString)
            Next
        End Sub

        Private Sub CheckImportsAreBad(importStrings As String(), expectedErrors As String())
            Assert.Throws(Of ArgumentException)(Function() GlobalImport.Parse(importStrings))

            Dim diagnostics As ImmutableArray(Of Diagnostic) = Nothing
            Dim globalImports = GlobalImport.Parse(importStrings, diagnostics)

            Assert.Equal(0, globalImports.Count)

            Assert.NotNull(diagnostics)
            Assert.NotEmpty(diagnostics)

            Dim errorTexts = (From e In diagnostics Let text = e.GetMessage(CultureInfo.GetCultureInfo("en")) Order By text Select text).ToArray()
            Dim expectedTexts = (From e In expectedErrors Order By e Select e).ToArray()

            For i = 0 To diagnostics.Length - 1
                Assert.Equal(expectedTexts(i), errorTexts(i))
            Next
        End Sub

        <Fact>
        Public Sub TestImports()
            CheckImportsAreGood({})
            CheckImportsAreGood({"A.B", "G.F(Of G)", "Q", "A = G.X"})

            CheckImportsAreBad({"A.B.435",
                               "Global.Foo"},
                                {"Error in project-level import 'A.B.435' at '.435' : End of statement expected.",
                                "Error in project-level import 'Global.Foo' at 'Global' : 'Global' not allowed in this context; identifier expected."})
        End Sub

        <Fact>
        Public Sub TestGlobalOptionsParseReturnsNonNullDiagnostics()
            Dim diagnostics As ImmutableArray(Of Diagnostic) = Nothing
            Dim globalImports = GlobalImport.Parse({"System"}, diagnostics)

            Assert.Equal(1, globalImports.Count())
            Assert.NotNull(diagnostics)
            Assert.Empty(diagnostics)
        End Sub

        <Fact>
        Public Sub WarningTest()
            Assert.Equal(0, New VBCompilationOptions(OutputKind.ConsoleApplication).WithSpecificDiagnosticOptions(Nothing).SpecificDiagnosticOptions.Count)

            Dim source =
                <compilation name="WarningTest">
                    <file name="a.vb">
Module Program
    Sub Main(args As String())
        Dim x As Integer
        Dim y As Integer
        Const z As Long = 0
    End Sub

    Function foo()
    End Function
End Module
                    </file>
                </compilation>

            ' Baseline
            Dim commonoption = New VBCompilationOptions(OutputKind.ConsoleApplication)
            Dim comp = CreateCompilationWithMscorlibAndVBRuntime(source, commonoption)
            comp.VerifyDiagnostics(
                Diagnostic(ERRID.WRN_UnusedLocal, "x").WithArguments("x"),
                Diagnostic(ERRID.WRN_UnusedLocal, "y").WithArguments("y"),
                Diagnostic(ERRID.WRN_UnusedLocalConst, "z").WithArguments("z"),
                Diagnostic(ERRID.WRN_DefAsgNoRetValFuncRef1, "End Function").WithArguments("foo"))

            ' Suppress All
            ' vbc a.vb /nowarn
            Dim options = commonoption.WithGeneralDiagnosticOption(ReportDiagnostic.Suppress)
            comp = CreateCompilationWithMscorlibAndVBRuntime(source, options)
            comp.VerifyDiagnostics()

            ' Suppress 42024
            ' vbc a.vb /nowarn:42024
            Dim warnings As IDictionary(Of String, ReportDiagnostic) = New Dictionary(Of String, ReportDiagnostic)()
            warnings.Add(MessageProvider.Instance.GetIdForErrorCode(42024), ReportDiagnostic.Suppress)
            options = commonoption.WithSpecificDiagnosticOptions(New ReadOnlyDictionary(Of String, ReportDiagnostic)(warnings))
            comp = CreateCompilationWithMscorlibAndVBRuntime(source, options)
            comp.VerifyDiagnostics(
                Diagnostic(ERRID.WRN_UnusedLocalConst, "z").WithArguments("z"),
                Diagnostic(ERRID.WRN_DefAsgNoRetValFuncRef1, "End Function").WithArguments("foo"))

            ' Suppress 42024, 42099
            ' vbc a.vb /nowarn:42024,42099
            warnings = New Dictionary(Of String, ReportDiagnostic)()
            warnings.Add(MessageProvider.Instance.GetIdForErrorCode(42024), ReportDiagnostic.Suppress)
            warnings.Add(MessageProvider.Instance.GetIdForErrorCode(42099), ReportDiagnostic.Suppress)
            options = commonoption.WithSpecificDiagnosticOptions(New ReadOnlyDictionary(Of String, ReportDiagnostic)(warnings))
            comp = CreateCompilationWithMscorlibAndVBRuntime(source, options)
            comp.VerifyDiagnostics(Diagnostic(ERRID.WRN_DefAsgNoRetValFuncRef1, "End Function").WithArguments("foo"))

            ' Treat All as Errors
            ' vbc a.vb /warnaserror
            options = commonoption.WithGeneralDiagnosticOption(ReportDiagnostic.Error)
            comp = CreateCompilationWithMscorlibAndVBRuntime(source, options)
            comp.VerifyDiagnostics(
                Diagnostic(ERRID.WRN_UnusedLocal, "x").WithArguments("x").WithWarningAsError(True),
                Diagnostic(ERRID.WRN_UnusedLocal, "y").WithArguments("y").WithWarningAsError(True),
                Diagnostic(ERRID.WRN_UnusedLocalConst, "z").WithArguments("z").WithWarningAsError(True),
                Diagnostic(ERRID.WRN_DefAsgNoRetValFuncRef1, "End Function").WithArguments("foo").WithWarningAsError(True))

            ' Treat 42105 as Error
            ' vbc a.vb /warnaserror:42105
            warnings = New Dictionary(Of String, ReportDiagnostic)()
            warnings.Add(MessageProvider.Instance.GetIdForErrorCode(42105), ReportDiagnostic.Error)
            options = commonoption.WithSpecificDiagnosticOptions(New ReadOnlyDictionary(Of String, ReportDiagnostic)(warnings))
            comp = CreateCompilationWithMscorlibAndVBRuntime(source, options)
            comp.VerifyDiagnostics(
                Diagnostic(ERRID.WRN_UnusedLocal, "x").WithArguments("x"),
                Diagnostic(ERRID.WRN_UnusedLocal, "y").WithArguments("y"),
                Diagnostic(ERRID.WRN_UnusedLocalConst, "z").WithArguments("z"),
                Diagnostic(ERRID.WRN_DefAsgNoRetValFuncRef1, "End Function").WithArguments("foo").WithWarningAsError(True))

            ' Treat 42105 and 42099 as Errors
            ' vbc a.vb /warnaserror:42105,42099
            warnings = New Dictionary(Of String, ReportDiagnostic)()
            warnings.Add(MessageProvider.Instance.GetIdForErrorCode(42105), ReportDiagnostic.Error)
            warnings.Add(MessageProvider.Instance.GetIdForErrorCode(42099), ReportDiagnostic.Error)
            options = commonoption.WithSpecificDiagnosticOptions(New ReadOnlyDictionary(Of String, ReportDiagnostic)(warnings))
            comp = CreateCompilationWithMscorlibAndVBRuntime(source, options)
            comp.VerifyDiagnostics(
                Diagnostic(ERRID.WRN_UnusedLocal, "x").WithArguments("x"),
                Diagnostic(ERRID.WRN_UnusedLocal, "y").WithArguments("y"),
                Diagnostic(ERRID.WRN_UnusedLocalConst, "z").WithArguments("z").WithWarningAsError(True),
                Diagnostic(ERRID.WRN_DefAsgNoRetValFuncRef1, "End Function").WithArguments("foo").WithWarningAsError(True))

            ' Treat All as Errors but Suppress 42024
            ' vbc a.vb /warnaserror /nowarn:42024
            warnings = New Dictionary(Of String, ReportDiagnostic)()
            warnings.Add(MessageProvider.Instance.GetIdForErrorCode(42024), ReportDiagnostic.Suppress)
            options = commonoption.WithSpecificDiagnosticOptions(New ReadOnlyDictionary(Of String, ReportDiagnostic)(warnings)).WithGeneralDiagnosticOption(ReportDiagnostic.Error)
            comp = CreateCompilationWithMscorlibAndVBRuntime(source, options)
            comp.VerifyDiagnostics(
                Diagnostic(ERRID.WRN_UnusedLocalConst, "z").WithArguments("z").WithWarningAsError(True),
                Diagnostic(ERRID.WRN_DefAsgNoRetValFuncRef1, "End Function").WithArguments("foo").WithWarningAsError(True))

            ' Suppress All with treaing 42024 as an error, which will be ignored
            ' vbc a.vb /warnaserror:42024 /nowarn or
            ' vbc a.vb /nowarn /warnaserror
            warnings = New Dictionary(Of String, ReportDiagnostic)()
            warnings.Add(MessageProvider.Instance.GetIdForErrorCode(42024), ReportDiagnostic.Error)
            options = commonoption.WithSpecificDiagnosticOptions(New ReadOnlyDictionary(Of String, ReportDiagnostic)(warnings)).WithGeneralDiagnosticOption(ReportDiagnostic.Suppress)
            comp = CreateCompilationWithMscorlibAndVBRuntime(source, options)
            comp.VerifyDiagnostics()

        End Sub

        <Fact, WorkItem(529809, "DevDiv")>
        Public Sub NetModuleWithVbCore()
            Dim options As New VBCompilationOptions(OutputKind.NetModule, embedVbCoreRuntime:=True)

            Assert.Equal(2042, options.Errors.Single().Code)

            AssertTheseDiagnostics(CreateCompilationWithMscorlibAndVBRuntime(<compilation><file/></compilation>, options),
                                   <expected>
BC2042: The options /vbruntime* and /target:module cannot be combined.
                                   </expected>)
        End Sub

        ''' <summary>
        ''' If this test fails, please update the <see cref="CompilationOptions.GetHashCode" />
        ''' And <see cref="CompilationOptions.Equals" /> methods to
        ''' make sure they are doing the right thing with your New field And then update the baseline
        ''' here.
        ''' </summary>
        <Fact>
        Public Sub TestFieldsForEqualsAndGetHashCode()
            ReflectionAssert.AssertPublicAndInternalFieldsAndProperties(
                (GetType(VBCompilationOptions)),
                "GlobalImports",
                "RootNamespace",
                "OptionStrict",
                "OptionInfer",
                "OptionExplicit",
                "OptionCompareText",
                "EmbedVbCoreRuntime",
                "ParseOptions")
        End Sub

        <Fact>
        Public Sub Serializability1()
            VerifySerializability(New VBSerializableCompilationOptions(New VBCompilationOptions(
                outputKind:=OutputKind.WindowsApplication,
                generalDiagnosticOption:=ReportDiagnostic.Hidden,
                specificDiagnosticOptions:={KeyValuePair.Create("VB0001", ReportDiagnostic.Suppress)},
                subsystemVersion:=SubsystemVersion.Windows2000,
                globalImports:={GlobalImport.Parse("Foo.Bar")})))
        End Sub

        <Fact>
        Public Sub Serializability2()
            VerifySerializability(New VBSerializableCompilationOptions(New VBCompilationOptions(
                outputKind:=OutputKind.WindowsApplication,
                parseOptions:=New VBParseOptions(
                    languageVersion:=LanguageVersion.Experimental,
                    documentationMode:=DocumentationMode.Diagnose,
                    preprocessorSymbols:={KeyValuePair.Create(Of String, Object)("s", 1), KeyValuePair.Create(Of String, Object)("t", 2)}))))
        End Sub

        <Fact>
        Public Sub Serializability3()
            Dim parseOptions = New VBParseOptions(
                languageVersion:=LanguageVersion.VisualBasic10,
                documentationMode:=DocumentationMode.Diagnose,
                kind:=SourceCodeKind.Regular,
                preprocessorSymbols:=ImmutableArray.Create(New KeyValuePair(Of String, Object)("key", "Value")))

            Dim compilationOptions = New VBCompilationOptions(
                OutputKind.ConsoleApplication,
                globalImports:={GlobalImport.Parse("Foo.Bar")},
                rootNamespace:="Alpha.Beta",
                optionStrict:=OptionStrict.Custom,
                optionInfer:=False,
                optionExplicit:=False,
                optionCompareText:=True,
                embedVbCoreRuntime:=True,
                parseOptions:=parseOptions)

            Dim stream = New MemoryStream()
            Dim formatter = New BinaryFormatter()

            formatter.Serialize(stream, New VBSerializableCompilationOptions(compilationOptions))
            stream.Position = 0

            Dim deserializedCompilationOptions = DirectCast(formatter.Deserialize(stream), VBSerializableCompilationOptions).Options

            Assert.Equal(compilationOptions.GlobalImports.First().Name,
                         deserializedCompilationOptions.GlobalImports.First().Name)
            Assert.Equal(compilationOptions.RootNamespace,
                         deserializedCompilationOptions.RootNamespace)
            Assert.Equal(compilationOptions.OptionStrict,
                         deserializedCompilationOptions.OptionStrict)
            Assert.Equal(compilationOptions.OptionInfer,
                         deserializedCompilationOptions.OptionInfer)
            Assert.Equal(compilationOptions.OptionExplicit,
                         deserializedCompilationOptions.OptionExplicit)
            Assert.Equal(compilationOptions.OptionCompareText,
                         deserializedCompilationOptions.OptionCompareText)
            Assert.Equal(compilationOptions.EmbedVbCoreRuntime,
                         deserializedCompilationOptions.EmbedVbCoreRuntime)
        End Sub

    End Class
End Namespace