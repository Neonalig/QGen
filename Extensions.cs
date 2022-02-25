using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Windows;

namespace QGen;

/// <summary>
/// General extension methods and shorthand for WPF utilisation.
/// </summary>
public static class Extensions {

    /// <summary>
    /// Gets the non-typed callback.
    /// </summary>
    /// <typeparam name="T">The underlying value of the dependency property.</typeparam>
    /// <param name="TypedCallback">The typed callback.</param>
    /// <returns>The non-typed callback equivalent.</returns>
    public static PropertyChangedCallback GetNonTyped<T>( this TypedPropertyChangedCallback<T> TypedCallback ) {
        void Callback( DependencyObject D, DependencyPropertyChangedEventArgs E ) {
            TypedCallback.Invoke(D, new TypedDependencyPropertyChangedEventArgs<T>(E));
        }
        return Callback;
    }

    /// <summary>
    /// Gets the non-typed callback.
    /// </summary>
    /// <typeparam name="T">The underlying value of the dependency property.</typeparam>
    /// <param name="TypedCallback">The typed callback.</param>
    /// <returns>The non-typed callback equivalent.</returns>
    public static CoerceValueCallback GetNonTyped<T>( this TypedCoerceValueCallback<T> TypedCallback ) {
        object Callback( DependencyObject D, object BaseValue ) => TypedCallback.Invoke(D, (T)BaseValue)!;
        return Callback;
    }

    /// <summary>
    /// Gets the typed equivalent of the dependency property.
    /// </summary>
    /// <typeparam name="T">The underlying value type of the property.</typeparam>
    /// <param name="DependencyProperty">The dependency property.</param>
    /// <returns>A new <see cref="TypedDependencyProperty{T}"/> instance.</returns>
    public static TypedDependencyProperty<T> GetTyped<T>( this DependencyProperty DependencyProperty ) => new TypedDependencyProperty<T>(DependencyProperty);

    /// <summary>
    /// Gets the typed equivalent of the dependency property.
    /// </summary>
    /// <typeparam name="TBase">The type of the dependency object.</typeparam>
    /// <typeparam name="T">The underlying value type of the property.</typeparam>
    /// <param name="DependencyProperty">The dependency property.</param>
    /// <param name="DependencyObject">The dependency object containing the dependency property.</param>
    /// <returns>A new <see cref="TypedDependencyProperty{T}"/> instance.</returns>
    public static TypedDependencyProperty<TBase, T> GetTyped<TBase, T>( this DependencyProperty DependencyProperty, TBase DependencyObject ) where TBase : DependencyObject => new TypedDependencyProperty<TBase, T>(DependencyObject, DependencyProperty);

    /// <summary>
    /// Gets the typed dependency property from the dependency object.
    /// </summary>
    /// <typeparam name="TBase">The type of the dependency object.</typeparam>
    /// <typeparam name="T">The underlying value type of the property.</typeparam>
    /// <param name="DependencyProperty">The dependency property.</param>
    /// <param name="DependencyObject">The dependency object containing the dependency property.</param>
    /// <returns><inheritdoc cref="GetTyped{TBase, T}(DependencyProperty, TBase)"/></returns>
    public static TypedDependencyProperty<TBase, T> GetProperty<TBase, T>( this TBase DependencyObject, DependencyProperty DependencyProperty ) where TBase : DependencyObject => new TypedDependencyProperty<TBase, T>(DependencyObject, DependencyProperty);

    /// <inheritdoc cref="TypedDependencyProperty{TBase, T}.Register(T, TBase, T, string)"/>
    public static TypedDependencyProperty<TBase, T> Register<TBase, T>( this TBase DependencyObject, T Property, T DefaultValue = default!, [CallerArgumentExpression("Property")] string PropertyName = "" ) where TBase : DependencyObject => TypedDependencyProperty<TBase, T>.Register(Property, DependencyObject, DefaultValue, PropertyName);

    /// <inheritdoc cref="TypedDependencyProperty{TBase, T}.Register(T, TBase, PropertyMetadata, string)"/>
    public static TypedDependencyProperty<TBase, T> Register<TBase, T>( this TBase DependencyObject, T Property, PropertyMetadata Metadata, [CallerArgumentExpression("Property")] string PropertyName = "" ) where TBase : DependencyObject => TypedDependencyProperty<TBase, T>.Register(Property, DependencyObject, Metadata, PropertyName);

