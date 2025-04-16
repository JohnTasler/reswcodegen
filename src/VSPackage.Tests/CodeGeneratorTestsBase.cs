using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using ChristianHelle.DeveloperTools.CodeGenerators.Resw.VSPackage.CustomTool;
using Microsoft.VisualBasic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ChristianHelle.DeveloperTools.CodeGenerators.Resw.CustomTool.Tests
{
    using ICodeGenerator = VSPackage.CustomTool.ICodeGenerator;

    public abstract class CodeGeneratorTestsBase
    {
        protected const string FILE_PATH = "Resources.resw";
        protected string ReswFileContents;
        protected string Actual;
        protected ICodeGenerator Target;
        protected CompilerResults CompilerResults;
        protected Type GeneratedType;

        private TypeAttributes? classAccessibility;
        private CodeDomProvider provider;

        private static int s_ctorCount = 0;
        private static int s_initCount = 0;
        private static int s_compileCount = 0;

        public CodeGeneratorTestsBase(TypeAttributes? classAccessibility = null, CodeDomProvider provider = null)
        {
            Debug.WriteLine($"{nameof(CodeGeneratorTestsBase)} constructor count = {++s_ctorCount}");

            this.classAccessibility = classAccessibility;
            this.provider = provider;
        }

        [TestInitialize]
        public void Initialize()
        {
            Debug.WriteLine($"{nameof(CodeGeneratorTestsBase)}.{nameof(Initialize)} count = {++s_initCount}");

            ReswFileContents = File.ReadAllText(FILE_PATH);

            Target = new CodeGeneratorFactory().Create(FILE_PATH.Replace(".resw", string.Empty), "TestApp", ReswFileContents, provider, classAccessibility: classAccessibility);
            Actual = Target.GenerateCode();
        }

        protected void CompileGeneratedCode()
        {
            if (CompilerResults is not null)
            {
                Assert.IsNotNull(Target, nameof(Target));
                Assert.IsNotNull(GeneratedType, nameof(GeneratedType));
                return;
            }

            Debug.WriteLine($"{nameof(CodeGeneratorTestsBase)}.{nameof(CompileGeneratedCode)} count = {++s_compileCount}");

            // Invoke compilation.
            var compilerParameters = GetCompilerParameters(Target.Provider);
            CompilerResults = Target.Provider.CompileAssemblyFromDom(compilerParameters, Target.CodeCompileUnit);

            Debug.WriteLine($"Compiler returned {CompilerResults.NativeCompilerReturnValue}");
            Debug.WriteLine($"Output\n{string.Join("\n", CompilerResults.Output.OfType<string>())}");

            Debug.WriteLine($"Environment.CurrentDirectory     ={Environment.CurrentDirectory}");
            Debug.WriteLine($"CompilerResults.PathToAssembly   ={CompilerResults.PathToAssembly}");
            Debug.WriteLine($"CompilerResults.TempFiles.TempDir={CompilerResults.TempFiles.TempDir}");
            Debug.WriteLine($"CompilerResults.TempFiles.Count  ={CompilerResults.TempFiles.Count}");
            Debug.WriteLine($"CompilerResults.TempFiles        ={string.Join(", ", CompilerResults.TempFiles.OfType<string>())}");

            Debug.WriteLine($"CompilerResults.CompiledAssembly.Location={CompilerResults.CompiledAssembly.Location}");
            GeneratedType = CompilerResults.CompiledAssembly.GetType("TestApp.Resources");
        }

        private static CompilerParameters GetCompilerParameters(CodeDomProvider provider)
        {
            var compilerOptions = new Dictionary<string, string>
            {
                { "lib"      , $"\"{Environment.ExpandEnvironmentVariables(@"%windir%\System32\WinMetadata")}\"" },
                { "optimize-", null },
            };

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
}
