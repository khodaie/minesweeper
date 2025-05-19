using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;

namespace MineSweeper;

public sealed class GameTimer : ObservableObject
{
    private readonly TimeProvider _timeProvider;
    private readonly DispatcherTimer _timer;
    private DateTimeOffset _startedAt;
    private bool _isRunning;

    public TimeSpan ElapsedTime { get; private set; }

    public GameTimer(TimeProvider timeProvider, TimeSpan interval)
    {
        _timeProvider = timeProvider;
        _timer = new DispatcherTimer
        {
            Interval = interval
        };

        _timer.Tick += (_, _) => UpdateElapsedTime();
    }

    public void Start()
    {
        if (_isRunning) return;
        _startedAt = _timeProvider.GetUtcNow();
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
        ElapsedTime = _timeProvider.GetUtcNow() - _startedAt;
        OnPropertyChanged(nameof(ElapsedTime));
    }
}