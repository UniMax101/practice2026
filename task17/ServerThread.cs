using System.Collections.Concurrent;

namespace Task17;

public class ServerThread
{
    private readonly BlockingCollection<ICommand> _queue = new();
    private readonly Thread _thread;
    private volatile bool _hardStopRequested;

    public IExceptionHandler? ExceptionHandler { get; set; }

    public ServerThread()
    {
        _thread = new Thread(Run) { IsBackground = true };
    }

    public void Start()
    {
        _thread.Start();
    }

    public void Enqueue(ICommand command)
    {
        try
        {
            _queue.Add(command);
        }
        catch (InvalidOperationException){}
    }

    public void Join()
    {
        _thread.Join();
    }

    private void Run()
    {
        foreach (var command in _queue.GetConsumingEnumerable())
        {
            try
            {
                command.Execute();
            }
            catch (Exception ex)
            {
                ExceptionHandler?.Handle(command, ex);
            }

            if (_hardStopRequested)
            {
                break;
            }
        }
    }

    internal void RequestHardStop()
    {
        EnsureRunningOnThisThread();
        _hardStopRequested = true;
        _queue.CompleteAdding();
    }

    internal void RequestSoftStop()
    {
        EnsureRunningOnThisThread();
        _queue.CompleteAdding();
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