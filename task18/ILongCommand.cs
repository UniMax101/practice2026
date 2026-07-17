using Task17;

namespace Task18;

public interface ILongCommand : ICommand
{
    bool IsCompleted { get; }
}