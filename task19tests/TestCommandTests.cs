using System.Collections.Concurrent;
using Task17;
using Task19;
using Xunit;

namespace Task19Tests;

public class TestCommandTests
{
    [Fact]
    public void TestCommand_Is_Not_Completed_Before_Three_Calls()
    {
        var command = new TestCommand(1);

        Assert.False(command.IsCompleted);
        command.Execute();
        Assert.False(command.IsCompleted);
        command.Execute();
        Assert.False(command.IsCompleted);
    }

    [Fact]
    public void TestCommand_Is_Completed_After_Three_Calls()
    {
        var command = new TestCommand(1);

        command.Execute();
        command.Execute();
        command.Execute();

        Assert.True(command.IsCompleted);
    }

    [Fact]
    public void TestCommand_Does_Not_Execute_Beyond_Three_Calls_When_Used_With_Scheduler()
    {
        var scheduler = new RoundRobinScheduler();
        var serverThread = new LongOperationServerThread(scheduler);
        var executionLog = new ConcurrentQueue<string>();
        var completed = new ManualResetEventSlim(false);
        var executedCount = 0;

        serverThread.Start();

        var command = new LoggingTestCommand(1, log =>
        {
            executionLog.Enqueue(log);
            if (Interlocked.Increment(ref executedCount) == 3)
            {
                completed.Set();
            }
        });

        serverThread.Enqueue(command);

        Assert.True(completed.Wait(TimeSpan.FromSeconds(2)));
        Thread.Sleep(100);

        Assert.Equal(3, executionLog.Count);

        serverThread.Enqueue(new Task19.HardStopCommand(serverThread));
        serverThread.Join();
    }

    [Fact]
    public void Five_Commands_Execute_Three_Times_Each_In_Round_Robin_Order()
    {
        var scheduler = new Task19.RoundRobinScheduler();
        var serverThread = new Task19.LongOperationServerThread(scheduler);
        var executionOrder = new ConcurrentQueue<string>();
        var allCompleted = new ManualResetEventSlim(false);
        const int commandCount = 5;
        const int callsPerCommand = 3;

        serverThread.Start();

        for (int i = 1; i <= commandCount; i++)
        {
            int id = i;
            var command = new LoggingTestCommand(id, log =>
            {
                executionOrder.Enqueue(log);
            });

            serverThread.Enqueue(command);
        }
        SpinWait.SpinUntil(() => executionOrder.Count >= commandCount * callsPerCommand, TimeSpan.FromSeconds(3));

        var order = executionOrder.ToArray();
        Assert.Equal(commandCount * callsPerCommand, order.Length);
        var firstRound = order.Take(5).Select(ExtractId).ToArray();
        Assert.Equal(new[] { 1, 2, 3, 4, 5 }, firstRound);
        serverThread.Enqueue(new Task19.HardStopCommand(serverThread));
        serverThread.Join();
    }

    [Fact]
    public void HardStop_Stops_Thread_Even_With_Unfinished_Commands()
    {
        var scheduler = new Task19.RoundRobinScheduler();
        var serverThread = new Task19.LongOperationServerThread(scheduler);
        var barrier = new ManualResetEventSlim(false);
        var firstCallDone = new ManualResetEventSlim(false);
        var totalCalls = 0;

        serverThread.Start();

        var command = new LoggingTestCommand(1, _ =>
        {
            Interlocked.Increment(ref totalCalls);
            firstCallDone.Set();
            barrier.Wait();
        });

        serverThread.Enqueue(command);

        Assert.True(firstCallDone.Wait(TimeSpan.FromSeconds(2)));

        serverThread.Enqueue(new Task19.HardStopCommand(serverThread));
        barrier.Set();

        serverThread.Join();
        Assert.False(command.IsCompleted);
        Assert.Equal(1, totalCalls);
    }

    private static int ExtractId(string log)
    {
        var parts = log.Split(' ');
        return int.Parse(parts[1]);
    }

    private class LoggingTestCommand : ILongCommand
    {
        private readonly int _id;
        private readonly Action<string> _onExecute;
        private int _counter;

        public LoggingTestCommand(int id, Action<string> onExecute)
        {
            _id = id;
            _onExecute = onExecute;
        }

        public bool IsCompleted => _counter >= 3;

        public void Execute()
        {
            _counter++;
            _onExecute($"Поток {_id} вызов {_counter}");
        }
    }
}