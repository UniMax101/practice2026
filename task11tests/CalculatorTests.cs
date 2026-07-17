using Xunit;

public class CalculatorTests
{
    private readonly ICalculator _calc;

    public CalculatorTests()
    {
        _calc = new CalculatorGenerator().Create();
    }

    [Fact]
    public void Add_ShouldReturnSum()
    {
        Assert.Equal(5, _calc.Add(2, 3));
    }

    [Fact]
    public void Minus_ShouldReturnDifference()
    {
        Assert.Equal(1, _calc.Minus(3, 2));
    }

    [Fact]
    public void Mul_ShouldReturnProduct()
    {
        Assert.Equal(6, _calc.Mul(2, 3));
    }

    [Fact]
    public void Div_ShouldReturnQuotient()
    {
        Assert.Equal(2, _calc.Div(6, 3));
    }
}
