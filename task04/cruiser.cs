public class Cruiser : ISpaceship
{
    public int Speed { get; } = 50;
    public int FirePower { get; } = 100;
    public void MoveForward(){}
    public void Rotate(int angle){}
    public void Fire(){}
}