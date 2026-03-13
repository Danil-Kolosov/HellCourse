using Xunit;


public class SimpleTest
{
    [Fact]
    public void Test1()
    {
        // Этот тест всегда проходит
        Assert.True(true);
    }

    [Fact]
    public void Test2()
    {
        int testVar = buildingCompany.Pages.Employees.Testing.Sum(1, 2);
        Assert.Equal(3, testVar);
    }
}