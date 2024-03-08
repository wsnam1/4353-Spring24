using Castle.Core.Logging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Project.Controllers;


[TestClass]
public class HomeControllerTests
{

    [TestMethod]
    public void Index_ReturnViewResult()
    {
        // Arrange
        // Mock the logger argument passed to the home controller with dependency injection to simulate a real logger
        var mockLogger = new Mock<ILogger<HomeController>>();
        var controller = new HomeController(mockLogger.Object);

        // Act
        var result = controller.Index();

        // Assert
        Assert.IsInstanceOfType(result, typeof(ViewResult));

    }
}