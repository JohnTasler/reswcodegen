using System.Reflection;
using Microsoft.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ChristianHelle.DeveloperTools.CodeGenerators.Resw.CustomTool.Tests;

[TestClass]
[DeploymentItem("Resources/Resources.resw")]
public sealed class CSharpCodeGeneratorTests : CodeGeneratorTestsBase
{
    #region A static field (to avoid repeating the same work for each test)
    private static readonly StaticData s_staticData = new();
    #endregion

    public CSharpCodeGeneratorTests()
        : base(s_staticData, TypeAttributes.Public, new CSharpCodeProvider())
    {
    }

    [TestMethod]
    public void GeneratedCodeIsAPublicClass()
    {
        CompileGeneratedCode();

        Assert.Contains("public sealed partial class", this.Actual);
        Assert.IsFalse(this.GeneratedType.IsNested);
        Assert.IsTrue(this.GeneratedType.IsPublic);
        Assert.IsTrue(this.GeneratedType.IsSealed);
        Assert.IsTrue(this.GeneratedType.IsClass);
    }

    [TestMethod]
    public void GeneratedCodeContainsPropertiesDefinedInResources()
    {
        CompileGeneratedCode();

        var resourceItems = this.Target.ResourceParser.Parse();

        foreach (var item in resourceItems)
        {
            var name = item.Name.Replace(".", "_");
            var nameProperty = $"public static string {name}";
            Assert.Contains(nameProperty, this.Actual);
            Assert.IsTrue(this.GeneratedType.GetProperty(name, BindingFlags.Public | BindingFlags.Static) != null);

            var propertyInfo = this.GeneratedType.GetProperty(name, BindingFlags.Public | BindingFlags.Static);
            Assert.IsTrue(propertyInfo != null);
            Assert.IsTrue(propertyInfo.PropertyType == typeof(string));
        }
    }

    [TestMethod]
    public void GeneratedCodeReplacesDottedKeysWithForwardSlash()
    {
        var resourceItems = this.Target.ResourceParser.Parse();

        foreach (var item in resourceItems)
        {
            var name = $"GetString(\"{item.Name.Replace(".", "/")}\")";
            Assert.Contains(name, this.Actual);
        }
    }

    [TestMethod]
    public void GeneratedCodePropertiesContainsCommentsSimilarToValuesDefinedInResources()
    {
        var resourceItems = this.Target.ResourceParser.Parse();

        foreach (var item in resourceItems)
            Assert.Contains("Localized resource similar to \"" + item.Value + "\"", this.Actual);
    }

    [TestMethod]
    public void ClassNameEqualsFileNameWithoutExtension()
    {
        Assert.Contains("class Resources", this.Actual);
    }

    [TestMethod]
    public void ResourceLoaderInitializedWithClassName()
    {
        Assert.Contains("ResourceLoader.GetForCurrentView(currentAssemblyName + \"/Resources\");", this.Actual);
    }
}
