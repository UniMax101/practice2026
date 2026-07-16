using System.Collections.Concurrent;
using Task17;

namespace Task19;

public class RoundRobinScheduler : IScheduler
{
    private readonly ConcurrentQueue<ICommand> _commands = new();

    public bool HasCommand() => !_commands.IsEmpty;

    public ICommand Select()
    {
        if (_commands.TryDequeue(out var command))
        {
            return command;
        }

        throw new InvalidOperationException("Планировщик пуст — нет команд для выбора.");
    }

    public void Add(ICommand cmd)
    {
        if (cmd == null) throw new ArgumentNullException(nameof(cmd));
        _commands.Enqueue(cmd);
    }
}