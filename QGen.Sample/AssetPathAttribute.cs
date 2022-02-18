namespace QGen.Sample;

[AttributeUsage(AttributeTargets.Field)]
public class AssetPathAttribute : Attribute {
    public string Path { get; }
    public Type Type { get; }

    public AssetPathAttribute( string Path, Type Type ) {
        this.Path = Path;
        this.Type = Type;
    }
}