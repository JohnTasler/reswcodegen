using System.CodeDom.Compiler;
using System;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ICodeGenerator = ChristianHelle.DeveloperTools.CodeGenerators.Resw.VSPackage.CustomTool.ICodeGenerator;
using Microsoft.CSharp;

namespace ChristianHelle.DeveloperTools.CodeGenerators.Resw.CustomTool.Tests;

[TestClass]
[DeploymentItem("Resources/Resources.resw")]
public sealed class CSharpCodeGeneratorTests : CodeGeneratorTestsBase
{
    #region Static Fields (to avoid repeating the same work for each test)
    private static TypeAttributes? s_classAccessibility;
    private static CodeDomProvider s_provider;
    private static string s_reswFileContents;
    private static string s_actual;
    private static ICodeGenerator s_target;
    private static CompilerResults s_compilerResults;
    private static Type s_generatedType;
    #endregion

    #region Overridden properties to access the static fields from the base class
    protected override TypeAttributes? ClassAccessibility
    {
        get => s_classAccessibility;
        set => s_classAccessibility = value;
    }

    protected override CodeDomProvider Provider
    {
        get => s_provider;
        set => s_provider = value;
    }

    protected override string ReswFileContents
    {
        get => s_reswFileContents;
        set => s_reswFileContents = value;
    }

    protected override string Actual
    {
        get => s_actual;
        set => s_actual = value;
    }

    protected override ICodeGenerator Target
    {
        get => s_target;
        set => s_target = value;
    }

    protected override CompilerResults CompilerResults
    {
        get => s_compilerResults;
        set => s_compilerResults = value;
    }

    protected override Type GeneratedType
    {
        get => s_generatedType;
        set => s_generatedType = value;
    }
    #endregion

    public CSharpCodeGeneratorTests()
        : base(TypeAttributes.Public, new CSharpCodeProvider())
    {
    }

    [TestMethod]
    public void GeneratedCodeIsAPublicClass()
    {
        CompileGeneratedCode();

        Assert.Contains("public sealed partial class", s_actual);
        Assert.IsFalse(s_generatedType.IsNested);
        Assert.IsTrue(s_generatedType.IsPublic);
        Assert.IsTrue(s_generatedType.IsSealed);
        Assert.IsTrue(s_generatedType.IsClass);
    }

    [TestMethod]
    public void GeneratedCodeContainsPropertiesDefinedInResources()
    {
        CompileGeneratedCode();

        var resourceItems = s_target.ResourceParser.Parse();

        foreach (var item in resourceItems)
        {
            var name = item.Name.Replace(".", "_");
            var nameProperty = $"public static string {name}";
            Assert.Contains(nameProperty, s_actual);
            Assert.IsTrue(s_generatedType.GetProperty(name, BindingFlags.Public | BindingFlags.Static) != null);

            var propertyInfo = s_generatedType.GetProperty(name, BindingFlags.Public | BindingFlags.Static);
            Assert.IsTrue(propertyInfo != null);
            Assert.IsTrue(propertyInfo.PropertyType == typeof(string));
        }
    }

    [TestMethod]
    public void GeneratedCodeReplacesDottedKeysWithForwardSlash()
    {
        var resourceItems = s_target.ResourceParser.Parse();

        foreach (var item in resourceItems)
        {
            var name = $"GetString(\"{item.Name.Replace(".", "/")}\")";
            Assert.Contains(name, s_actual);
        }
    }

    [TestMethod]
    public void GeneratedCodePropertiesContainsCommentsSimilarToValuesDefinedInResources()
    {
        var resourceItems = s_target.ResourceParser.Parse();

        foreach (var item in resourceItems)
            Assert.Contains("Localized resource similar to \"" + item.Value + "\"", s_actual);
    }

    [TestMethod]
    public void ClassNameEqualsFileNameWithoutExtension()
    {
        Assert.Contains("class Resources", s_actual);
    }

    [TestMethod]
    public void ResourceLoaderInitializedWithClassName()
    {
        Assert.Contains("ResourceLoader.GetForCurrentView(currentAssemblyName + \"/Resources\");", s_actual);
    }
}
