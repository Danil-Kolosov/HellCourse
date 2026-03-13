using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using buildingCompany;
namespace buildingCompany
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            int testVar = buildingCompany.Pages.Employees.Testing.Sum(1, 2);
            Assert.AreEqual(3, testVar);
        }
    }
}
