using Task17;

namespace Task19;

public class TestCommand : ILongCommand
{
    private readonly int _id;
    private int _counter;
    private const int MaxCalls = 3;

    public TestCommand(int id)
    {
        _id = id;
    }

    public bool IsCompleted => _counter >= MaxCalls;

    public void Execute()
    {
        _counter++;
        Console.WriteLine($"Поток {_id} вызов {_counter}");
    }
}