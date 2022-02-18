namespace QGen.Sample;

/// <summary>
/// Represents an input action.
/// </summary>
/// <typeparam name="T">The type of the input.</typeparam>
public class Input<T> where T : struct {
    /// <summary>
    /// Gets the type of the input.
    /// </summary>
    /// <value>
    /// The type of the input.
    /// </value>
    public KnownInput InputType { get; }

    /// <summary>
    /// Gets the asset path.
    /// </summary>
    /// <value>
    /// The asset path.
    /// </value>
    public string AssetPath { get; }

    /// <summary>
    /// Gets or sets the value.
    /// </summary>
    /// <value>
    /// The value.
    /// </value>
    public T Value { get; set; }

    /// <summary>
    /// Initialises a new instance of the <see cref="Input{T}"/> class.
    /// </summary>
    /// <param name="InputType">The type of the input.</param>
    /// <param name="AssetPath">The path to the input action.</param>
    /// <param name="Value">The value.</param>
    public Input( KnownInput InputType, string AssetPath, T Value = default ) {
        this.InputType = InputType;
        this.AssetPath = AssetPath;
        this.Value = Value;
    }

    /// <summary>
    /// Performs an <see langword="implicit"/> conversion from <see cref="Input{T}"/> to <see cref="T"/>.
    /// </summary>
    /// <param name="Input">The input.</param>
    /// <returns>
    /// The result of the conversion.
    /// </returns>
    public static implicit operator T( Input<T> Input ) => Input.Value;
}