    /// <inheritdoc cref="TypedDependencyProperty{TBase, T}.Register(T, TBase, T, TypedPropertyChangedCallback{T}, string)"/>
    public static TypedDependencyProperty<TBase, T> Register<TBase, T>( this TBase DependencyObject, T Property, T DefaultValue, TypedPropertyChangedCallback<T> PropertyChangedCallback, [CallerArgumentExpression("Property")] string PropertyName = "" ) where TBase : DependencyObject => TypedDependencyProperty<TBase, T>.Register(Property, DependencyObject, DefaultValue, PropertyChangedCallback, PropertyName);

    /// <inheritdoc cref="TypedDependencyProperty{TBase, T}.Register(T, TBase, T, TypedPropertyChangedCallback{T}, TypedCoerceValueCallback{T}, string)"/>
    public static TypedDependencyProperty<TBase, T> Register<TBase, T>( this TBase DependencyObject, T Property, T DefaultValue, TypedPropertyChangedCallback<T> PropertyChangedCallback, TypedCoerceValueCallback<T> CoerceValueCallback, [CallerArgumentExpression("Property")] string PropertyName = "" ) where TBase : DependencyObject => TypedDependencyProperty<TBase, T>.Register(Property, DependencyObject, DefaultValue, PropertyChangedCallback, CoerceValueCallback, PropertyName);
}

public interface ITypedDependencyProperty<T> {

    #region Fields / Properties

    /// <summary>
    /// Gets the underlying dependency property.
    /// </summary>
    /// <value>
    /// The dependency property.
    /// </value>
    DependencyProperty Property { get; }

    #endregion

    #region GetValue

    /// <summary>
    /// Gets the value of the dependency property in the specified dependency object.
    /// </summary>
    /// <param name="DependencyObject">The dependency object.</param>
    /// <returns>The current value of the property in the dependency object.</returns>
    T GetValue( DependencyObject DependencyObject );

    #endregion

    #region SetValue

    /// <summary>
    /// Sets the value of the dependency property in the specified dependency object without first checking if the value has changed since.
    /// </summary>
    /// <param name="DependencyObject">The dependency object.</param>
    /// <param name="Value">The new value for the property in the dependency object.</param>
    void ForceSetValue( DependencyObject DependencyObject, T Value );

    /// <summary>
    /// Sets the value of the dependency property in the specified dependency object without first checking if the value has changed since.
    /// </summary>
    /// <param name="DependencyObject">The dependency object.</param>
    /// <param name="Value">The new value for the property in the dependency object.</param>
    void SetValue( DependencyObject DependencyObject, T Value );

    #endregion

}

public abstract class TypedDependencyPropertyBase<T> : ITypedDependencyProperty<T> {

    #region Fields / Properties
    
    /// <inheritdoc />
    public DependencyProperty Property { get; }

    #endregion

    #region Constructors

    /// <summary>
    /// Initialises a new instance of the <see cref="TypedDependencyPropertyBase{T}"/> class.
    /// </summary>
    /// <param name="Property">The dependency property.</param>
    protected TypedDependencyPropertyBase( DependencyProperty Property ) => this.Property = Property;

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

    /// <inheritdoc />
    public void ForceSetValue( DependencyObject DependencyObject, T Value ) => DependencyObject.SetValue(Property, Value);

    /// <inheritdoc />
    public void SetValue( DependencyObject DependencyObject, T Value ) {
        if ( Value is IEquatable<T> ValEq ) {
            SetValue(DependencyObject, Value, ValEq);
        } else {
            SetValue(DependencyObject, Value, _DefComp.Value);
        }
    }

    readonly Lazy<IEqualityComparer<T>> _DefComp = new Lazy<IEqualityComparer<T>>(EqualityComparer<T>.Default.Retrieve());

    /// <inheritdoc cref="SetValue(DependencyObject, T)"/>
    /// <param name="DependencyObject">The dependency object.</param>
    /// <param name="Value">The new value for the property in the dependency object.</param>
    /// <param name="EqualityComparer">The equality comparison method to invoke.</param>
    internal void SetValue( DependencyObject DependencyObject, T Value, Func<T, T, bool> EqualityComparer ) {
        T CurrentValue = GetValue(DependencyObject);
        if ( !EqualityComparer(Value, CurrentValue) ) {
            ForceSetValue(DependencyObject, Value);
        }
    }

    /// <inheritdoc cref="SetValue(DependencyObject, T)"/>
    /// <param name="DependencyObject">The dependency object.</param>
    /// <param name="Value">The new value for the property in the dependency object.</param>
    /// <param name="Comparer">The equality comparer to use.</param>
    internal void SetValue( DependencyObject DependencyObject, T Value, IEqualityComparer<T> Comparer ) {
        T CurrentValue = GetValue(DependencyObject);
        if ( !Comparer.Equals(Value, CurrentValue) ) {
            ForceSetValue(DependencyObject, Value);
        }
    }

