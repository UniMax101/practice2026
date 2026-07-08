public class Fighter : ISpaceship
{
    public int Speed { get; } = 100;
    public int FirePower { get; } = 40;
    public void MoveForward(){}
    public void Rotate(int angle){}
    public void Fire(){}
}