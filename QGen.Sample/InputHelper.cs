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