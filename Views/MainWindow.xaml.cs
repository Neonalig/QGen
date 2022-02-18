#region Copyright (C) 2017-2022  Cody Bock
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License (Version 3.0)
// as published by the Free Software Foundation.
// 
// More information can be found here: https://www.gnu.org/licenses/gpl-3.0.en.html
#endregion

#region Using Directives

using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using QGen.Core;

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

    static async void TestTwo() {
        try {
            await TestTwoAsync();
        } catch ( Exception Ex ) {
            Debug.WriteLine($"Caught: {Ex}", "EXCEPTION");
            throw;
        }
    }

    #region TestOne

    readonly struct ExMethods : IMatchGenerator {
        /// <inheritdoc />
        public string Name => @"exMethods";

        /// <inheritdoc />
        public string Generate( Match Match, string Line ) {
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
        public string Generate( Match Match, string Line ) =>
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

    static async Task TestTwoAsync() {
        DirectoryInfo ReadDir = new DirectoryInfo("E:\\Projects\\Visual Studio\\QGen\\QGen.Sample");

        await ScriptGenerator.GenerateAsync(ReadDir, SearchOption.TopDirectoryOnly, new [] { new InputHelper_AG() }, new CancellationToken());
    }

    public class InputHelper_AG : IFileModifier {
        /// <inheritdoc />
        public string Name => "InputHelper-GenUtil";

        /// <inheritdoc />
        public Version Version { get; } = new Version(0, 1, 0);

        /// <inheritdoc />
        public string RequestedPath => "InputHelperAG";

        /// <inheritdoc />
        public async Task ReadAsync( FileInfo Path, SyntaxTree Tree, CompilationUnitSyntax Root, Out<IEnumerable<IMatchGenerator>> Generators, CancellationToken Token = default ) {
            Debug.WriteLine("Reading...");
            FileInfo KIFile = Path.Directory!.GetSubFile("KnownInput.cs");
            SyntaxTree KITree = await KIFile.GetSyntaxTreeAsync(Token);
            Debug.WriteLine("Got syntax tree.");
            SyntaxNode TreeRoot = await KITree.GetRootAsync(Token);

            bool Ready = false;
            List<string> Usings = new List<string>();
            List<(string Name, string AssetPath, string Type)> EnumMembers = new List<(string Name, string AssetPath, string Type)>();

            (SyntaxToken NameToken, SyntaxToken? AssetPathToken, SyntaxToken? TypeToken)? KIMatch = null;
            foreach ( SyntaxNode KINode in TreeRoot.IterateAllNodes() ) {
                if ( !Ready ) {
                    if ( KINode.IsKind(SyntaxKind.UsingDirective) ) {
                        Usings.Add(KINode.ToString());
                    } else if ( KINode.TryGetToken(SyntaxKind.EnumKeyword, out _) ) {
                        Ready = true;
                    }
                    continue;
                }

                SyntaxToken[] Tks = KINode.ChildTokens().ToArray();

                if ( KIMatch is null ) {
                    if ( Tks.TryGetFirst(STk => STk.IsKind(SyntaxKind.IdentifierToken), out SyntaxToken NameToken)) {
                        KIMatch = (NameToken, null, null);
                    }
                } else {
                    if ( KIMatch.Value.AssetPathToken is null ) {
                        if ( Tks.TryGetFirst(STk => STk.IsKind(SyntaxKind.StringLiteralToken), out SyntaxToken AssetPathToken) ) {
                            KIMatch = (KIMatch.Value.NameToken, AssetPathToken, null);
                        }
                    } else if (Tks.TryGetFirst(Stk => Stk.RawKind.Equals((int)SyntaxKind.BoolKeyword, (int)SyntaxKind.ByteKeyword, (int)SyntaxKind.DecimalKeyword, (int)SyntaxKind.DoubleKeyword, (int)SyntaxKind.FloatKeyword, (int)SyntaxKind.IntKeyword, (int)SyntaxKind.LongKeyword, (int)SyntaxKind.NullKeyword, (int)SyntaxKind.ObjectKeyword, (int)SyntaxKind.SByteKeyword, (int)SyntaxKind.StringKeyword, (int)SyntaxKind.UIntKeyword, (int)SyntaxKind.ULongKeyword, (int)SyntaxKind.UShortKeyword, (int)SyntaxKind.IdentifierToken), out SyntaxToken TypeToken)) {
                        EnumMembers.Add((KIMatch.Value.NameToken.Text, KIMatch.Value.AssetPathToken.Value.Text, TypeToken.Text));
                        KIMatch = null;
                    }
                }
            }
            
            Generators.Value = new IMatchGenerator[] {
                new InputHelper_OtherUsings().Init(Usings),
                new InputHelper_CtoInputs().Init(EnumMembers),
                new InputHelper_UpdInputs().Init(EnumMembers),
                new InputHelper_InputFlds().Init(EnumMembers)
            };
        }
    }

    /// <summary />
    internal class InputHelper_OtherUsings : IMatchGenerator {
        /// <summary />
        internal InputHelper_OtherUsings Init( IEnumerable<string> Usings ) {
            _Content = Usings.Join("\r\n");
            return this;
        }

        /// <inheritdoc />
        public string Name => "OtherUsings";

        /// <summary />
        string _Content = string.Empty;

        /// <inheritdoc />
        public string Generate( Match Match, string Line ) => _Content;
    }

    /// <summary />
    internal class InputHelper_CtoInputs : IMatchGenerator {
        /// <summary />
        internal InputHelper_CtoInputs Init( IEnumerable<(string Name, string AssetPath, string Type)> Members ) {
            StringBuilder SB = new StringBuilder();
            foreach ( (string Nm, string AssPth, string Tp) in Members ) {
                _ = SB.Append("\r\n\t\tInput");
                _ = SB.Append(Nm);
                _ = SB.Append(" = new Input<");
                _ = SB.Append(Tp);
                _ = SB.Append(">(KnownInput.");
                _ = SB.Append(Nm);
                _ = SB.Append(", ");
                _ = SB.Append(AssPth);
                _ = SB.Append(", default);");
            }
            _ = SB.Remove(0, 4);
            _Content = SB.ToString();
            return this;
        }

        string _Content = string.Empty;

        /// <inheritdoc />
        public string Name => "CtoInputs";

        /// <inheritdoc />
        public string Generate( Match Match, string Line ) => _Content;
    }

    /// <summary />
    internal class InputHelper_UpdInputs : IMatchGenerator {

        /// <summary />
        internal InputHelper_UpdInputs Init( IEnumerable<(string Name, string AssetPath, string Type)> Members ) {
            StringBuilder SB = new StringBuilder();
            foreach ( (string Nm, _, _) in Members ) {
                _ = SB.Append("\r\n\t\tUpdateInput(Input");
                _ = SB.Append(Nm);
                _ = SB.Append(");");
            }
            _ = SB.Remove(0, 4);
            _Content = SB.ToString();
            return this;
        }

        string _Content = string.Empty;

        /// <inheritdoc />
        public string Name => "UpdInputs";

        /// <inheritdoc />
        public string Generate( Match Match, string Line ) => _Content;
    }

    /// <summary />
    internal class InputHelper_InputFlds : IMatchGenerator {

        /// <summary />
        internal InputHelper_InputFlds Init( IEnumerable<(string Name, string AssetPath, string Type)> Members ) {
            StringBuilder SB = new StringBuilder();
            foreach ( (string Nm, _, string Tp) in Members ) {
                _ = SB.Append("\r\n\tpublic static Input<");
                _ = SB.Append(Tp);
                _ = SB.Append("> Input");
                _ = SB.Append(Nm);
                _ = SB.Append(" { get; private set; } = null!;\r\n");
            }
            _ = SB.Remove(0, 3);
            _Content = SB.ToString().TrimEnd(2);
            return this;
        }

        string _Content = string.Empty;

        /// <inheritdoc />
        public string Name => "InputFlds";

        /// <inheritdoc />
        public string Generate( Match Match, string Line ) => _Content;
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