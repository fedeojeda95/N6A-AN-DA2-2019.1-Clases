using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;

using Homeworks.Domain;
using Homeworks.WebApi.Controllers;
using Homeworks.BusinessLogic;

namespace Homeworks.WebApi.Tests
{
    [TestClass]
    public class HomeworksControllerTests
    {
        [TestMethod]
        public void CreateValidHomework()
        {
            Homework homework = new Homework();
            homework.DueDate = DateTime.Now;
            homework.Description = "testing description";

            var mock = new Mock<IHomeworksLogic>(MockBehavior.Strict);
            mock.Setup(m => m.Create(It.IsAny<Homework>())).Returns(homework); 
            var controller = new HomeworksController(mock.Object);

            var result = controller.Post(homework);
            var createdResult = result as CreatedAtRouteResult;
            var model = createdResult.Value as Homework;

            mock.VerifyAll();
            Assert.AreEqual(homework.Description, model.Description);
            Assert.AreEqual(homework.DueDate, model.DueDate);
        }

        [TestMethod]
        public void CreateInvalidHomeworkBadRequestTest()
        {
            var mock = new Mock<IHomeworksLogic>(MockBehavior.Strict);
            mock.Setup(m => m.Create(null)).Throws(new ArgumentException());
            var controller = new HomeworksController(mock.Object);

            var result = controller.Post(null);

            mock.VerifyAll();
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
        }

    }
}
