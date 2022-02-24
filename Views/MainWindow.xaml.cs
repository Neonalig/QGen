#region Copyright (C) 2017-2022  Cody Bock
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License (Version 3.0)
// as published by the Free Software Foundation.
// 
// More information can be found here: https://www.gnu.org/licenses/gpl-3.0.en.html
#endregion

#region Using Directives

using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

using Ookii.Dialogs.Wpf;

using QGen.Core;
using QGen.Lib.Common;

#endregion

namespace QGen.Views;

/// <summary> Interaction logic for MainWindow.xaml </summary>
public partial class MainWindow {
    public MainWindow() {
        InitializeComponent();
        DataContext = this;

        //Simple replacements for '%UserProfile%/Desktop/template.cs' to '%UserProfile%/Desktop/destfile.cs'
        //TestOne();

        //Complex example of generating an executable assembly from a file and performing reflection on it.
        //TestTwo();

        //Close();
        //Environment.Exit(0);
    }

    public static Result<DirectoryInfo> ResolvePath( string RequestedRootFolder ) {
        VistaFolderBrowserDialog VFBD = new VistaFolderBrowserDialog {
            Description = $"Select the '{RequestedRootFolder}' path.",
            Multiselect = false,
            ShowNewFolderButton = false,
            UseDescriptionForTitle = true
        };

        return VFBD.ShowDialog() == true
            ? VFBD.SelectedPath.GetDirectory(true)
            : Result<DirectoryInfo>.UserCancelledDialog;
    }

    /// <summary>
    /// A dynamically loaded assembly.
    /// </summary>
    /// <param name="Path">The path to the assembly file.</param>
    /// <param name="Assembly">The loaded assembly.</param>
    /// <param name="GeneratorProviders">The collection of valid <see cref="IGeneratorProvider"/>s in the assembly.</param>
    [StructLayout(LayoutKind.Sequential)]
    record struct CachedAssembly( FileInfo Path, Result<Assembly> Assembly, Result<IEnumerable<CachedProvider>> GeneratorProviders );

    /// <summary>
    /// Loads the found assemblies in the folder, finding any relevant <see cref="IGeneratorProvider"/>s defined in the assembly at the same time.
    /// </summary>
    /// <param name="SearchDirectory">The directory to search in.</param>
    /// <param name="Wildcard">The wildcard used to find assemblies in the folder</param>
    /// <param name="SearchOption">The directory search method. (top-level only, or all children)</param>
    /// <returns>The collection of <see cref="CachedAssembly">CachedAssemblies</see> found and loaded.</returns>
    static IEnumerable<CachedAssembly> FindDynamicAssembliesAsync( DirectoryInfo SearchDirectory, string Wildcard = "QGenDynamic*.dll", SearchOption SearchOption = SearchOption.TopDirectoryOnly ) {
        foreach ( FileInfo AssemblyFile in SearchDirectory.GetFiles(Wildcard, SearchOption) ) {
            CachedAssembly Return;
            try {
                Assembly Ass = Assembly.LoadFile(AssemblyFile.FullName);
                Return = new CachedAssembly(AssemblyFile, Ass, GetGeneratorProviders(Ass).GetResult(true));
            } catch ( Exception Ex ) {
                Return = new CachedAssembly(AssemblyFile, Ex, Ex);
            }
            yield return Return;
        }
    }

    /// <summary>
    /// A dynamically constructed <see cref="IGeneratorProvider"/> retrieved from an assembly loaded in memory.
    /// </summary>
    /// <param name="ClassType">The type of the deriving class.</param>
    /// <param name="Provider">The constructed provider.</param>
    [StructLayout(LayoutKind.Sequential)]
    record struct CachedProvider( Type ClassType, Result<IGeneratorProvider> Provider );

    /// <summary>
    /// Finds the relevant <see cref="IGeneratorProvider"/> implementations in the given assembly.
    /// </summary>
    /// <param name="Ass">The assembly to search.</param>
    /// <returns>The collection of cached <see cref="IGeneratorProvider"/>s in the assembly.</returns>
    static IEnumerable<CachedProvider> GetGeneratorProviders( Assembly Ass ) {
        foreach ( Type Tp in Ass.GetTypes() ) {
            //Debug.WriteLine($"\t\t\tChecking '{Tp.FullName}'...");
            if ( Tp.IsAbstract/*|| Tp.IsGenericType*/ ) {
                //Debug.WriteLine("\t\t\t\tIgnore abstracts.");
                continue;
            }

            if ( typeof(IGeneratorProvider).IsAssignableFrom(Tp) ) {
                //Debug.WriteLine("\t\t\t\tDerives IGeneratorProvider!");
                if ( Tp.GetConstructor(Type.EmptyTypes) is { } Cto ) {
                    //Debug.WriteLine("\t\t\t\t\tFound constructor!");
                    yield return new CachedProvider(Tp, ((IGeneratorProvider)Cto.Invoke(null)).GetResult(true));
                } else {
                    //Debug.WriteLine("\t\t\t\t\tInvalid constructor.");
                    yield return new CachedProvider(Tp, Result<IGeneratorProvider>.MissingParameterlessConstructor());
                }
            }/* else {
                Debug.WriteLine("\t\t\t\tDoes not derive IGeneratorProvider!");
            }*/
        }
    }

