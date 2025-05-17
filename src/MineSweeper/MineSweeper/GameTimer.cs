using Timer = System.Timers.Timer;

namespace MineSweeper;

public sealed class GameTimer
{
    private readonly Timer _timer;
    private DateTime _startedAt;
    private TimeSpan _elapsed;
    private bool _isRunning;

    public event Action? Elapsed;

    public TimeSpan ElapsedTime => _isRunning ? DateTime.UtcNow - _startedAt : _elapsed;

    public GameTimer(TimeSpan interval)
    {
        _timer = new Timer(interval);
        _timer.Elapsed += (_, _) => Elapsed?.Invoke();
    }

    public void Start()
    {
        if (_isRunning) return;
        _startedAt = DateTime.UtcNow;
        _isRunning = true;
        _timer.Start();
    }

    public void Stop()
    {
        if (!_isRunning) return;
        _elapsed = DateTime.UtcNow - _startedAt;
        _isRunning = false;
        _timer.Stop();
    }
}