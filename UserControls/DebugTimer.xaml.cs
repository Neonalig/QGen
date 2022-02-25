using System.Windows;

namespace QGen.UserControls;

/// <summary>
/// Interaction logic for DebugTimer.xaml
/// </summary>
public partial class DebugTimer {

    public DebugTimer() {
        InitializeComponent();
    }

    /// <summary>
    /// Gets or sets the current time.
    /// </summary>
    /// <value>
    /// The current time.
    /// </value>
    public string CurrentTime {
        get => (string)GetValue(CurrentTimeProperty);
        set => SetValue(CurrentTimeProperty, value);
    }

    /// <summary>
    /// The dependency property for the <see cref="CurrentTime"/> property.
    /// </summary>
    public static readonly DependencyProperty CurrentTimeProperty = DependencyProperty.Register(nameof(CurrentTime), typeof(string), typeof(DebugTimer), new PropertyMetadata("00:00"));


}
