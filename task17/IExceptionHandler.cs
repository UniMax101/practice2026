namespace Task17;

public interface IExceptionHandler
{
    void Handle(ICommand command, Exception exception);
}