    /// <inheritdoc cref="SetValue(DependencyObject, T)"/>
    /// <param name="DependencyObject">The dependency object.</param>
    /// <param name="Value">The new value for the property in the dependency object.</param>
    /// <param name="ValueComparer">The <see cref="IEquatable{T}"/> implementation of the specified <paramref name="Value"/>.</param>
    internal void SetValue( DependencyObject DependencyObject, T Value, IEquatable<T> ValueComparer ) {
        T CurrentValue = GetValue(DependencyObject);
        if ( !ValueComparer.Equals(CurrentValue) ) {
            ForceSetValue(DependencyObject, Value);
        }
    }

    /// <inheritdoc cref="SetValue(DependencyObject, T)"/>
    /// <typeparam name="TX">The type of the value.</typeparam>
    /// <param name="DependencyObject">The dependency object.</param>
    /// <param name="Value">The new value for the property in the dependency object.</param>
    internal void SetValue<TX>( DependencyObject DependencyObject, TX Value ) where TX : T, IEquatable<T> => SetValue(DependencyObject, Value, Value);

    #endregion

}

public class TypedDependencyProperty<T> : TypedDependencyPropertyBase<T> {

    #region Constructors

    /// <inheritdoc />
    public TypedDependencyProperty( DependencyProperty Property ) : base(Property) { }

    #endregion

    #region SetValue

    /// <inheritdoc cref="TypedDependencyPropertyBase{T}.SetValue(DependencyObject, T, Func{T, T, bool})"/>
    public new void SetValue( DependencyObject DependencyObject, T Value, Func<T, T, bool> EqualityComparer ) => base.SetValue(DependencyObject, Value, EqualityComparer);

    /// <inheritdoc cref="TypedDependencyPropertyBase{T}.SetValue(DependencyObject, T, IEqualityComparer{T})"/>
    public new void SetValue( DependencyObject DependencyObject, T Value, IEqualityComparer<T> Comparer ) => base.SetValue(DependencyObject, Value, Comparer);

    /// <inheritdoc cref="TypedDependencyPropertyBase{T}.SetValue(DependencyObject, T, IEquatable{T})"/>
    public new void SetValue( DependencyObject DependencyObject, T Value, IEquatable<T> ValueComparer ) => base.SetValue(DependencyObject, Value, ValueComparer);

    /// <inheritdoc cref="TypedDependencyPropertyBase{T}.SetValue{TX}(DependencyObject, TX)"/>
    public new void SetValue<TX>( DependencyObject DependencyObject, TX Value ) where TX : T, IEquatable<T> => base.SetValue(DependencyObject, Value);

    #endregion

    #region Register

    /// <inheritdoc cref="Register{TBase}(T, T, TypedPropertyChangedCallback{T}, TypedCoerceValueCallback{T}, string)"/>
    /// <param name="Property"><inheritdoc cref="Register{TBase}(T, T, TypedPropertyChangedCallback{T}, TypedCoerceValueCallback{T}, string)"/></param>
    /// <param name="Metadata">The metadata to use if current type doesn't specify type-specific metadata.</param>
    /// <param name="PropertyName"><inheritdoc cref="Register{TBase}(T, T, TypedPropertyChangedCallback{T}, TypedCoerceValueCallback{T}, string)"/></param>
    [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Used for type-safety and attributes.")]
    public static TypedDependencyProperty<T> Register<TBase>( T Property, PropertyMetadata Metadata, [CallerArgumentExpression("Property")] string PropertyName = "" ) => new TypedDependencyProperty<T>(DependencyProperty.Register(PropertyName, typeof(T), typeof(TBase), Metadata));

    /// <inheritdoc cref="Register{TBase}(T, T, TypedPropertyChangedCallback{T}, TypedCoerceValueCallback{T}, string)"/>
    public static TypedDependencyProperty<T> Register<TBase>( T Property, T DefaultValue = default!, [CallerArgumentExpression("Property")] string PropertyName = "" ) => Register<TBase>(Property, new PropertyMetadata(DefaultValue), PropertyName);


    /// <inheritdoc cref="Register{TBase}(T, T, TypedPropertyChangedCallback{T}, TypedCoerceValueCallback{T}, string)"/>
    public static TypedDependencyProperty<T> Register<TBase>( T Property, T DefaultValue, TypedPropertyChangedCallback<T> PropertyChangedCallback, [CallerArgumentExpression("Property")] string PropertyName = "" ) => Register<TBase>(Property, new PropertyMetadata(DefaultValue, PropertyChangedCallback.GetNonTyped()), PropertyName);


    /// <inheritdoc cref="DependencyProperty.Register(string, Type, Type, PropertyMetadata)"/>
    /// <typeparam name="TBase">The type of the <see cref="DependencyObject"/> containing the <see cref="DependencyProperty"/>.</typeparam>
    /// <param name="Property">The dependency property.</param>
    /// <param name="DefaultValue">The initial value for the dependency property.</param>
    /// <param name="PropertyChangedCallback">Callback invoked when the property has changed</param>
    /// <param name="CoerceValueCallback">Callback invoked on the updating of the value.</param>
    /// <param name="PropertyName">The name of the property.</param>
    public static TypedDependencyProperty<T> Register<TBase>( T Property, T DefaultValue, TypedPropertyChangedCallback<T> PropertyChangedCallback, TypedCoerceValueCallback<T> CoerceValueCallback, [CallerArgumentExpression("Property")] string PropertyName = "" ) => Register<TBase>(Property, new PropertyMetadata(DefaultValue, PropertyChangedCallback.GetNonTyped(), CoerceValueCallback.GetNonTyped()), PropertyName);

    #endregion

}

public class TypedDependencyProperty<TBase, T> : TypedDependencyPropertyBase<T> where TBase : DependencyObject {

