using System.Numerics;

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