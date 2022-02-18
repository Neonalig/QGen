using System.Runtime.CompilerServices;

namespace QGen.Sample;

[CompilerGenerated]
public static partial class InputHelper {

    public static void ConstructInputs() {
        $(CtoInputs)
        //InputMove = new Input<Vector2>(KnownInput.Move, "Player/Move", default);
    }

    public static void UpdateAll() {
        $(UpdInputs)
        //UpdateInput(InputMove);
    }

    $(InputFlds)

    //public static Input<Vector2> InputMove { get; private set; } = null!;
}