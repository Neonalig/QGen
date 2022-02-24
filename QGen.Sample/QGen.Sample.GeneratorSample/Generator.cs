using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using System.Diagnostics;
using System.Text;

using QGen.Lib;
using QGen.Lib.Common;
using QGen.Lib.FileSystem;

namespace QGen.Sample.Generator;

public class Generator : IGeneratorProvider {

    readonly IFileGenerator[] _Generators = { new InputHelper_AG() };

    #region IGeneratorProvider Implementation

    /// <inheritdoc />
    public Task<Result<IEnumerable<IFileGenerator>>> GetGeneratorsAsync( CancellationToken Token = new CancellationToken() ) => _Generators.AsEnum().GetResult(true).AsTask();

    /// <inheritdoc />
    public string RequestedRootFolder => @"QGen\QGen.Sample\QGen.Sample.GenerationSample";

    /// <inheritdoc />
    public string DefaultRootFolder => @"E:\Projects\Visual Studio\QGen\QGen.Sample\QGen.Sample.GenerationSample";

    #endregion
}

public class InputHelper_AG : TemplateModifier {

    #region IFileGenerator Implementation

    /// <inheritdoc />
    public override string Name => "InputHelper-GenUtil";

    /// <inheritdoc />
    public override Version Version { get; } = new Version(0, 3, 0);

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
            //Debug.WriteLine($"\tNode: {KINode}.");
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

        IDynamicMatchGenerator[] DynGens = {
            new OtherUsings_MatchGenerator(),
            new CtoInputs_MatchGenerator(),
            new UpdInputs_MatchGenerator(),
            new InputFlds_MatchGenerator()
        };

        foreach ( IDynamicMatchGenerator DynGen in DynGens ) {
            DynGen.AppendLines(Usings);
        }

        foreach ( (string Nm, string AssPth, string Tp) in EnumMembers ) {
            foreach ( IDynamicMatchGenerator DynGen in DynGens ) {
                DynGen.AppendLines(Nm, AssPth, Tp);
            }
        }

