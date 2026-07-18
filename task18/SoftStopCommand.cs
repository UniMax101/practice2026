using Task17;

namespace Task18;

public class SoftStopCommand : ICommand
{
    private readonly LongOperationServerThread _serverThread;

    public SoftStopCommand(LongOperationServerThread serverThread)
    {
        _serverThread = serverThread;
    }

    public void Execute() => _serverThread.RequestSoftStop();
}