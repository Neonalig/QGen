using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

using SysWatch = System.Diagnostics.Stopwatch;

namespace QGen.UserControls;

#pragma warning disable IDE0079
#pragma warning disable CS0078 //Just use a half decent font

/// <summary>
/// Interaction logic for DebugTimer.xaml
/// </summary>
public partial class DebugStopwatch {
#pragma warning restore IDE0079 // Remove unnecessary suppression

    #region Constructors

    /// <summary>
    /// Initialises a new instance of the <see cref="DebugStopwatch"/> class.
    /// </summary>
    public DebugStopwatch() {
        DataContext = this;

        InitializeComponent();

        if ( DesignerProperties.GetIsInDesignMode(this) ) { return; }

        Started += DS => {
            _ = Dispatcher.Invoke(async () => {
                DebugStopwatch D = DS!;
                while ( D.IsRunning ) {
                    D.CurrentTime = GetDisplayString(D.Stopwatch.Elapsed);
                    await Task.Yield();
                }
            }, DispatcherPriority.ApplicationIdle);
        };
        Stopped += DS => {
            Dispatcher.Invoke(() => {
                DebugStopwatch D = DS!;
                D.CurrentTime = GetDisplayString(D.Stopwatch.Elapsed);
            }, DispatcherPriority.ApplicationIdle);
        };
    }

    static string GetTooltipString( TimeSpan TS ) {
        long Ticks = TS.Ticks;
        return Ticks switch {
            < 10000l                           => $"{Ticks:N0} ticks",
            < 10000l * 1000l                   => $"{TS.Milliseconds:##0}ms {TS.GetTicks():N0} ticks",
            < 10000l * 1000l * 60l             => $"{TS.Seconds}s {TS.Milliseconds:##0}ms {TS.GetTicks():N0} ticks",
            < 10000l * 1000l * 60l * 60l       => $"{TS.Minutes}m {TS.Seconds:#0}s {TS.Milliseconds:##0}ms {TS.GetTicks():N0} ticks",
            < 10000l * 1000l * 60l * 60l * 24l => $"{TS.Hours}h {TS.Minutes:#0}m {TS.Seconds:#0}s {TS.Milliseconds:##0}ms {TS.GetTicks():N0} ticks",
            _                                  => $"{TS.Days}d {TS.Hours}h {TS.Minutes:#0}m {TS.Seconds:#0}s {TS.Milliseconds:##0}ms {TS.GetTicks():N0} ticks",
        };
    }

    static string GetDisplayString( TimeSpan TS ) {
        long Ticks = TS.Ticks;
        return Ticks switch {
            < 10000l                           => $"{Ticks:N} ticks",
            < 10000l * 1000l                   => $"{Ticks / 10000:##0.###}ms",
            < 10000l * 1000l * 10l             => $"{TS.Seconds}s {TS.Milliseconds:000}ms",
            < 10000l * 1000l * 60l             => $"{TS.Seconds}.{TS.Milliseconds / 10:00}s", //Additional stage to ease readability (appears after first 10 seconds)
            < 10000l * 1000l * 60l * 60l       => $"{TS.Minutes}m {TS.Seconds:00}s",
            < 10000l * 1000l * 60l * 60l * 24l => $"{TS.Hours}h {TS.Minutes:00}m {TS.Seconds:00}s",
            _                                  => $"{TS:g}",
        };
    }

    #endregion

    #region Fields / Properties

    /// <summary>
    /// Gets the current time's tooltip representation.
    /// </summary>
    public string CurrentTimeTooltip => GetTooltipString(Stopwatch.Elapsed);

    /// <summary>
    /// Gets or sets the current time.
    /// </summary>
    /// <value>
    /// The current time.
    /// </value>
    public string CurrentTime {
        get => CurrentTimeProperty.GetValue(this);
        set => CurrentTimeProperty.SetValue(this, value);
    }

    /// <summary>
    /// Dependency property for the <see cref="CurrentTime"/> property.
    /// </summary>
    public static readonly TypedDependencyProperty<string> CurrentTimeProperty = TypedDependencyProperty<string>.Register<DebugStopwatch>("", "00:00.000", nameof(CurrentTime));

    /// <inheritdoc cref="SysWatch.IsRunning"/>
    public bool IsRunning {
        get => IsRunningProperty.GetValue(this);
        private set => IsRunningProperty.SetValue(this, value);
    }

    /// <summary>
    /// Dependency property for the <see cref="IsRunning"/> property.
    /// </summary>
    public static readonly TypedDependencyProperty<bool> IsRunningProperty = TypedDependencyProperty<bool>.Register<DebugStopwatch>(false, false, nameof(IsRunning));

    /// <summary>
    /// Gets or sets the value indicating whether user control is allowed.
    /// </summary>
    /// <value>
    /// If <see langword="true"/>, user control is allowed (clicking on the button starts/stops the stopwatch); otherwise user control is disallowed (clicking on the button copies the text to the system clipboard).
    /// </value>
    public bool AllowUserControl {
        get => AllowUserControlProperty.GetValue(this);
        private set => AllowUserControlProperty.SetValue(this, value);
    }

