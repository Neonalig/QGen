using System.CodeDom.Compiler;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace QGen.Core;

/// <summary>
/// Provides methods to generate assemblies from raw script files.
/// </summary>
public class AssemblyGenerator {

    static readonly DirectoryInfo _CurrentDir = new DirectoryInfo(Environment.CurrentDirectory);

    /// <inheritdoc cref="GenerateAssembly(FileInfo, string, out string?)"/>
    public static bool GenerateAssembly( FileInfo ReadFile, [NotNullWhen(true)] out string? Destination ) => GenerateAssembly(ReadFile, Path.GetFileNameWithoutExtension(ReadFile.Name), out Destination);

    /// <summary>
    /// Generates an assembly from the given script file.
    /// </summary>
    /// <param name="ReadFile">The file to read.</param>
    /// <param name="AssemblyName">The name to save the generated executable with.</param>
    /// <param name="Destination">The path to the generated executable.</param>
    public static bool GenerateAssembly( FileInfo ReadFile, string AssemblyName, [NotNullWhen(true)] out string? Destination ) {
        CodeDomProvider Provider = CodeDomProvider.CreateProvider("CSharp"); //Alternatively: VisualBasic

        //FileInfo ReadDestFile = ReadFile.WithExtension("_cs.exe");
        FileInfo ReadDestFile = _CurrentDir.GetSubFile($"{AssemblyName}.exe");

        CompilerParameters CP = new CompilerParameters {
            GenerateExecutable = true,
            OutputAssembly = ReadDestFile.FullName,
            GenerateInMemory = false, //Physical file
            TreatWarningsAsErrors = false
        };

        CompilerResults CR = Provider.CompileAssemblyFromFile(CP, ReadFile.FullName);
        if ( CR.Errors.Count > 0 ) {
            Debug.WriteLine("An error occurred.", "ERROR");
            foreach ( CompilerError Err in CR.Errors ) {
                Debug.WriteLine($"\t{Err}\n", "ERROR");
            }
            Destination = null;
            return false;
        }

        Debug.WriteLine($"Source '{AssemblyName}' was built into '{CR.PathToAssembly}' successfully.");
        Destination = CR.PathToAssembly;
        return true;
    }
}
