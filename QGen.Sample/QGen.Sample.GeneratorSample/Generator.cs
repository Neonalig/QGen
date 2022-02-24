using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using System.Diagnostics;
using System.Text;

using QGen.Lib;
using QGen.Lib.Common;
using QGen.Lib.FileSystem;

namespace QGen.Sample.Generator;

public class Generator : IGeneratorProvider {

    #region IGeneratorProvider Implementation

    IFileGenerator[] _Generators;

    /// <inheritdoc />
    public async Task<Result<IEnumerable<IFileGenerator>>> GetGeneratorsAsync( CancellationToken Token = new CancellationToken() ) {

    }

    #endregion
}

public class InputHelper_AG : TemplateModifier {

    #region IFileGenerator Implementation

    /// <inheritdoc />
    public override string Name => "InputHelper-GenUtil";

    /// <inheritdoc />
    public override Version Version { get; } = new Version(0, 2, 1);

    #endregion

    /// <inheritdoc />
    public override string DestinationPath => "InputHelperAG.cs";

    static readonly IAsyncEnumerable<string> _TempLines = new []{
            @"using System.Runtime.CompilerServices;",
            @"",
            @"$(OtherUsings)",
            @"",
            @"namespace QGen.Sample;",
            @"",
            @"[CompilerGenerated]",
            @"public static partial class InputHelper {",
            @"",
            @"    public static void ConstructInputs() {",
            @"        $(CtoInputs)",
            @"    }",
            @"",
            @"    public static void UpdateAll() {",
            @"        $(UpdInputs)",
            @"    }",
            @"",
            @"    $(InputFlds)",
            @"}"
        }.AsAsync();

    /// <inheritdoc />
    public override IAsyncEnumerable<string> TemplateLines => _TempLines;

    /// <inheritdoc />
    public override async Task<Result<IEnumerable<IMatchGenerator>>> LookupAsync( ParsedDirectory RootDirectory, ParsedFile DestinationFile, CancellationToken Token ) {
        ParsedFile EnumFile = RootDirectory["KnownInput.cs"];

        SyntaxNode TreeRoot = await EnumFile.RootNode;
        bool Ready = false;
        List<string> Usings = new List<string>();
        List<(string Name, string AssetPath, string Type)> EnumMembers = new List<(string Name, string AssetPath, string Type)>();

        (SyntaxToken NameToken, SyntaxToken? AssetPathToken, SyntaxToken? TypeToken)? KIMatch = null;
        foreach ( SyntaxNode KINode in TreeRoot.IterateAllNodes() ) {
            Debug.WriteLine($"\tNode: {KINode}.");
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
                if ( Tks.TryGetFirst(STk => STk.IsKind(SyntaxKind.IdentifierToken), out SyntaxToken NameToken) ) {
                    KIMatch = (NameToken, null, null);
                }
            } else {
                if ( KIMatch.Value.AssetPathToken is null ) {
                    if ( Tks.TryGetFirst(STk => STk.IsKind(SyntaxKind.StringLiteralToken), out SyntaxToken AssetPathToken) ) {
                        KIMatch = (KIMatch.Value.NameToken, AssetPathToken, null);
                    }
                } else if ( Tks.TryGetFirst(Stk => Stk.RawKind.Equals((int)SyntaxKind.BoolKeyword, (int)SyntaxKind.ByteKeyword, (int)SyntaxKind.DecimalKeyword, (int)SyntaxKind.DoubleKeyword, (int)SyntaxKind.FloatKeyword, (int)SyntaxKind.IntKeyword, (int)SyntaxKind.LongKeyword, (int)SyntaxKind.NullKeyword, (int)SyntaxKind.ObjectKeyword, (int)SyntaxKind.SByteKeyword, (int)SyntaxKind.StringKeyword, (int)SyntaxKind.UIntKeyword, (int)SyntaxKind.ULongKeyword, (int)SyntaxKind.UShortKeyword, (int)SyntaxKind.IdentifierToken), out SyntaxToken TypeToken) ) {
                    EnumMembers.Add((KIMatch.Value.NameToken.Text, KIMatch.Value.AssetPathToken.Value.Text, TypeToken.Text));
                    KIMatch = null;
                }
            }
        }

