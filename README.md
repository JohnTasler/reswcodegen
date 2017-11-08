# ResW File Code Generator
A Visual Studio Custom Tool for generating a strongly typed helper class for accessing localized resources from a .ResW file.

**Features**

- Define custom namespace for the generated file
- Auto-updating of generated code file when changes are made to the .ResW Resource file
- XML documentation style comments like "Localized resource similar to '[the value]'"


**Custom Tools**

- ReswFileCodeGenerator - Generates a public class
- InternalReswFileCodeGenerator - Generates an internal (C#) / friend (VB) class

**Supported Languages**

- C#
- Visual Basic

**Screenshots**

![alt text](http://www.codeplex.com/Download?ProjectName=reswcodegen&DownloadId=527605)
- Visual Studio Custom Tool (C#)

![alt text](http://www.codeplex.com/Download?ProjectName=reswcodegen&DownloadId=528704)
- Visual Studio Custom Tool (VB)

![alt text](http://www.codeplex.com/Download?ProjectName=reswcodegen&DownloadId=527606)
- Resource File Contents



**C# Usage**

    string test1, test2, test3;

    void LoadLocalizedStrings()
    {
        test1 = App1.LocalizedResources.Resources.Test1;
        test2 = App1.LocalizedResources.Resources.Test2;
        test3 = App1.LocalizedResources.Resources.Test3;
    }


**VB Usage**

    Dim test1, test2, test3

    Private Sub LoadLocalizedStrings()
        test1 = AppVb.LocalizedStrings.Resources.Test1
        test2 = AppVb.LocalizedStrings.Resources.Test2
        test3 = AppVb.LocalizedStrings.Resources.Test3
    End Sub


**Generated C# Code**

    //------------------------------------------------------------------------------
    // <auto-generated>
    //     This code was generated by a tool.
    //     Runtime Version:4.0.30319.18010
    //
    //     Changes to this file may cause incorrect behavior and will be lost if
    //     the code is regenerated.
    // </auto-generated>
    //------------------------------------------------------------------------------

    // --------------------------------------------------------------------------------------------------
    // <auto-generatedInfo>
    // 	This code was generated by ResW File Code Generator (http://reswcodegen.codeplex.com)
    // 	ResW File Code Generator was written by Christian Resma Helle
    // 	and is under GNU General Public License version 2 (GPLv2)
    // 
    // 	This code contains a helper class exposing property representations
    // 	of the string resources defined in the specified .ResW file
    // 
    // 	Generated: 11/08/2012 22:41:22
    // </auto-generatedInfo>
    // --------------------------------------------------------------------------------------------------
    namespace App1.LocalizedResources
    {
        using Windows.ApplicationModel.Resources;

        public partial class Resources
        {

            private static ResourceLoader resourceLoader = new ResourceLoader();

            /// <summary>
            /// Localized resource similar to "Test 1 value"
            /// </summary>
            public static string Test1
            {
                get
                {
                    return resourceLoader.GetString("Test1");
                }
            }

            /// <summary>
            /// Localized resource similar to "Test 2 value"
            /// </summary>
            public static string Test2
            {
                get
                {
                    return resourceLoader.GetString("Test2");
                }
            }

            /// <summary>
            /// Localized resource similar to "Test 3 value"
            /// </summary>
            public static string Test3
            {
                get
                {
                    return resourceLoader.GetString("Test3");
                }
            }
        }
    }


**Generated Visual Basic Code**

    ' <auto-generated>
    '     This code was generated by a tool.
    '     Runtime Version:4.0.30319.18010
    '
    '     Changes to this file may cause incorrect behavior and will be lost if
    '     the code is regenerated.
    ' </auto-generated>
    '------------------------------------------------------------------------------

    Option Strict Off
    Option Explicit On

    Imports Windows.ApplicationModel.Resources

    '--------------------------------------------------------------------------------------------------
    '<auto-generatedInfo>
    '	This code was generated by ResW File Code Generator (http://reswcodegen.codeplex.com)
    '	ResW File Code Generator was written by Christian Resma Helle
    '	and is under GNU General Public License version 2 (GPLv2)
    '
    '	This code contains a helper class exposing property representations
    '	of the string resources defined in the specified .ResW file
    '
    '	Generated: 11/12/2012 21:30:52
    '</auto-generatedInfo>
    '--------------------------------------------------------------------------------------------------

    Namespace AppVb.LocalizedStrings

        Partial Public Class Resources

            Private Shared resourceLoader As ResourceLoader = New ResourceLoader()

            '''<summary>
            '''Localized resource similar to "Test 1 value"
            '''</summary>
            Public Shared ReadOnly Property Test1() As String
                Get
                    Return resourceLoader.GetString("Test1")
                End Get
            End Property

            '''<summary>
            '''Localized resource similar to "Test 2 value"
            '''</summary>
            Public Shared ReadOnly Property Test2() As String
                Get
                    Return resourceLoader.GetString("Test2")
                End Get
            End Property

            '''<summary>
            '''Localized resource similar to "Test 3 value"
            '''</summary>
            Public Shared ReadOnly Property Test3() As String
                Get
                    Return resourceLoader.GetString("Test3")
                End Get
            End Property
        End Class
    End Namespace

