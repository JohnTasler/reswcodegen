using System.CodeDom.Compiler;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ChristianHelle.DeveloperTools.CodeGenerators.Resw.CustomTool.Tests
{
    [TestClass]
    [DeploymentItem("Resources/Resources.resw")]
    public class CSharpCodeGeneratorTestsInternal : CodeGeneratorTestsBase
    {
        public CSharpCodeGeneratorTestsInternal()
            : base(TypeAttributes.NestedAssembly)
        {

        }

        [TestMethod]
        public void GeneratedCodeCompilesCleanly()
        {
            CompileGeneratedCode();

            Assert.IsFalse(CompilerResults.Errors.HasErrors, string.Join("\n", CompilerResults.Output.OfType<string>()));
            Assert.IsFalse(CompilerResults.Errors.HasWarnings, string.Join("\n", CompilerResults.Output.OfType<string>()));
            Assert.IsNotNull(GeneratedType);
        }

        [TestMethod]
        public void GenerateCodeDoesNotReturnNull()
        {
            Assert.IsNotNull(Actual);
        }

        [TestMethod]
        public void GeneratedCodeIsAnInternalClass()
        {
            CompileGeneratedCode();

            Assert.Contains("internal sealed partial class", Actual);
            Assert.IsFalse(GeneratedType.IsNested);
            Assert.IsTrue(GeneratedType.IsNotPublic);
            Assert.IsTrue(GeneratedType.IsSealed);
            Assert.IsTrue(GeneratedType.IsClass);
        }

        [TestMethod]
        public void GeneratedCodeContainsPropertiesDefinedInResources()
        {
            CompileGeneratedCode();

            var resourceItems = Target.ResourceParser.Parse();

            foreach (var item in resourceItems)
            {
                var name = item.Name.Replace(".", "_");
                var nameProperty = $"public static string {name}";
                Assert.Contains(nameProperty, Actual);

                var propertyInfo = GeneratedType.GetProperty(name, BindingFlags.Public | BindingFlags.Static);
                Assert.IsTrue(propertyInfo != null);
                Assert.IsTrue(propertyInfo.PropertyType == typeof(string));
            }
        }

        [TestMethod]
        public void GeneratedCodePropertiesContainsCommentsSimilarToValuesDefinedInResources()
        {
            var resourceItems = Target.ResourceParser.Parse();

            foreach (var item in resourceItems.Where(item => !item.Name.Contains(".")))
                Assert.Contains("Localized resource similar to \"" + item.Value + "\"", Actual);
        }

        [TestMethod]
        public void ClassNameEqualsFileNameWithoutExtension()
        {
            Assert.Contains("class Resources", Actual);
        }

        [TestMethod]
        public void ResourceLoaderInitializedWithClassName()
        {
            Assert.Contains("ResourceLoader.GetForCurrentView(currentAssemblyName + \"/Resources\");", Actual);
        }

        [TestMethod]
        public void ContainsProjectUrl()
        {
            Assert.Contains("http://bit.ly/reswcodegen", Actual);
        }
    }
}
