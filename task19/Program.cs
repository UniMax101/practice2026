using Task17;
using Task19;

var scheduler = new RoundRobinScheduler();
var serverThread = new LongOperationServerThread(scheduler);

serverThread.Start();

for (int i = 1; i <= 5; i++)
{
    serverThread.Enqueue(new TestCommand(i));
}
Thread.Sleep(500);

serverThread.Enqueue(new Task19.HardStopCommand(serverThread));
serverThread.Join();

Console.WriteLine("Поток остановлен (HardStop).");