using System.Collections.Concurrent;
using Task17;
using Xunit;

namespace Task17Tests;

public class ServerThreadTests
{
    private class ActionCommand : ICommand
    {
        private readonly Action _action;

        public ActionCommand(Action action)
        {
            _action = action;
        }

        public void Execute() => _action();
    }

    [Fact]
    public void SoftStop_Drains_All_Queued_Commands_Before_Stopping()
    {
        var serverThread = new ServerThread();
        var executedOrder = new ConcurrentQueue<int>();

        serverThread.Start();

        for (int i = 0; i < 5; i++)
        {
            int captured = i;
            serverThread.Enqueue(new ActionCommand(() => executedOrder.Enqueue(captured)));
        }

        serverThread.Enqueue(new SoftStopCommand(serverThread));
        serverThread.Join();

        Assert.Equal(new[] { 0, 1, 2, 3, 4 }, executedOrder.ToArray());
    }

    [Fact]
    public void HardStop_Stops_Immediately_Ignoring_Remaining_Queued_Commands()
    {
        var serverThread = new ServerThread();
        var executedOrder = new ConcurrentQueue<int>();
        var barrier = new ManualResetEventSlim(false);

        serverThread.Start();
        serverThread.Enqueue(new ActionCommand(() =>
        {
            executedOrder.Enqueue(0);
            barrier.Wait();
        }));

        serverThread.Enqueue(new HardStopCommand(serverThread));

        for (int i = 1; i <= 5; i++)
        {
            int captured = i;
            serverThread.Enqueue(new ActionCommand(() => executedOrder.Enqueue(captured)));
        }

        barrier.Set();

        serverThread.Join();
        Assert.Equal(new[] { 0 }, executedOrder.ToArray());
    }

    [Fact]
    public void HardStop_Called_From_Wrong_Thread_Throws()
    {
        var serverThread = new ServerThread();
        serverThread.Start();

        var hardStop = new HardStopCommand(serverThread);

        Assert.Throws<InvalidOperationException>(() => hardStop.Execute());
        var executed = new ManualResetEventSlim(false);
        serverThread.Enqueue(new ActionCommand(() => executed.Set()));

        Assert.True(executed.Wait(TimeSpan.FromSeconds(2)));
        serverThread.Enqueue(new SoftStopCommand(serverThread));
        serverThread.Join();
    }

    [Fact]
    public void SoftStop_Called_From_Wrong_Thread_Throws()
    {
        var serverThread = new ServerThread();
        serverThread.Start();

        var softStop = new SoftStopCommand(serverThread);

        Assert.Throws<InvalidOperationException>(() => softStop.Execute());

        var executed = new ManualResetEventSlim(false);
        serverThread.Enqueue(new ActionCommand(() => executed.Set()));

        Assert.True(executed.Wait(TimeSpan.FromSeconds(2)));

        serverThread.Enqueue(new SoftStopCommand(serverThread));
        serverThread.Join();
    }
}