    //static async void TestTwo() {
    //    try {
    //        await TestTwoAsync();
    //    } catch ( Exception Ex ) {
    //        Debug.WriteLine($"Caught: {Ex}", "EXCEPTION");
    //        throw;
    //    }
    //}

    #region TestOne

    readonly struct ExMethods : IMatchGenerator {
        /// <inheritdoc />
        public string Name => @"exMethods";

        /// <inheritdoc />
        public Result<string> Generate( Match Match, string Line ) {
            int Sp = 0;
            foreach ( char C in Line ) {
                switch ( C ) {
                    case ' ':
                        Sp++;
                        break;
                    case '\t':
                        Sp += 4;
                        break;
                }
            }

            //return "";
            return $"\n\n\tinternal int Spacing => {Sp};";
        }
    }


    readonly struct ExCond : IMatchGenerator {
        /// <inheritdoc />
        public string Name => @"exCond";

        /// <inheritdoc />
        public Result<string> Generate( Match Match, string Line ) =>
            //""
            ", \"RELEASE\""
        ;
    }


    static void TestOne() {
        FileInfo TestFile = new FileInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "template.cs"));
        FileInfo DestFile = new FileInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "destfile.cs"));

        IMatchGenerator[] Gens = { new ExCond(), new ExMethods() };

        string[] Lines = File.ReadAllLines(TestFile.FullName);
        string[] NewLines = SourceGenerator.Generate(Lines, Gens).ToArray();

        if ( DestFile.Exists ) {
            DestFile.Delete();
        }
        File.WriteAllLines(DestFile.FullName, NewLines);

        _ = Process.Start(new ProcessStartInfo("subl", $"\"{DestFile.FullName}\"") {
            UseShellExecute = true
        });
    }

    #endregion

    #region TestTwo

    static async Task TestTwoAsync(CancellationToken Token = new CancellationToken() ) {//DirectoryInfo CurDir = new DirectoryInfo(Environment.CurrentDirectory);
        DirectoryInfo CurDir = new DirectoryInfo(@"E:\Projects\Visual Studio\QGen\QGen.Sample\QGen.Sample.GeneratorSample\bin\Debug\net6.0");

        foreach ( (FileInfo Path, Result<Assembly> Assembly, Result<IEnumerable<CachedProvider>> Providers) in FindDynamicAssembliesAsync(CurDir, "QGen.Sample.GeneratorSample.dll") ) {
            if ( Assembly.Success ) {
                Debug.WriteLine($"Found assembly: '{Assembly.Value}' @ '{Path.Name}'");
                if ( Providers.Success ) {
                    foreach ( (Type ClassType, Result<IGeneratorProvider> Provider) in Providers.Value ) {
                        // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
                        if ( Provider.Success ) {
                            Debug.WriteLine($"\t\t>> Successfully constructed the provider: '{Provider.Value.GetType().FullName}' ({Provider.Value}).");

                            Result<DirectoryInfo> Dir = Provider.Value.ResolveRootFolder(ResolvePath);
                            if ( Dir.Success ) {

                                Result<IEnumerable<IFileGenerator>> Generators = await Provider.Value.GetGeneratorsAsync(Token);
                                if ( Generators.Success ) {
                                    Result GenRes = await ScriptGenerator.GenerateAsync(Dir.Value, Generators.Value, Token);
                                    if ( GenRes.Success ) {
                                        Debug.WriteLine("\t\t\t\t\tSource generators ran successfully!");
                                    } else {
                                        Debug.WriteLine($"\t\t\t\t\tSource generation failed with the result: '{Generators.Message}'.");
                                    }
                                } else {
                                    Debug.WriteLine($"\t\t\t\tSource generator retrieval failed with the result: '{Generators.Message}'.");
                                }
                            } else {
                                Debug.WriteLine($"\t\t\tRoot folder resolution failed with the result: '{Dir.Message}'.");
                            }
                        } else {
                            Debug.WriteLine($"\t\tParsing of type {ClassType.FullName} failed with the result: '{Provider.Message}'.");
                        }
                    }
                } else {
                    Debug.WriteLine($"\tProvider search failed with the result: '{Providers.Message}'.");
                }
            } else {
                Debug.WriteLine($"Assembly '{Path.Name}' failed to load with the result: '{Assembly.Message}'.");
            }
        }
    }

    #endregion

    bool _Running = false;
    async void Button_Click( object Sender, RoutedEventArgs E ) {
        Button Btn = (Button)Sender;
        if ( _Running ) { return; }
        _Running = true;
        Btn.IsEnabled = false;
        await TestTwoAsync();
        Btn.IsEnabled = true;
        _Running = false;
    }
}