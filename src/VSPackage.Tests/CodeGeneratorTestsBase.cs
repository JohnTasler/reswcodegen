using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using ChristianHelle.DeveloperTools.CodeGenerators.Resw.VSPackage.CustomTool;
using Microsoft.VisualBasic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ChristianHelle.DeveloperTools.CodeGenerators.Resw.CustomTool.Tests;

using ICodeGenerator = VSPackage.CustomTool.ICodeGenerator;

public abstract class CodeGeneratorTestsBase
{
    protected const string FILE_PATH = "Resources.resw";

    protected abstract TypeAttributes? ClassAccessibility { get; set; }
    protected abstract CodeDomProvider Provider { get; set; }
    protected abstract string ReswFileContents { get; set; }
    protected abstract string Actual { get; set; }
    protected abstract ICodeGenerator Target { get; set; }
    protected abstract CompilerResults CompilerResults { get; set; }
    protected abstract Type GeneratedType { get; set; }

    protected CodeGeneratorTestsBase(TypeAttributes? classAccessibility = null, CodeDomProvider provider = null)
    {
        this.ClassAccessibility ??= classAccessibility;
        this.Provider ??= provider;

        this.ReswFileContents ??= File.ReadAllText(FILE_PATH);

        this.Target ??= new CodeGeneratorFactory().Create(FILE_PATH.Replace(".resw", string.Empty), "TestApp", ReswFileContents, provider, classAccessibility: classAccessibility);
        this.Actual ??= Target.GenerateCode();
    }


    [TestMethod]
    public void GenerateCodeDoesNotReturnNull()
    {
        Assert.IsNotNull(Actual);
    }

    [TestMethod]
    public void GeneratedCodeCompilesCleanly()
    {
        this.CompileGeneratedCode();

        Assert.IsFalse(CompilerResults.Errors.HasErrors, string.Join("\n", CompilerResults.Output.OfType<string>()));
        Assert.IsFalse(CompilerResults.Errors.HasWarnings, string.Join("\n", CompilerResults.Output.OfType<string>()));
        Assert.IsNotNull(GeneratedType);
    }

    [TestMethod]
    public void ContainsProjectUrl()
    {
        Assert.Contains("http://bit.ly/reswcodegen", Actual);
    }

    protected void CompileGeneratedCode()
    {
        if (this.CompilerResults is not null)
        {
            Assert.IsNotNull(this.GeneratedType);
            return;
        }

        Assert.IsNotNull(this.Target);
        Assert.IsNotNull(this.Target.Provider);

        // Invoke compilation.
        var compilerParameters = GetCompilerParameters(this.Target.Provider);
        CompilerResults = this.Target.Provider.CompileAssemblyFromDom(
            compilerParameters, this.Target.CodeCompileUnit, GenerateClassesInConflictingNamespaces());

        Debug.WriteLine($"Compiler returned {CompilerResults.NativeCompilerReturnValue}");
        Debug.WriteLine($"Output:\n{string.Join("\n", CompilerResults.Output.OfType<string>())}");

        Debug.WriteLine($"Environment.CurrentDirectory     ={Environment.CurrentDirectory}");
        Debug.WriteLine($"CompilerResults.PathToAssembly   ={this.CompilerResults.PathToAssembly}");
        Debug.WriteLine($"CompilerResults.TempFiles.TempDir={this.CompilerResults.TempFiles.TempDir}");
        Debug.WriteLine($"CompilerResults.TempFiles.Count  ={this.CompilerResults.TempFiles.Count}");
        Debug.WriteLine($"CompilerResults.TempFiles        ={string.Join(", ", this.CompilerResults.TempFiles.OfType<string>())}");
        Debug.WriteLine($"CompilerResults.Errors.Count     ={this.CompilerResults.Errors.Count}");

        Debug.WriteLine($"CompilerResults.CompiledAssembly.Location={this.CompilerResults.CompiledAssembly.Location}");
        this.GeneratedType = CompilerResults.CompiledAssembly.GetType("TestApp.Resources");
    }

    private static CodeCompileUnit GenerateClassesInConflictingNamespaces()
    {
        return new CodeCompileUnit
        {
            Namespaces =
            {
                new CodeNamespace("TestApp.Windows.Library")
                {
                    Types =
                    {
                        new CodeTypeDeclaration("Class1")
                        {
                            IsClass = true,
                            TypeAttributes = TypeAttributes.Sealed | TypeAttributes.Public,
                        }
                    }
                },
                new CodeNamespace("TestApp.System.Library")
                {
                    Types =
                    {
                        new CodeTypeDeclaration("Class2")
                        {
                            IsClass = true,
                            TypeAttributes = TypeAttributes.Sealed | TypeAttributes.Public,
                        }
                    }
                }
            }
        };
    }

    private static CompilerParameters GetCompilerParameters(CodeDomProvider provider)
    {
        var compilerOptions = new Dictionary<string, string>
        {
            { "lib"      , $"\"{Environment.ExpandEnvironmentVariables(@"%windir%\System32\WinMetadata")}\"" },
            { "optimize-", null },
        };

        // The VB compiler has slightly different names for some of its options
        if (provider.GetType() == typeof(VBCodeProvider))
        {
            compilerOptions["libpath"] = compilerOptions["lib"];
            compilerOptions.Remove("lib");
        }

        var compilerParameters = new CompilerParameters
        {
            // Generate a class library instead of an executable.
            GenerateExecutable = false,

            // Set the assembly file name to generate.
            // OutputAssembly = "GeneratedResources.dll",

            // Save the assembly as a non-file.
            GenerateInMemory = true,

            // Set the level at which the compiler
            // should start displaying warnings.
            WarningLevel = 3,

            // Set whether to treat all warnings as errors.
            TreatWarningsAsErrors = true,

            // Set a temporary files collection.
            // The TempFileCollection stores the temporary files
            // generated during a build in the current directory,
            // and does not delete them after compilation.
            TempFiles = new TempFileCollection(".", true),

            // Set additional library paths for finding referenced assemblies
            CompilerOptions = string.Join(" ", compilerOptions.Select(
                kv => string.IsNullOrWhiteSpace(kv.Value) ? $"-{kv.Key}" : $"-{kv.Key}:{kv.Value}")),

            // Assemblies referenced by the generated code
            ReferencedAssemblies =
            {
                "System.dll",
                "System.Runtime.dll",
                "Windows.ApplicationModel.winmd",
                "Windows.Foundation.winmd",
                "Windows.UI.winmd",
            },
        };

        return compilerParameters;
    }
}
