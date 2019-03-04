using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;

using Homeworks.Domain;
using Homeworks.BusinessLogic;
using Homeworks.DataAccess;

namespace Homeworks.BusinessLogic.Tests
{
    [TestClass]
    public class HomeworksLogicTests
    {
        [TestMethod]
        public void CreateValidHomeworkTest()
        {
            Homework homework = new Homework();
            homework.DueDate = DateTime.Now;
            homework.Description = "testing description";

            var mock = new Mock<IRepository<Homework>>(MockBehavior.Strict);
            mock.Setup(m => m.Add(It.IsAny<Homework>()));
            mock.Setup(m => m.Save());

            var homeworksLogic = new HomeworksLogic(mock.Object);

            var result = homeworksLogic.Create(homework);

            mock.VerifyAll();
            Assert.AreEqual(homework.Description, result.Description);
            Assert.AreEqual(homework.DueDate, result.DueDate);
        }
    }
}
