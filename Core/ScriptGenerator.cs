#region Copyright (C) 2017-2022  Cody Bock
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License (Version 3.0)
// as published by the Free Software Foundation.
// 
// More information can be found here: https://www.gnu.org/licenses/gpl-3.0.en.html
#endregion

#region Using Directives

using System.IO;

using QGen.Lib.Common;
using QGen.Lib.FileSystem;

#endregion

namespace QGen.Core;

/// <summary>
/// Provides methods to generate assemblies from raw script files.
/// </summary>
public class ScriptGenerator {

    public static async Task<Result> GenerateAsync( DirectoryInfo RootDirectory, IEnumerable<IFileGenerator> Generators, CancellationToken Token = new CancellationToken() ) {
        ParsedDirectory Dir = new ParsedDirectory(RootDirectory);
        foreach ( IFileGenerator Generator in Generators ) {
            if ( Token.IsCancellationRequested ) { return Result.Cancelled(true); }
            switch ( Generator ) {
                case ITemplateModifier Mod:
                    if ( !Dir.TryGetFilePointer(Mod.DestinationPath, out ParsedFile? DestinationFile) ) {
                        return Result.FilePathInvalid(Mod.DestinationPath);
                    }

                    if ( !(await Mod.LookupAsync(Dir, DestinationFile, Token)).TryGetValue(out (IEnumerable<string> Lines, IEnumerable<IMatchGenerator> MatchGens) Res ) ) {
                        return Result.LookupFailed(Mod);
                    }

                    if ( DestinationFile.Exists ) { DestinationFile.Delete(); }
                    await using ( StreamWriter SW = File.CreateText(DestinationFile.FullName) ) {
                        foreach ( string HeaderLine in Mod.GetFileHeader() ) {
                            await SW.WriteLineAsync(HeaderLine);
                        }
                        foreach ( string Line in SourceGenerator.Generate(Res.Lines, Res.MatchGens) ) {
                            await SW.WriteLineAsync(Line);
                        }
                        await SW.FlushAsync();
                    }
                    break;
                case IFileCreator Crt:
                    int I = 0;
                    foreach ( string Dest in Crt.DestinationFiles ) {
                        if ( !Dir.TryGetFilePointer(Dest, out ParsedFile? DestFile) ) {
                            return Result.FilePathInvalid(Dest);
                        }

                        if ( DestFile.Exists ) { DestFile.Delete(); }
                        Result<IEnumerable<string>> CreateRes = await Crt.CreateAsync(Dir, I, DestFile, Token);
                        if ( !CreateRes.Success ) {
                            return CreateRes;
                        }

                        await using ( StreamWriter SW = File.CreateText(DestFile.FullName) ) {
                            foreach ( string HeaderLine in Crt.GetFileHeader() ) {
                                await SW.WriteLineAsync(HeaderLine);
                            }
                            foreach ( string Line in CreateRes.Value ) {
                                await SW.WriteLineAsync(Line);
                            }
                            await SW.FlushAsync();
                        }

                        I++;
                    }
                    break;
                default:
                    throw new IndexOutOfRangeException();
            }
        }
        return Result.Successful;
    }

}
