using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Project.Controllers;



[TestClass]
public class HomeControllerTests
{

    private Mock<ILogger<HomeController>> _mockLogger;
    private HomeController _controller;

    [TestInitialize]
    public void Setup()
    {
        // Mock the logger argument passed to the home controller with dependency injection to simulate a real logger
        _mockLogger = new Mock<ILogger<HomeController>>();
        _controller = new HomeController(_mockLogger.Object);
    }

    [TestMethod]
    public void Index_ReturnsViewResult()
    {
        // Act
        var result = _controller.Index() as ViewResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result, typeof(ViewResult));
    }

    [TestMethod]
    public void Privacy_ReturnsViewResult()
    {
        // Act
        var result = _controller.Privacy() as ViewResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result, typeof(ViewResult));
    }

}