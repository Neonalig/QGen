//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by InputHelper-GenUtil.
//     Runtime Version: 0.3.0
//
//     Changes to this file may cause incorrect behaviour and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System.Runtime.CompilerServices;

using System.Numerics;

namespace QGen.Sample;

[CompilerGenerated]
public static partial class InputHelper {

    public static void ConstructInputs() {
        InputFire = new Input<bool>(KnownInput.Fire, "Player/Fire", default);
		InputMove = new Input<Vector2>(KnownInput.Move, "Player/Move", default);
		InputLook = new Input<Vector2>(KnownInput.Look, "Player/Look", default);
		InputClick = new Input<bool>(KnownInput.Click, "UI/Click", default);
		InputFocus = new Input<float>(KnownInput.Focus, "UI/Focus", default);
    }

    public static void UpdateAll() {
        UpdateInput(InputFire);
		UpdateInput(InputMove);
		UpdateInput(InputLook);
		UpdateInput(InputClick);
		UpdateInput(InputFocus);
    }

    public static Input<bool> InputFire { get; private set; } = null!;

	public static Input<Vector2> InputMove { get; private set; } = null!;

	public static Input<Vector2> InputLook { get; private set; } = null!;

	public static Input<bool> InputClick { get; private set; } = null!;

	public static Input<float> InputFocus { get; private set; } = null!;

}