    /// <summary>
    /// Dependency property for the <see cref="AllowUserControl"/> property.
    /// </summary>
    public static readonly TypedDependencyProperty<bool> AllowUserControlProperty = TypedDependencyProperty<bool>.Register<DebugStopwatch>(false, true, nameof(AllowUserControl));

    /// <summary>
    /// Gets the stopwatch.
    /// </summary>
    /// <value>
    /// The stopwatch.
    /// </value>
    SysWatch Stopwatch { get; } = new SysWatch();

    /// <inheritdoc cref="SysWatch.Elapsed"/>
    public TimeSpan Elapsed => Stopwatch.Elapsed;

    /// <inheritdoc cref="SysWatch.ElapsedMilliseconds"/>
    public long ElapsedMilliseconds => Stopwatch.ElapsedMilliseconds;

    /// <inheritdoc cref="SysWatch.ElapsedTicks"/>
    public long ElapsedTicks => Stopwatch.ElapsedTicks;

    #endregion

    void UpdateDisplay() => CurrentTime = Stopwatch.Elapsed.ToString();

    #region Public Methods

    /// <inheritdoc cref="SysWatch.Start()"/>
    public void Start() {
        if ( IsRunning ) { return; }
        Stopwatch.Start();
        IsRunning = true;
        OnStarted();
    }

    /// <inheritdoc cref="SysWatch.Stop()"/>
    public void Stop() {
        if ( !IsRunning ) { return; }
        Debug.WriteLine($"Stopwatch finished with time: {ElapsedTicks} ticks ({ElapsedMilliseconds} ms).");
        Stopwatch.Stop();
        IsRunning = false;
        OnStopped();
    }

    /// <inheritdoc cref="SysWatch.Restart()"/>
    public void Restart() {
        if ( IsRunning ) {
            IsRunning = false;
            OnStopped();
        }
        Stopwatch.Restart();
        IsRunning = true;
        OnStarted();
    }

    /// <inheritdoc cref="SysWatch.Reset()"/>
    public void Reset() {
        bool WasRunning = IsRunning;
        Stopwatch.Reset();
        IsRunning = false;
        if ( WasRunning ) {
            OnStopped();
        }
        UpdateDisplay();
    }

    #endregion

    #region Events

    #region Started

    /// <summary>
    /// Raised when the <see cref="Stopwatch"/> is started.
    /// </summary>
    public event StartedEventHandler? Started;

    /// <summary>
    /// Represents the method that will handle the <see cref="Started"/> <see langword="event"/> on a <see cref="DebugStopwatch"/> instance.
    /// </summary>
    /// <param name="Sender">The <see langword="event"/> raiser.</param>
    public delegate void StartedEventHandler( DebugStopwatch? Sender );

    /// <summary>
    /// Invokes the <see cref="Started"/> <see langword="event"/>.
    /// </summary>
    public void OnStarted() => Started?.Invoke(this);

    #endregion

    #region Stopped

    /// <summary>
    /// Raised when the <see cref="Stopwatch"/> is stopped.
    /// </summary>
    public event StoppedEventHandler? Stopped;

    /// <summary>
    /// Represents the method that will handle the <see cref="Stopped"/> <see langword="event"/> on a <see cref="DebugStopwatch"/> instance.
    /// </summary>
    /// <param name="Sender">The <see langword="event"/> raiser.</param>
    public delegate void StoppedEventHandler( DebugStopwatch? Sender );

    /// <summary>
    /// Invokes the <see cref="Stopped"/> <see langword="event"/>.
    /// </summary>
    public void OnStopped() => Stopped?.Invoke(this);

    #endregion

    #endregion

    /// <summary>
    /// Raised when the <see cref="Button.Click"/> <see langword="event"/> is raised.
    /// </summary>
    /// <param name="Sender">The event raiser.</param>
    /// <param name="E">The raised event arguments.</param>
    void UserButton_OnClick( object Sender, RoutedEventArgs E ) {
        if ( !AllowUserControl || _ControlHeld ) {
            Clipboard.SetText(CurrentTime);
        } else if ( _ShiftHeld ) {
            Reset();
        } else if ( IsRunning ) {
            Stop();
        } else {
            Start();
        }
    }

    bool _ShiftHeld = false;
    bool _ControlHeld = false;
    void UserControl_PreviewKeyDown( object Sender, KeyEventArgs E ) {
        switch ( E.Key ) {
            case Key.LeftShift:
            case Key.RightShift:
                _ShiftHeld = true;
                break;
            case Key.LeftCtrl:
            case Key.RightCtrl:
                _ControlHeld = true;
                break;
        }
    }

    void UserControl_PreviewKeyUp( object Sender, KeyEventArgs E ) {
        switch ( E.Key ) {
            case Key.LeftShift:
            case Key.RightShift:
                _ShiftHeld = false;
                break;
            case Key.LeftCtrl:
            case Key.RightCtrl:
                _ControlHeld = false;
                break;
        }
    }

    void UserControl_ToolTipOpening( object Sender, ToolTipEventArgs E ) => ((FrameworkElement)E.Source).ToolTip = CurrentTimeTooltip;
}
