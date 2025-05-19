using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;

namespace MineSweeper;

public sealed class GameTimer : ObservableObject
{
    private readonly DispatcherTimer _timer;
    private DateTimeOffset _startedAt;
    private bool _isRunning;

    public TimeSpan ElapsedTime { get; private set; }

    public GameTimer(TimeSpan interval)
    {
        _timer = new DispatcherTimer
        {
            Interval = interval
        };

        _timer.Tick += (_, _) => UpdateElapsedTime();
    }

    public void Start()
    {
        if (_isRunning) return;
        _startedAt = TimeProvider.System.GetUtcNow();
        _isRunning = true;
        _timer.Start();
    }

    public void Stop()
    {
        if (!_isRunning) return;
        UpdateElapsedTime();
        _isRunning = false;
        _timer.Stop();
    }

    private void UpdateElapsedTime()
    {
        if (!_isRunning)
            return;

        OnPropertyChanging(nameof(ElapsedTime));
        ElapsedTime = TimeProvider.System.GetUtcNow() - _startedAt;
        OnPropertyChanged(nameof(ElapsedTime));
    }
}