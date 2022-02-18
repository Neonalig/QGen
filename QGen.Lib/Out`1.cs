namespace QGen.Lib;

/// <summary>
/// Type housing allowing values to be passed through asynchronous methods.
/// </summary>
/// <typeparam name="T"></typeparam>
public class Out<T> {

    /// <summary>
    /// Gets or sets the value.
    /// </summary>
    /// <value>
    /// The value.
    /// </value>
    public T Value { get; set; }

    /// <summary>
    /// Initialises a new instance of the <see cref="Out{T}"/> class.
    /// </summary>
    /// <param name="Value">The value.</param>
    public Out( T Value ) => this.Value = Value;

    /// <summary>
    /// Performs an <see langword="implicit"/> conversion from <see cref="Out{T}"/> to <see cref="T"/>.
    /// </summary>
    /// <param name="Out">The out.</param>
    /// <returns>
    /// The result of the conversion.
    /// </returns>
    public static implicit operator T( Out<T> Out ) => Out.Value;

    /// <summary>
    /// Performs an <see langword="implicit"/> conversion from <see cref="T"/> to <see cref="Out{T}"/>.
    /// </summary>
    /// <param name="Value">The value.</param>
    /// <returns>
    /// The result of the conversion.
    /// </returns>
    public static implicit operator Out<T>( T Value ) => new Out<T>(Value);

}