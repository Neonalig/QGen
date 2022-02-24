#region Copyright (C) 2017-2022  Cody Bock
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License (Version 3.0)
// as published by the Free Software Foundation.
// 
// More information can be found here: https://www.gnu.org/licenses/gpl-3.0.en.html
#endregion

#region Using Directives

using System.Numerics;

#endregion

namespace QGen.Sample;

public enum KnownInput {
    [AssetPath("Player/Fire", typeof(bool))]
    Fire,
    [AssetPath("Player/Move", typeof(Vector2))]
    Move,
    [AssetPath("Player/Look", typeof(Vector2))]
    Look,
    [AssetPath("UI/Click", typeof(bool))]
    Click,
    [AssetPath("UI/Focus", typeof(float))]
    Focus
}