        StringBuilder
            CtoInputsSB = new StringBuilder(),
                UpdInputsSB = new StringBuilder(),
                InputFldsSB   = new StringBuilder();
        foreach ( (string Nm, string AssPth, string Tp) in EnumMembers ) {
            _ = CtoInputsSB.Append("\r\n\t\tInput");
            _ = CtoInputsSB.Append(Nm);
            _ = CtoInputsSB.Append(" = new Input<");
            _ = CtoInputsSB.Append(Tp);
            _ = CtoInputsSB.Append(">(KnownInput.");
            _ = CtoInputsSB.Append(Nm);
            _ = CtoInputsSB.Append(", ");
            _ = CtoInputsSB.Append(AssPth);
            _ = CtoInputsSB.Append(", default);");

            _ = UpdInputsSB.Append("\r\n\t\tUpdateInput(Input");
            _ = UpdInputsSB.Append(Nm);
            _ = UpdInputsSB.Append(");");

            _ = InputFldsSB.Append("\r\n\tpublic static Input<");
            _ = InputFldsSB.Append(Tp);
            _ = InputFldsSB.Append("> Input");
            _ = InputFldsSB.Append(Nm);
            _ = InputFldsSB.Append(" { get; private set; } = null!;\r\n");
        }
        _ = CtoInputsSB.Remove(0, 4);
        _ = UpdInputsSB.Remove(0, 4);
        _ = InputFldsSB.Remove(0, 3);

        return new IMatchGenerator[] {
                new MatchGenerator("OtherUsings", Usings.Join("\r\n")),
                new MatchGenerator("CtoInputs", CtoInputsSB.ToString()),
                new MatchGenerator("UpdInputs", UpdInputsSB.ToString()),
                new MatchGenerator("InputFlds", InputFldsSB.ToString().TrimEnd(2))
            }.AsEnumerable().GetResult(true);
    }

    /// <summary>
    /// A dynamic match generator definition.
    /// </summary>
    internal interface ISynBasedMatchGenerator {

        /// <summary>
        /// Appends the lines to the internal string builder.
        /// </summary>
        /// <param name="Name">The enum member's name.</param>
        /// <param name="AssetPath">The enum member's asset path.</param>
        /// <param name="Type">The enum member's value type.</param>
        void AppendLines( string Name, string AssetPath, string Type );

        /// <summary>
        /// Finalises the string generated in the internal string builder.
        /// </summary>
        void Finalise();

        /// <summary>
        /// Gets the match text generated via the internal string builder and specified enum members.
        /// </summary>
        /// <value>
        /// The generated match text.
        /// </value>
        string MatchText { get; }

        /// <summary>
        /// Gets the name of the generator.
        /// </summary>
        /// <value>
        /// The name of the generator.
        /// </value>
        string GeneratorName { get; }

        /// <summary>
        /// Gets the match generator.
        /// </summary>
        /// <value>
        /// The match generator.
        /// </value>
        IMatchGenerator? Generator { get; set; }

        /// <summary>
        /// Gets the match generator.
        /// </summary>
        /// <remarks>Ensure the method is only invoked <b>after</b> <see cref="AppendLines(string, string, string)"/> has been invoked.</remarks>
        /// <param name="SynBasedGen">The dynamic match generator definition.</param>
        /// <returns>The match generator.</returns>
        static IMatchGenerator GetMatchGenerator( ISynBasedMatchGenerator SynBasedGen ) => SynBasedGen.Generator ??= new MatchGenerator(SynBasedGen.GeneratorName, SynBasedGen.MatchText);
    }

    /// <inheritdoc cref="ISynBasedMatchGenerator"/>
    internal abstract class SynBasedMatchGenerator : ISynBasedMatchGenerator {
        readonly StringBuilder _SB = new StringBuilder();

        /// <inheritdoc cref="AppendLines(string, string, string)"/>
        /// <param name="SB">The string builder to append the lines to.</param>
        /// <param name="Name"><inheritdoc cref="AppendLines(string, string, string)"/></param>
        /// <param name="AssetPath"><inheritdoc cref="AppendLines(string, string, string)"/></param>
        /// <param name="Type"><inheritdoc cref="AppendLines(string, string, string)"/></param>
        internal abstract void AppendLines( StringBuilder SB, string Name, string AssetPath, string Type );

        internal abstract string Finalise(StringBuilder SB);

        #region ISynBasedMatchGenerator Implementation

        /// <inheritdoc />
        public void AppendLines( string Name, string AssetPath, string Type ) => AppendLines(_SB, Name, AssetPath, Type);

        /// <inheritdoc />
        public void Finalise() => MatchText = Finalise(_SB);

        /// <inheritdoc />
        public string MatchText { get; private set; } = string.Empty;

        /// <inheritdoc />
        public abstract string GeneratorName { get; }

        /// <inheritdoc />
        public IMatchGenerator? Generator { get; set; }

        #endregion
    }
}