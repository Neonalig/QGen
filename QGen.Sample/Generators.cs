using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Text.RegularExpressions;

using QGen.Lib;

namespace QGen.Sample;

public static class GeneratorSupplier {
    //public static readonly IReadOnlyList<(string Name, string Type, string Path)> Actions;
}


public readonly struct GenMethodConstructInputs : IMatchGenerator {
    /// <inheritdoc />
    public string Name => "MethodBody_ConstructInputs";

    /// <inheritdoc />
    public string Generate( Match Match, string Line ) => null;
}