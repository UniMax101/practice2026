using System.Collections.Concurrent;
using Task17;

namespace Task19;

public class LongOperationServerThread
{
    private readonly BlockingCollection<ICommand> _incomingQueue = new();
    private readonly IScheduler _scheduler;
    private readonly Thread _thread;
    private volatile bool _hardStopRequested;

    public IExceptionHandler? ExceptionHandler { get; set; }

    public LongOperationServerThread(IScheduler scheduler)
    {
        _scheduler = scheduler ?? throw new ArgumentNullException(nameof(scheduler));
        _thread = new Thread(Run) {IsBackground = true};
    }

    public void Start() => _thread.Start();

    public void Enqueue(ICommand command)
    {
        try
        {
            _incomingQueue.Add(command);
        }
        catch (InvalidOperationException){}
    }

    public void Join() => _thread.Join();

    private void Run()
{
    while (true)
    {
        if (_hardStopRequested)
        {
            break;
        }
        while (_incomingQueue.TryTake(out var incoming))
        {
            if (incoming is HardStopCommand || incoming is SoftStopCommand)
            {
                ExecuteSafely(incoming);
            }
            else
            {
                _scheduler.Add(incoming);
            }
        }
        if (_hardStopRequested)
        {
            break;
        }

        if (_scheduler.HasCommand())
        {
            var command = _scheduler.Select();
            ExecuteSafely(command);

            if (_hardStopRequested)
            {
                break;
            }

            if (command is ILongCommand longCommand && !longCommand.IsCompleted)
            {
                _scheduler.Add(command);
            }
        }
        else
        {
            ICommand next;
            try
            {
                next = _incomingQueue.Take();
            }
            catch (InvalidOperationException)
            {
                break;
            }
            if (next is HardStopCommand || next is SoftStopCommand)
            {
                ExecuteSafely(next);
            }
            else
            {
                _scheduler.Add(next);
            }
        }
    }
}

    private void ExecuteSafely(ICommand command)
    {
        try
        {
            command.Execute();
        }
        catch (Exception ex)
        {
            ExceptionHandler?.Handle(command, ex);
        }
    }

    internal void RequestHardStop()
    {
        EnsureRunningOnThisThread();
        _hardStopRequested = true;
        _incomingQueue.CompleteAdding();
    }

    internal void RequestSoftStop()
    {
        EnsureRunningOnThisThread();
        _incomingQueue.CompleteAdding();
    }

    private void EnsureRunningOnThisThread()
    {
        if (Thread.CurrentThread != _thread)
        {
            throw new InvalidOperationException(
                "HardStop/SoftStop могут быть выполнены только в потоке, который они должны остановить.");
        }
    }
}