    #region Fields / Properties

    /// <summary>
    /// Gets the dependency object of this property.
    /// </summary>
    /// <value>
    /// The dependency object.
    /// </value>
    public TBase Object { get; set; }

    #endregion

    #region Constructors

    /// <inheritdoc cref="TypedDependencyPropertyBase{T}(DependencyProperty)"/>
    /// <param name="Object">The dependency object.</param>
    /// <param name="Property">The dependency property.</param>
    public TypedDependencyProperty( TBase Object, DependencyProperty Property ) : base(Property) => this.Object = Object;

    #endregion

    #region GetValue

    /// <inheritdoc cref="TypedDependencyPropertyBase{T}.GetValue(DependencyObject)"/>
    public T GetValue() => GetValue(Object);

    #endregion

    #region SetValue

    /// <inheritdoc cref="TypedDependencyPropertyBase{T}.ForceSetValue(DependencyObject, T)"/>
    public void ForceSetValue( T Value ) => ForceSetValue(Object, Value);

    /// <inheritdoc cref="TypedDependencyPropertyBase{T}.SetValue(DependencyObject, T)"/>
    public void SetValue( T Value ) => SetValue(Object, Value);

    /// <inheritdoc cref="TypedDependencyPropertyBase{T}.SetValue(DependencyObject, T, Func{T, T, bool})"/>
    public void SetValue( T Value, Func<T, T, bool> EqualityComparer ) => SetValue(Object, Value, EqualityComparer);

    /// <inheritdoc cref="TypedDependencyPropertyBase{T}.SetValue(DependencyObject, T, IEqualityComparer{T})"/>
    public void SetValue( T Value, IEqualityComparer<T> Comparer ) => SetValue(Object, Value, Comparer);

    /// <inheritdoc cref="TypedDependencyPropertyBase{T}.SetValue(DependencyObject, T, IEquatable{T})"/>
    public void SetValue( T Value, IEquatable<T> ValueComparer ) => SetValue(Object, Value, ValueComparer);

    /// <inheritdoc cref="TypedDependencyPropertyBase{T}.SetValue{TX}(DependencyObject, TX)"/>
    public void SetValue<TX>( TX Value ) where TX : T, IEquatable<T> => base.SetValue(Object, Value);

    #endregion

    #region Register

