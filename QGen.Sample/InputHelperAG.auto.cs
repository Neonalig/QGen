using System.Runtime.CompilerServices;

$(OtherUsings)

namespace QGen.Sample;

[CompilerGenerated]
public static partial class InputHelper {

    public static void ConstructInputs() {
        $(CtoInputs)
    }

    public static void UpdateAll() {
        $(UpdInputs)
    }

    $(InputFlds)
}