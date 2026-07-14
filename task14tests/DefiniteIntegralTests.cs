using Xunit;

public class DefiniteIntegralTests
{
    private readonly Func<double, double> X = x => x;
    private readonly Func<double, double> SIN = x => Math.Sin(x);

    [Fact]
    public void Integral_LinearFunction_SymmetricInterval()
    {
        Assert.Equal(0, DefiniteIntegral.Solve(-1, 1, X, 1e-4, 2), 1e-4);
    }

    [Fact]
    public void Integral_SinFunction_SymmetricInterval()
    {
        Assert.Equal(0, DefiniteIntegral.Solve(-1, 1, SIN, 1e-5, 8), 1e-4);
    }

    [Fact]
    public void Integral_LinearFunction_ZeroToFive()
    {
        Assert.Equal(12.5, DefiniteIntegral.Solve(0, 5, X, 1e-6, 8), 1e-5);
    }
}