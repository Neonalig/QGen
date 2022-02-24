#region Copyright (C) 2017-2022  Cody Bock
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License (Version 3.0)
// as published by the Free Software Foundation.
// 
// More information can be found here: https://www.gnu.org/licenses/gpl-3.0.en.html
#endregion

namespace QGen.Sample;

public static partial class InputHelper {

    static readonly InputAsset _Asset = null!;

    static InputHelper() {
        ConstructInputs();
        UpdateAll();
    }

    static T IntGetInput<T>( string Path ) => (T)_Asset.GetValue(Path);
    static void UpdateInput<T>( Input<T> Inp ) where T : struct => Inp.Value = IntGetInput<T>(Inp.AssetPath);

}