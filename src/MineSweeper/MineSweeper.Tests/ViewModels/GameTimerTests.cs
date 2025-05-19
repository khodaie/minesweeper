namespace MineSweeper.Tests.ViewModels;

public sealed class GameTimerTests
{
    private sealed class FakeTimeProvider : TimeProvider
    {
        private DateTimeOffset _now;
        public FakeTimeProvider(DateTimeOffset initial) => _now = initial;
        public void Advance(TimeSpan ts) => _now += ts;
        public override DateTimeOffset GetUtcNow() => _now;
    }

    [Fact]
    public void Start_SetsIsRunningAndStartedAt()
    {
        var fakeTime = new FakeTimeProvider(DateTimeOffset.UtcNow);
        var timer = new GameTimer(fakeTime, TimeSpan.FromMilliseconds(10));

        timer.Start();

        Assert.Equal(TimeSpan.Zero, timer.ElapsedTime);
    }

    [Fact]
    public void Start_DoesNothing_IfAlreadyRunning()
    {
        var fakeTime = new FakeTimeProvider(DateTimeOffset.UtcNow);
        var timer = new GameTimer(fakeTime, TimeSpan.FromMilliseconds(10));
        timer.Start();
        fakeTime.Advance(TimeSpan.FromSeconds(1));
        timer.Start(); // Should not reset
        Assert.Equal(TimeSpan.Zero, timer.ElapsedTime);
    }

    [Fact]
    public void Stop_StopsTimerAndUpdatesElapsedTime()
    {
        var fakeTime = new FakeTimeProvider(DateTimeOffset.UtcNow);
        var timer = new GameTimer(fakeTime, TimeSpan.FromMilliseconds(10));
        timer.Start();
        fakeTime.Advance(TimeSpan.FromSeconds(2));
        timer.Stop();
        fakeTime.Advance(TimeSpan.FromSeconds(1));
        Assert.True(timer.ElapsedTime >= TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void Stop_DoesNothing_IfNotRunning()
    {
        var fakeTime = new FakeTimeProvider(DateTimeOffset.UtcNow);
        var timer = new GameTimer(fakeTime, TimeSpan.FromMilliseconds(10));
        timer.Stop(); // Should not throw
        fakeTime.Advance(TimeSpan.FromSeconds(1));
        Assert.Equal(TimeSpan.Zero, timer.ElapsedTime);
    }

    [Fact]
    public void UpdateElapsedTime_UpdatesElapsedTime_WhenRunning()
    {
        var fakeTime = new FakeTimeProvider(DateTimeOffset.UtcNow);
        var timer = new GameTimer(fakeTime, TimeSpan.FromMilliseconds(10));
        timer.Start();
        fakeTime.Advance(TimeSpan.FromSeconds(3));
        var updateElapsedTimeMethod = typeof(GameTimer).GetMethod("UpdateElapsedTime",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        updateElapsedTimeMethod!.Invoke(timer, null);
        Assert.True(timer.ElapsedTime >= TimeSpan.FromSeconds(3));
    }

    [Fact]
    public void UpdateElapsedTime_DoesNothing_WhenNotRunning()
    {
        var fakeTime = new FakeTimeProvider(DateTimeOffset.UtcNow);
        var timer = new GameTimer(fakeTime, TimeSpan.FromMilliseconds(10));
        var updateElapsedTimeMethod = typeof(GameTimer).GetMethod("UpdateElapsedTime", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        updateElapsedTimeMethod!.Invoke(timer, null);
        Assert.Equal(TimeSpan.Zero, timer.ElapsedTime);
    }

    [Fact]
    public void UpdateElapsedTime_RaisesPropertyChanged_ForElapsedTime()
    {
        var fakeTime = new FakeTimeProvider(DateTimeOffset.UtcNow);
        var timer = new GameTimer(fakeTime, TimeSpan.FromMilliseconds(10));
        timer.Start();
        fakeTime.Advance(TimeSpan.FromSeconds(1));

        var changedProperties = new List<string?>();
        timer.PropertyChanged += (_, e) => changedProperties.Add(e.PropertyName);

        var updateElapsedTimeMethod = typeof(GameTimer).GetMethod("UpdateElapsedTime",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        updateElapsedTimeMethod!.Invoke(timer, null);

        Assert.Contains(nameof(timer.ElapsedTime), changedProperties);
    }

    [Fact]
    public void UpdateElapsedTime_RaisesPropertyChanging_ForElapsedTime()
    {
        var fakeTime = new FakeTimeProvider(DateTimeOffset.UtcNow);
        var timer = new GameTimer(fakeTime, TimeSpan.FromMilliseconds(10));
        timer.Start();
        fakeTime.Advance(TimeSpan.FromSeconds(1));

        var changingProperties = new List<string?>();
        timer.PropertyChanging += (_, e) => changingProperties.Add(e.PropertyName);

        var updateElapsedTimeMethod = typeof(GameTimer).GetMethod("UpdateElapsedTime",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        updateElapsedTimeMethod!.Invoke(timer, null);

        Assert.Contains(nameof(timer.ElapsedTime), changingProperties);
    }
}