    /// <inheritdoc cref="Register(T, TBase, T, TypedPropertyChangedCallback{T}, TypedCoerceValueCallback{T}, string)"/>
    /// <param name="Property"><inheritdoc cref="Register(T, TBase, T, TypedPropertyChangedCallback{T}, TypedCoerceValueCallback{T}, string)"/></param>
    /// <param name="Base"><inheritdoc cref="Register(T, TBase, T, TypedPropertyChangedCallback{T}, TypedCoerceValueCallback{T}, string)"/></param>
    /// <param name="Metadata">The metadata to use if current type doesn't specify type-specific metadata.</param>
    /// <param name="PropertyName"><inheritdoc cref="Register(T, TBase, T, TypedPropertyChangedCallback{T}, TypedCoerceValueCallback{T}, string)"/></param>
    [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Used for type-safety and attributes.")]
    public static TypedDependencyProperty<TBase, T> Register( T Property, TBase Base, PropertyMetadata Metadata, [CallerArgumentExpression("Property")] string PropertyName = "" ) => new TypedDependencyProperty<TBase, T>(Base, DependencyProperty.Register(PropertyName, typeof(T), typeof(TBase), Metadata));

    /// <inheritdoc cref="Register(T, TBase, T, TypedPropertyChangedCallback{T}, TypedCoerceValueCallback{T}, string)"/>
    public static TypedDependencyProperty<TBase, T> Register( T Property, TBase Base, T DefaultValue = default!, [CallerArgumentExpression("Property")] string PropertyName = "" ) => Register(Property, Base, new PropertyMetadata(DefaultValue), PropertyName);


    /// <inheritdoc cref="Register(T, TBase, T, TypedPropertyChangedCallback{T}, TypedCoerceValueCallback{T}, string)"/>
    public static TypedDependencyProperty<TBase, T> Register( T Property, TBase Base, T DefaultValue, TypedPropertyChangedCallback<T> PropertyChangedCallback, [CallerArgumentExpression("Property")] string PropertyName = "" ) => Register(Property, Base, new PropertyMetadata(DefaultValue, PropertyChangedCallback.GetNonTyped()), PropertyName);

    /// <inheritdoc cref="DependencyProperty.Register(string, Type, Type, PropertyMetadata)"/>
    /// <typeparam name="TBase">The type of the <see cref="DependencyObject"/> containing the <see cref="DependencyProperty"/>.</typeparam>
    /// <param name="Property">The dependency property.</param>
    /// <param name="Base">The <see cref="DependencyObject"/> containing the <see cref="DependencyProperty"/>.</param>
    /// <param name="DefaultValue">The initial value for the dependency property.</param>
    /// <param name="PropertyChangedCallback">Callback invoked when the property has changed</param>
    /// <param name="CoerceValueCallback">Callback invoked on the updating of the value.</param>
    /// <param name="PropertyName">The name of the property.</param>
    public static TypedDependencyProperty<TBase, T> Register( T Property, TBase Base, T DefaultValue, TypedPropertyChangedCallback<T> PropertyChangedCallback, TypedCoerceValueCallback<T> CoerceValueCallback, [CallerArgumentExpression("Property")] string PropertyName = "" ) => Register(Property, Base, new PropertyMetadata(DefaultValue, PropertyChangedCallback.GetNonTyped(), CoerceValueCallback.GetNonTyped()), PropertyName);

    #endregion

}

/// <inheritdoc cref="CoerceValueCallback"/>
/// <param name="D">The dependency object where the change occurred.</param>
/// <param name="BaseValue">The original dependency property value to coerce.</param>
public delegate T TypedCoerceValueCallback<T>( DependencyObject D, T BaseValue );

/// <inheritdoc cref="PropertyChangedCallback"/>
/// <param name="D">The dependency object where the change occurred.</param>
/// <param name="E">The event arguments</param>
public delegate void TypedPropertyChangedCallback<T>( DependencyObject D, TypedDependencyPropertyChangedEventArgs<T> E );

/// <inheritdoc cref="DependencyPropertyChangedEventArgs"/>
/// <typeparam name="T">The underlying value type of the dependency property.</typeparam>
public struct TypedDependencyPropertyChangedEventArgs<T> {

    /// <summary>
    /// Initialises a new instance of the <see cref="TypedDependencyPropertyChangedEventArgs{T}"/> struct.
    /// </summary>
    /// <param name="Args">The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
    public TypedDependencyPropertyChangedEventArgs( DependencyPropertyChangedEventArgs Args ) : this(new TypedDependencyProperty<T>(Args.Property), (T)Args.OldValue, (T)Args.NewValue) { }

    /// <summary>
    /// Initialises a new instance of the <see cref="TypedDependencyPropertyChangedEventArgs{T}"/> struct.
    /// </summary>
    /// <param name="Property">The identifier for the dependency property where the value change occurred.</param>
    /// <param name="OldValue">The value of the property before the change.</param>
    /// <param name="NewValue">The value of the property after the change.</param>
    public TypedDependencyPropertyChangedEventArgs( TypedDependencyProperty<T> Property, T OldValue, T NewValue ) {
        this.Property = Property;
        this.OldValue = OldValue;
        this.NewValue = NewValue;
    }

    /// <inheritdoc cref="DependencyPropertyChangedEventArgs.Property"/>
    public readonly TypedDependencyProperty<T> Property;

    /// <inheritdoc cref="DependencyPropertyChangedEventArgs.OldValue"/>
    public readonly T OldValue;

    /// <inheritdoc cref="DependencyPropertyChangedEventArgs.NewValue"/>
    public readonly T NewValue;
}