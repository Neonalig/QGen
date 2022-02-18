#region Copyright (C) 2017-2022  Cody Bock
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License (Version 3.0)
// as published by the Free Software Foundation.
// 
// More information can be found here: https://www.gnu.org/licenses/gpl-3.0.en.html
#endregion

#region Using Directives

using System.IO;
using System.Text.RegularExpressions;

using QGen.Core;

#endregion

namespace QGen.Views;

/// <summary> Interaction logic for MainWindow.xaml </summary>
public partial class MainWindow {
    public MainWindow() {
        InitializeComponent();
        DataContext = this;

        FileInfo TestFile = new FileInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "template.cs"));
        FileInfo DestFile = new FileInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "destfile.cs"));

        IMatchGenerator[] Gens = { new ExCond(), new ExMethods() };

        string[] Lines = File.ReadAllLines(TestFile.FullName);
        string[] NewLines = Generator.Generate(Lines, Gens).ToArray();

        if ( DestFile.Exists ) {
            DestFile.Delete();
        }
        File.WriteAllLines(DestFile.FullName, NewLines);

        _ = Process.Start(new ProcessStartInfo("subl", $"\"{DestFile.FullName}\"") {
            UseShellExecute = true
        });
        //Close();
        Environment.Exit(0);
    }
}

public readonly struct ExMethods : IMatchGenerator {
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


public readonly struct ExCond : IMatchGenerator {
    /// <inheritdoc />
    public string Name => @"exCond";

    /// <inheritdoc />
    public string Generate( Match Match, string Line ) =>
        //""
        ", \"RELEASE\""
    ;
}
