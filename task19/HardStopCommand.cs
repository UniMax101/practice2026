using Task17;

namespace Task19;

public class HardStopCommand : ICommand
{
    private readonly LongOperationServerThread _serverThread;

    public HardStopCommand(LongOperationServerThread serverThread)
    {
        _serverThread = serverThread;
    }

    public void Execute() => _serverThread.RequestHardStop();
}