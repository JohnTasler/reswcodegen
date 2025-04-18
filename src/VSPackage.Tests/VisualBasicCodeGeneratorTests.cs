using System.CodeDom.Compiler;
using System;
using System.Reflection;
using Microsoft.VisualBasic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ICodeGenerator = ChristianHelle.DeveloperTools.CodeGenerators.Resw.VSPackage.CustomTool.ICodeGenerator;

namespace ChristianHelle.DeveloperTools.CodeGenerators.Resw.CustomTool.Tests;

[TestClass]
[DeploymentItem("Resources/Resources.resw")]
public sealed class VisualBasicCodeGeneratorTests : CodeGeneratorTestsBase
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

    public VisualBasicCodeGeneratorTests()
        : base(TypeAttributes.Public, new VBCodeProvider())
    {
    }

    [TestMethod]
    public void GeneratedCodeIsPublicClass()
    {
        CompileGeneratedCode();

        Assert.Contains("Partial Public NotInheritable Class", Actual);
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
            var nameProperty = $"Public Shared ReadOnly Property {name}() As String";
            Assert.Contains(nameProperty, Actual);
            Assert.IsNotNull(GeneratedType.GetProperty(name, BindingFlags.Public | BindingFlags.Static));

            var propertyInfo = GeneratedType.GetProperty(name, BindingFlags.Public | BindingFlags.Static);
            Assert.IsNotNull(propertyInfo);
            Assert.IsTrue(propertyInfo.PropertyType == typeof(string));
        }
    }

    [TestMethod]
    public void GeneratedCodeReplacesDottedKeysWithForwardSlash()
    {
        var resourceItems = Target.ResourceParser.Parse();

        foreach (var item in resourceItems)
        {
            var value = $"GetString(\"{item.Name.Replace(".", "/")}\")";
            Assert.Contains(value, Actual);
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
        Assert.Contains("Class Resources", Actual);
    }

    [TestMethod]
    public void ResourceLoaderInitializedWithClassName()
    {
        Assert.Contains("ResourceLoader.GetForCurrentView(currentAssemblyName + \"/Resources\")", Actual);
    }
}
