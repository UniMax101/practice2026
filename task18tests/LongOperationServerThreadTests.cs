using System.Collections.Concurrent;
using Task17;
using Task18;
using Xunit;

namespace Task18Tests;

public class LongOperationServerThreadTests
{
    private class InstantCommand : ICommand
    {
        private readonly Action _action;
        public InstantCommand(Action action) => _action = action;
        public void Execute() => _action();
    }

    private class SteppedLongCommand : ILongCommand
    {
        private readonly int _totalSteps;
        private readonly Action<int> _onStep;
        private int _currentStep;

        public SteppedLongCommand(int totalSteps, Action<int> onStep)
        {
            _totalSteps = totalSteps;
            _onStep = onStep;
        }

        public bool IsCompleted => _currentStep >= _totalSteps;

        public void Execute()
        {
            _currentStep++;
            _onStep(_currentStep);
        }
    }

    [Fact]
    public void LongCommand_Executes_In_Multiple_Steps_Until_Completed()
    {
        var scheduler = new RoundRobinScheduler();
        var serverThread = new LongOperationServerThread(scheduler);
        var steps = new ConcurrentQueue<int>();
        var completed = new ManualResetEventSlim(false);

        serverThread.Start();

        var longCommand = new SteppedLongCommand(3, step =>
        {
            steps.Enqueue(step);
            if (step == 3) completed.Set();
        });

        serverThread.Enqueue(longCommand);

        Assert.True(completed.Wait(TimeSpan.FromSeconds(2)));
        Assert.Equal(new[] { 1, 2, 3 }, steps.ToArray());

        serverThread.Enqueue(new Task18.SoftStopCommand(serverThread));
        serverThread.Join();
    }

    [Fact]
    public void RoundRobin_Interleaves_Multiple_Long_Commands()
    {
        var scheduler = new RoundRobinScheduler();
        var serverThread = new LongOperationServerThread(scheduler);
        var executionOrder = new ConcurrentQueue<string>();
        var completedCount = 0;
        var allCompleted = new ManualResetEventSlim(false);
        var totalCommands = 2;

        void OnCommandCompleted()
        {
            if (Interlocked.Increment(ref completedCount) == totalCommands)
            {
                allCompleted.Set();
            }
        }

        serverThread.Start();

        var commandA = new SteppedLongCommand(3, step =>
        {
            executionOrder.Enqueue($"A{step}");
            if (step == 3) OnCommandCompleted();
        });

        var commandB = new SteppedLongCommand(3, step =>
        {
            executionOrder.Enqueue($"B{step}");
            if (step == 3) OnCommandCompleted();
        });

        serverThread.Enqueue(commandA);
        serverThread.Enqueue(commandB);

        Assert.True(allCompleted.Wait(TimeSpan.FromSeconds(2)));
        var order = executionOrder.ToArray();
        Assert.Equal(new[] { "A1", "B1", "A2", "B2", "A3", "B3" }, order);

        serverThread.Enqueue(new Task18.SoftStopCommand(serverThread));
        serverThread.Join();
    }

    [Fact]
    public void New_Command_Is_Picked_Up_While_Long_Command_Is_Running()
    {
        var scheduler = new RoundRobinScheduler();
        var serverThread = new LongOperationServerThread(scheduler);
        var executionOrder = new ConcurrentQueue<string>();
        var newCommandExecuted = new ManualResetEventSlim(false);
        var longCommandFirstStepDone = new ManualResetEventSlim(false);

        serverThread.Start();

        var longCommand = new SteppedLongCommand(5, step =>
        {
            executionOrder.Enqueue($"long{step}");
            if (step == 1) longCommandFirstStepDone.Set();
            Thread.Sleep(20);
        });

        serverThread.Enqueue(longCommand);

        Assert.True(longCommandFirstStepDone.Wait(TimeSpan.FromSeconds(2)));

        serverThread.Enqueue(new InstantCommand(() =>
        {
            executionOrder.Enqueue("new");
            newCommandExecuted.Set();
        }));
        Assert.True(newCommandExecuted.Wait(TimeSpan.FromSeconds(2)));

        serverThread.Enqueue(new Task18.SoftStopCommand(serverThread));
        serverThread.Join();
    }

    [Fact]
    public void No_Deadlock_When_Enqueueing_Many_Commands_While_Long_Command_Runs()
    {
        var scheduler = new RoundRobinScheduler();
        var serverThread = new LongOperationServerThread(scheduler);
        var completedCount = 0;
        var allDone = new ManualResetEventSlim(false);
        const int commandCount = 50;

        serverThread.Start();

        var longCommand = new SteppedLongCommand(commandCount + 1, _ => Thread.Sleep(1));
        serverThread.Enqueue(longCommand);

        for (int i = 0; i < commandCount; i++)
        {
            serverThread.Enqueue(new InstantCommand(() =>
            {
                if (Interlocked.Increment(ref completedCount) == commandCount)
                {
                    allDone.Set();
                }
            }));
        }

        Assert.True(allDone.Wait(TimeSpan.FromSeconds(5)));

        serverThread.Enqueue(new Task18.SoftStopCommand(serverThread));
        serverThread.Join();
    }

    [Fact]
    public void SoftStop_Waits_For_Long_Command_To_Complete()
    {
        var scheduler = new RoundRobinScheduler();
        var serverThread = new LongOperationServerThread(scheduler);
        var stepsCompleted = 0;

        serverThread.Start();

        var longCommand = new SteppedLongCommand(3, _ => Interlocked.Increment(ref stepsCompleted));
        serverThread.Enqueue(longCommand);
        serverThread.Enqueue(new Task18.SoftStopCommand(serverThread));

        serverThread.Join();

        Assert.Equal(3, stepsCompleted);
    }

    [Fact]
    public void HardStop_Interrupts_Long_Command_Immediately()
    {
        var scheduler = new RoundRobinScheduler();
        var serverThread = new LongOperationServerThread(scheduler);
        var stepsCompleted = 0;
        var barrier = new ManualResetEventSlim(false);

        serverThread.Start();

        var longCommand = new SteppedLongCommand(10, step =>
        {
            Interlocked.Increment(ref stepsCompleted);
            if (step == 1) barrier.Wait();
        });

        serverThread.Enqueue(longCommand);
        serverThread.Enqueue(new Task18.HardStopCommand(serverThread));

        barrier.Set();
        serverThread.Join();
        Assert.True(stepsCompleted < 10);
    }
}
