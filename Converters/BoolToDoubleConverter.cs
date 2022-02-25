using System.Globalization;
using System.Windows.Data;

using MVVMUtils;

namespace QGen.Converters;

[ValueConversion(typeof(bool), typeof(double))]
public class BoolToDoubleConverter : ValueConverter<bool, double> {

    /// <summary>
    /// Gets or sets the value to return when <see langword="true"/>.
    /// </summary>
    /// <value>
    /// The value to return when <see langword="true"/>.
    /// </value>
    public double True { get; set; } = 1.0;

    /// <summary>
    /// Gets or sets the value to return when <see langword="aflse"/>.
    /// </summary>
    /// <value>
    /// The value to return when <see langword="false"/>.
    /// </value>
    public double False { get; set; } = 0.0;

    #region Overrides of ValueConverter<bool,double>

    /// <inheritdoc />
    public override bool CanReverse => false;

    /// <inheritdoc />
    public override double Forward( bool From, object? Parameter = null, CultureInfo? Culture = null ) => From ? True : False;

    /// <inheritdoc />
    public override bool Reverse( double To, object? Parameter = null, CultureInfo? Culture = null ) => false;

    #endregion

}
