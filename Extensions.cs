using System.Runtime.CompilerServices;
using System.Windows;

namespace QGen;

/// <summary>
/// General extension methods and shorthand for WPF utilisation.
/// </summary>
public static class Extensions {



}

public class TypedDependencyProperty<T> {

    #region Fields / Properties

    /// <summary>
    /// Gets the underlying dependency property.
    /// </summary>
    /// <value>
    /// The dependency property.
    /// </value>
    public DependencyProperty Property { get; }

    #endregion

    #region Constructors

    /// <summary>
    /// Initialises a new instance of the <see cref="TypedDependencyProperty{T}"/> class.
    /// </summary>
    /// <param name="Property">The dependency property.</param>
    public TypedDependencyProperty( DependencyProperty Property ) => this.Property = Property;

    #endregion

    #region GetValue

    /// <summary>
    /// Gets the value of the dependency property in the specified dependency object.
    /// </summary>
    /// <param name="DependencyObject">The dependency object.</param>
    /// <returns>The current value of the property in the dependency object.</returns>
    public T GetValue( DependencyObject DependencyObject ) => (T)DependencyObject.GetValue(Property);

    #endregion

    #region SetValue

    /// <summary>
    /// Sets the value of the dependency property in the specified dependency object without first checking if the value has changed since.
    /// </summary>
    /// <param name="DependencyObject">The dependency object.</param>
    /// <param name="Value">The new value for the property in the dependency object.</param>
    public void ForceSetValue( DependencyObject DependencyObject, T Value ) => DependencyObject.SetValue(Property, Value);

    /// <summary>
    /// Sets the value of the dependency property in the specified dependency object if the value is different from the current value.
    /// </summary>
    /// <param name="DependencyObject">The dependency object.</param>
    /// <param name="Value">The new value for the property in the dependency object.</param>
    public void SetValue( DependencyObject DependencyObject, T Value ) {
        if ( Value is IEquatable<T> ValEq ) {
            SetValue(DependencyObject, Value, ValEq);
        } else {
            SetValue(DependencyObject, Value, _DefComp.Value);
        }
    }

    readonly Lazy<IEqualityComparer<T>> _DefComp = new Lazy<IEqualityComparer<T>>(() => EqualityComparer<T>.Default);

    /// <inheritdoc cref="SetValue(DependencyObject, T)"/>
    /// <param name="DependencyObject">The dependency object.</param>
    /// <param name="Value">The new value for the property in the dependency object.</param>
    /// <param name="EqualityComparer">The equality comparison method to invoke.</param>
    public void SetValue( DependencyObject DependencyObject, T Value, Func<T, T, bool> EqualityComparer ) {
        T CurrentValue = GetValue(DependencyObject);
        if ( !EqualityComparer(Value, CurrentValue) ) {
            ForceSetValue(DependencyObject, Value);
        }
    }

    /// <inheritdoc cref="SetValue(DependencyObject, T)"/>
    /// <param name="DependencyObject">The dependency object.</param>
    /// <param name="Value">The new value for the property in the dependency object.</param>
    /// <param name="Comparer">The equality comparer to use.</param>
    public void SetValue( DependencyObject DependencyObject, T Value, IEqualityComparer<T> Comparer ) {
        T CurrentValue = GetValue(DependencyObject);
        if ( !Comparer.Equals(Value, CurrentValue) ) {
            ForceSetValue(DependencyObject, Value);
        }
    }

    /// <inheritdoc cref="SetValue(DependencyObject, T)"/>
    /// <param name="DependencyObject">The dependency object.</param>
    /// <param name="Value">The new value for the property in the dependency object.</param>
    /// <param name="ValueComparer">The <see cref="IEquatable{T}"/> implementation of the specified <paramref name="Value"/>.</param>
    public void SetValue( DependencyObject DependencyObject, T Value, IEquatable<T> ValueComparer ) {
        T CurrentValue = GetValue(DependencyObject);
        if ( !ValueComparer.Equals(CurrentValue) ) {
            ForceSetValue(DependencyObject, Value);
        }
    }

    /// <inheritdoc cref="SetValue(DependencyObject, T)"/>
    /// <typeparam name="TX">The type of the value.</typeparam>
    /// <param name="DependencyObject">The dependency object.</param>
    /// <param name="Value">The new value for the property in the dependency object.</param>
    public void SetValue<TX>( DependencyObject DependencyObject, TX Value ) where TX : T, IEquatable<T> => SetValue(DependencyObject, Value, Value);

    #endregion

    /// <inheritdoc cref="DependencyProperty.Register(string, Type, Type, PropertyMetadata)"/>
    /// <typeparam name="TBase">The type of the <see cref="DependencyObject"/> containing the <see cref="DependencyProperty"/>.</typeparam>
    /// <param name="Property">The dependency property.</param>
    /// <param name="Base">The base dependency object.</param>
    /// <param name="Metadata">The metadata used for property registration.</param>
    /// <param name="PropertyName">The name of the property.</param>
    public static TypedDependencyProperty<T> Register<TBase>( T Property, TBase Base, PropertyMetadata Metadata, [CallerArgumentExpression("Property")] string PropertyName = "" ) => new TypedDependencyProperty<T>(DependencyProperty.Register(PropertyName, typeof(T), typeof(TBase), Metadata));
}