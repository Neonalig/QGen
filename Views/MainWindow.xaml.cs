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

using QGen.Core;
using QGen.Lib.Common;
using QGen.Lib.FileSystem;

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

    static async Task TestTwoAsync() {
        DirectoryInfo ReadDir = new DirectoryInfo("E:\\Projects\\Visual Studio\\QGen\\QGen.Sample");

        Result Res = await ScriptGenerator.GenerateAsync(ReadDir, new [] { new InputHelper_AG() }, new CancellationToken());
        Res.Log();
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