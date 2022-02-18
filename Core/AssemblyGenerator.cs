using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Text;

namespace QGen.Core;

/// <summary>
/// Provides methods to generate assemblies from raw script files.
/// </summary>
public class AssemblyGenerator {

    /// <summary>
    /// Generates an assembly from the given folder.
    /// </summary>
    /// <param name="ReadDir">The folder to read the files within.</param>
    /// <param name="Result">The generated assembly.</param>
    public static bool GenerateAssembly( DirectoryInfo ReadDir, [NotNullWhen(true)] out Assembly? Result ) => GenerateAssembly(ReadDir.GetFiles("*.cs"), ReadDir.Name, out Result);

    /// <summary>
    /// Generates an assembly from the given files.
    /// </summary>
    /// <param name="Files">The files to read.</param>
    /// <param name="AssemblyName">The name of the generated assembly.</param>
    /// <param name="Result">The generated assembly.</param>
    public static bool GenerateAssembly( IList<FileInfo> Files, string AssemblyName, [NotNullWhen(true)] out Assembly? Result ) {
        SyntaxTree[] Trees = new SyntaxTree[Files.Count];

        for ( int I = 0; I < Files.Count; I++ ) {
            FileInfo File = Files[I];
            using ( FileStream FS = System.IO.File.OpenRead(File.FullName) ) {
                SourceText ST = SourceText.From(FS);
                SyntaxTree Tree = CSharpSyntaxTree.ParseText(ST);
                Trees[I] = Tree;
            }
        }

        return GenerateAssembly(Trees, AssemblyName, out Result);
    }

    /// <summary>
    /// Generates an assembly from the given syntax tree.
    /// </summary>
    /// <param name="Trees">The syntax trees to read.</param>
    /// <param name="AssemblyName">The name of the generated assembly.</param>
    /// <param name="Result">The generated assembly.</param>
    public static bool GenerateAssembly( SyntaxTree[] Trees, string AssemblyName, [NotNullWhen(true)] out Assembly? Result ) {
        string[] DefaultNamespaces = {
            "System",
            "System.IO",
            "System.Net",
            "System.Linq",
            "System.Text",
            "System.Text.RegularExpressions",
            "System.Collections.Generic",
        };

        string AssemblyPath = Path.GetDirectoryName(typeof(object).Assembly.Location)!;
        // assemblyPath = "C:\Program Files\dotnet\shared\Microsoft.NETCore.App\6.0.2\"

        MetadataReference[] DefaultReferences = {
            MetadataReference.CreateFromFile(Path.Combine(AssemblyPath, "mscorlib.dll")),
            MetadataReference.CreateFromFile(Path.Combine(AssemblyPath, "System.dll")),
            MetadataReference.CreateFromFile(Path.Combine(AssemblyPath, "System.Core.dll")),
            MetadataReference.CreateFromFile(Path.Combine(AssemblyPath, "System.Private.CoreLib.dll")),
            MetadataReference.CreateFromFile(Path.Combine(AssemblyPath, "System.Runtime.dll")),
            MetadataReference.CreateFromFile(typeof(IMatchGenerator).GetTypeInfo().Assembly.Location),
        };

        //MetadataReference[] Refs = DefaultReferences.Select(R => MetadataReference.CreateFromFile(R));
        //MetadataReference[] Refs = RefPaths.Select(R => AssemblyMetadata.CreateFromFile(R).GetReference());
        //Debug.WriteLine($"Compiling assembly '{AssemblyName}' with references '{Refs.Join("', '", MR => MR.Display ?? MR.ToString() ?? "<unknown name>")}'...");
        Debug.WriteLine("Compiling assembly...");

        //MetadataReferenceResolver Res

        CSharpCompilation Comp = CSharpCompilation.Create(
            assemblyName: AssemblyName,
            syntaxTrees: Trees,
            references: DefaultReferences,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary).WithUsings(DefaultNamespaces).WithAssemblyIdentityComparer(DesktopAssemblyIdentityComparer.Default));

        Debug.WriteLine($"External References: '{Comp.ExternalReferences.Join("', '", MR => MR.Display ?? "<Unk. Name>")}'");
        Debug.WriteLine($"References: '{Comp.References.Join("', '", MR => MR.Display ?? "<Unk. Name>")}'");

        using ( MemoryStream MS = new MemoryStream() ) {
            EmitResult Res = Comp.Emit(MS);

            if ( !Res.Success ) {
                Debug.WriteLine("Compilation failed!", "ERROR");
                foreach ( Diagnostic Diag in Res.Diagnostics ) {
                    if ( Diag.IsWarningAsError || Diag.Severity == DiagnosticSeverity.Error ) {
                        Debug.WriteLine($"{Diag.Id}: {Diag.GetMessage()}", "ERROR");
                    }
                }
                Result = null;
                return false;
            }

            Debug.WriteLine("Compilation successful!", "SUCCESS");
            _ = MS.Seek(0, SeekOrigin.Begin);
            Assembly Assem = AssemblyLoadContext.Default.LoadFromStream(MS);

            Debug.WriteLine($"Got assembly '{Assem.FullName}'");
            foreach ( Type Tp in Assem.GetTypes() ) {
                Debug.WriteLine($"\tType: {Tp.FullName}");
            }

            Result = Assem;
            return true;
        }
    }
}