        return DynGens.Select(MG => MG.Finalise()).AsEnum().GetResult(true);
    }

    /// <summary>
    /// <see cref="DynamicMatchGenerator"/> for the <c>$(OtherUsings)</c> variable.
    /// </summary>
    /// <seealso cref="DynamicMatchGenerator" />
    internal sealed class OtherUsings_MatchGenerator : DynamicMatchGenerator {

        /// <inheritdoc />
        public override string GeneratorName => "OtherUsings";

        /// <inheritdoc />
        internal override void AppendLines( StringBuilder SB, string Name, string AssetPath, string Type ) { }

        /// <inheritdoc />
        internal override void AppendLines( StringBuilder SB, IEnumerable<string> Usings ) => SB.Append(Usings.Join("\r\n"));

        /// <inheritdoc />
        internal override string Finalise( StringBuilder SB ) => SB.ToString();

    }

    /// <summary>
    /// <see cref="DynamicMatchGenerator"/> for the <c>$(CtoInputs)</c> variable.
    /// </summary>
    /// <seealso cref="DynamicMatchGenerator" />
    internal sealed class CtoInputs_MatchGenerator : DynamicMatchGenerator {

        /// <inheritdoc />
        public override string GeneratorName => "CtoInputs";

        /// <inheritdoc />
        internal override void AppendLines( StringBuilder SB, string Name, string AssetPath, string Type ) =>
            SB.Append("\r\n\t\tInput")
                .Append(Name)
                .Append(" = new Input<")
                .Append(Type)
                .Append(">(KnownInput.")
                .Append(Name)
                .Append(", ")
                .Append(AssetPath)
                .Append(", default);");

        /// <inheritdoc />
        internal override void AppendLines( StringBuilder SB, IEnumerable<string> Usings ) { }

        /// <inheritdoc />
        internal override string Finalise( StringBuilder SB ) => SB.Remove(0, 4).ToString();

    }

    /// <summary>
    /// <see cref="DynamicMatchGenerator"/> for the <c>$(UpdInputs)</c> variable.
    /// </summary>
    /// <seealso cref="DynamicMatchGenerator" />
    internal sealed class UpdInputs_MatchGenerator : DynamicMatchGenerator {

        /// <inheritdoc />
        public override string GeneratorName => "UpdInputs";

        /// <inheritdoc />
        internal override void AppendLines( StringBuilder SB, string Name, string AssetPath, string Type ) =>
            SB.Append("\r\n\t\tUpdateInput(Input")
                .Append(Name)
                .Append(");");

        /// <inheritdoc />
        internal override void AppendLines( StringBuilder SB, IEnumerable<string> Usings ) { }

        /// <inheritdoc />
        internal override string Finalise( StringBuilder SB ) => SB.Remove(0, 4).ToString();

    }

    /// <summary>
    /// <see cref="DynamicMatchGenerator"/> for the <c>$(InputFlds)</c> variable.
    /// </summary>
    /// <seealso cref="DynamicMatchGenerator" />
    internal sealed class InputFlds_MatchGenerator : DynamicMatchGenerator {

        /// <inheritdoc />
        public override string GeneratorName => "InputFlds";

        /// <inheritdoc />
        internal override void AppendLines( StringBuilder SB, string Name, string AssetPath, string Type ) =>
            SB.Append("\r\n\tpublic static Input<")
                .Append(Type)
                .Append("> Input")
                .Append(Name)
                .Append(" { get; private set; } = null!;\r\n");
        
        /// <inheritdoc />
        internal override void AppendLines( StringBuilder SB, IEnumerable<string> Usings ) { }

        /// <inheritdoc />
        internal override string Finalise( StringBuilder SB ) => SB.Remove(0, 3).ToString();

    }

    /// <summary>
    /// A dynamic match generator definition.
    /// </summary>
    internal interface IDynamicMatchGenerator {

        /// <summary>
        /// Appends the lines to the internal string builder.
        /// </summary>
        /// <param name="Name">The enum member's name.</param>
        /// <param name="AssetPath">The enum member's asset path.</param>
        /// <param name="Type">The enum member's value type.</param>
        void AppendLines( string Name, string AssetPath, string Type );

        /// <summary>
        /// Appends the using statements to the internal string builder.
        /// </summary>
        /// <param name="Usings">The using statements.</param>
        void AppendLines( IEnumerable<string> Usings );

        /// <summary>
        /// Finalises the string generated in the internal string builder, constructing the respective match generator.
        /// </summary>
        IMatchGenerator Finalise();

    }

    /// <inheritdoc cref="IDynamicMatchGenerator"/>
    internal abstract class DynamicMatchGenerator : IDynamicMatchGenerator {
        readonly StringBuilder _SB = new StringBuilder();

        /// <inheritdoc cref="AppendLines(string, string, string)"/>
        /// <param name="SB">The string builder to append the lines to.</param>
        /// <param name="Name"><inheritdoc cref="AppendLines(string, string, string)"/></param>
        /// <param name="AssetPath"><inheritdoc cref="AppendLines(string, string, string)"/></param>
        /// <param name="Type"><inheritdoc cref="AppendLines(string, string, string)"/></param>
        internal abstract void AppendLines( StringBuilder SB, string Name, string AssetPath, string Type );

        /// <summary>
        /// Appends the using statements to the internal string buffer.
        /// </summary>
        /// <param name="SB">The string builder to append the lines to.</param>
        /// <param name="Usings">The using statements.</param>
        /// <seealso cref="AppendLines(string, string, string)"/>
        internal abstract void AppendLines( StringBuilder SB, IEnumerable<string> Usings );

        /// <summary>
        /// Finalises the internal string builder's resultant string, caching the result.
        /// </summary>
        /// <param name="SB">The string builder.</param>
        /// <returns>The final replacement string.</returns>
        internal abstract string Finalise(StringBuilder SB);

        IMatchGenerator? _Generator = null;

        #region ISynBasedMatchGenerator Implementation

        /// <inheritdoc />
        public void AppendLines( string Name, string AssetPath, string Type ) => AppendLines(_SB, Name, AssetPath, Type);

        /// <inheritdoc />
        public void AppendLines( IEnumerable<string> Usings ) => AppendLines(_SB, Usings);

        /// <inheritdoc />
        public IMatchGenerator Finalise() => _Generator ??= new MatchGenerator(GeneratorName, Finalise(_SB));

        /// <summary>
        /// Gets the name of the generator.
        /// </summary>
        /// <value>
        /// The name of the generator.
        /// </value>
        public abstract string GeneratorName { get; }

        #endregion
    }
}