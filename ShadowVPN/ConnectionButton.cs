using System;
using System.Threading;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Styling;
using ShadowVPN.ViewModels;

namespace ShadowVPN;

public class ConnectionButton : Button
{
    private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
    private Arc _arc = null!;

    public ConnectionButton()
    {
        Classes.Add("disconnected");
    }

    public static readonly StyledProperty<ConnectionStatus> StatusProperty =
        AvaloniaProperty.Register<ConnectionButton, ConnectionStatus>(nameof(Status));

    public ConnectionStatus Status
    {
        get => GetValue(StatusProperty);
        set => SetValue(StatusProperty, value);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property != StatusProperty) return;
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource.Dispose();
        _cancellationTokenSource = new CancellationTokenSource();

        Classes.Clear();
        switch ((ConnectionStatus)change.NewValue!)
        {
            case ConnectionStatus.Disconnected:
                Classes.Add("disconnected");
                break;
            case ConnectionStatus.Connecting:
                Classes.Add("connecting");
                var connectingAnimation = new Animation()
                {
                    Duration = TimeSpan.FromSeconds(1),
                    Children =
                    {
                        new KeyFrame()
                        {
                            Cue = new Cue(0),
                            Setters =
                            {
                                new Setter() { Property = Arc.SweepAngleProperty, Value = 0d }
                            }
                        },
                        new KeyFrame()
                        {
                            Cue = new Cue(1),
                            Setters =
                            {
                                new Setter() { Property = Arc.SweepAngleProperty, Value = 45d }
                            }
                        }
                    }
                };

                _ = connectingAnimation.RunAsync(_arc, _cancellationTokenSource.Token);
                break;
            case ConnectionStatus.Connected:
                Classes.Add("connected");
                var connectedAnimation = new Animation()
                {
                    Duration = TimeSpan.FromSeconds(1),
                    Children =
                    {
                        new KeyFrame()
                        {
                            Cue = new Cue(0),
                            Setters =
                            {
                                new Setter() { Property = Arc.SweepAngleProperty, Value = 0d }
                            }
                        },
                        new KeyFrame()
                        {
                            Cue = new Cue(1),
                            Setters =
                            {
                                new Setter() { Property = Arc.SweepAngleProperty, Value = 360d }
                            }
                        }
                    }
                };

                _ = connectedAnimation.RunAsync(_arc, _cancellationTokenSource.Token);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _arc = e.NameScope.Find<Arc>("PART_Arc")!;
    }
}