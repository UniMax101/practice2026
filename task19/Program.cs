using Task17;
using Task19;

var scheduler = new RoundRobinScheduler();
var serverThread = new LongOperationServerThread(scheduler);
ChartGenerator.GenerateCharts();
serverThread.Start();

for (int i = 1; i <= 3; i++)
{
    serverThread.Enqueue(new TestCommand(i));
}
serverThread.Enqueue(new Task19.SoftStopCommand(serverThread));
serverThread.Enqueue(new TestCommand(100)); 

serverThread.Join();
Console.WriteLine("Поток плавно остановлен (SoftStop). Все запущенные задачи завершены.");