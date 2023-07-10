[TestFixture]
public class CalculatorTests
{
    private Calculator calculator;

    [SetUp]
    public void Setup()
    {
        calculator = new Calculator();
    }

    [Test]
    public void Add_TwoIntegers_ReturnsSum()
    {
        int result = calculator.Add(2, 3);
        Assert.AreEqual(5, result);
    }

    [Test]
    public void Subtract_TwoIntegers_ReturnsDifference()
    {
        int result = calculator.Subtract(5, 2);
        Assert.AreEqual(3, result);
    }

    [Test]
    public void Multiply_TwoIntegers_ReturnsProduct()
    {
        int result = calculator.Multiply(3, 4);
        Assert.AreEqual(12, result);
    }

    [Test]
    public void Divide_TwoIntegers_ReturnsQuotient()
    {
        int result = calculator.Divide(10, 2);
        Assert.AreEqual(5, result);
    }

    [Test]
    public void Divide_ByZero_ThrowsException()
    {
        Assert.Throws<ArgumentException>(() => calculator.Divide(10, 0));
    }
}