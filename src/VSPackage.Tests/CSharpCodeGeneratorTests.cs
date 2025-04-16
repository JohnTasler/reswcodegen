﻿using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ChristianHelle.DeveloperTools.CodeGenerators.Resw.CustomTool.Tests
{
    [TestClass]
    [DeploymentItem("Resources/Resources.resw")]
    public class CSharpCodeGeneratorTests : CodeGeneratorTestsBase
    {
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
        public void GeneratedCodeIsAPublicClass()
        {
            CompileGeneratedCode();

            Assert.Contains("public sealed partial class", Actual);
            Assert.IsFalse(GeneratedType.IsNested);
            Assert.IsTrue(GeneratedType.IsPublic);
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
                Assert.IsTrue(GeneratedType.GetProperty(name, BindingFlags.Public | BindingFlags.Static) != null);

                var propertyInfo = GeneratedType.GetProperty(name, BindingFlags.Public | BindingFlags.Static);
                Assert.IsTrue(propertyInfo != null);
                Assert.IsTrue(propertyInfo.PropertyType == typeof(string));
            }
        }

        [TestMethod]
        public void GeneratedCodeReplacesDottedKeysWithForwardSlash()
        {
            var resourceItems = Target.ResourceParser.Parse();

            foreach (var item in resourceItems)
            {
                var name = $"GetString(\"{item.Name.Replace(".", "/")}\")";
                Assert.Contains(name, Actual);
            }
        }

        [TestMethod]
        public void GeneratedCodePropertiesContainsCommentsSimilarToValuesDefinedInResources()
        {
            var resourceItems = Target.ResourceParser.Parse();

            foreach (var item in resourceItems)
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
