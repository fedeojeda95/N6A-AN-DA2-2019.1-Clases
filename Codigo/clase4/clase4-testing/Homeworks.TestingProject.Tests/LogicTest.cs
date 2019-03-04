using Microsoft.VisualStudio.TestTools.UnitTesting;
using Homeworks.TestingProject;

namespace Homeworks.TestingProject.Tests
{
    [TestClass]
    public class LogicTest
    {
        private readonly ToTest toTest;

        public LogicTest()
        {
            toTest = new ToTest();
        }

        [TestMethod]
        public void FirstMethodIsOk()
        {
            int result = toTest.FirstMethod();
            Assert.AreEqual(5, result);
        }

        [TestMethod]
        public void SecondMethodIsOk()
        {
            int result = toTest.SecondMethod();
            Assert.AreEqual(6, result);
        }
    }
}