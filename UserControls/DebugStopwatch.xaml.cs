using SysWatch = System.Diagnostics.Stopwatch;

namespace QGen.UserControls;

/// <summary>
/// Interaction logic for DebugTimer.xaml
/// </summary>
public partial class DebugStopwatch {

    /// <summary>
    /// Initialises a new instance of the <see cref="DebugStopwatch"/> class.
    /// </summary>
    public DebugStopwatch() {
        CurrentTimeProperty = this.Register(CurrentTime, "00:00.000");
        IsRunningProperty = this.Register(IsRunning, false);

        InitializeComponent();
    }

    /// <summary>
    /// Gets or sets the current time.
    /// </summary>
    /// <value>
    /// The current time.
    /// </value>
    public string CurrentTime {
        get => CurrentTimeProperty.GetValue();
        set => CurrentTimeProperty.SetValue(value);
    }

    /// <summary>
    /// Dependency property for the <see cref="CurrentTime"/> property.
    /// </summary>
    public static TypedDependencyProperty<DebugStopwatch, string> CurrentTimeProperty { get; private set; } = null!;

    /// <summary>
    /// Gets the stopwatch.
    /// </summary>
    /// <value>
    /// The stopwatch.
    /// </value>
    public SysWatch Stopwatch { get; } = new SysWatch();

    /// <inheritdoc cref="SysWatch.Start()"/>
    public void Start() {
        Stopwatch.Start();
        IsRunning = true;
    }

    /// <inheritdoc cref="SysWatch.Stop()"/>
    public void Stop() {
        Stopwatch.Stop();
        IsRunning = false;
    }

    /// <inheritdoc cref="SysWatch.Restart()"/>
    public void Restart() {
        IsRunning = false;
        Stopwatch.Restart();
        IsRunning = true;
    }

    /// <inheritdoc cref="SysWatch.Reset()"/>
    public void Reset() {
        Stopwatch.Reset();
        IsRunning = false;
    }

    /// <inheritdoc cref="SysWatch.Elapsed"/>
    public TimeSpan Elapsed => Stopwatch.Elapsed;

    /// <inheritdoc cref="SysWatch.ElapsedMilliseconds"/>
    public long ElapsedMilliseconds => Stopwatch.ElapsedMilliseconds;

    /// <inheritdoc cref="SysWatch.ElapsedTicks"/>
    public long ElapsedTicks => Stopwatch.ElapsedTicks;

    /// <inheritdoc cref="SysWatch.IsRunning"/>
    public bool IsRunning {
        get => IsRunningProperty.GetValue();
        private set => IsRunningProperty.SetValue(value);
    }

    /// <summary>
    /// Dependency property for the <see cref="IsRunning"/> property.
    /// </summary>
    public static TypedDependencyProperty<DebugStopwatch, bool> IsRunningProperty { get; private set; } = null!;



}
