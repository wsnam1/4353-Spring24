using Microsoft.AspNetCore.Mvc;
using Moq;
using Project.Controllers;
using Project.Models;


[TestClass]
public class ProfileControllerTests
{
    private ProfileController _controller;

    [TestInitialize]
    public void Setup()
    {
        _controller = new ProfileController();
    }

    [TestMethod]
    public void Index_ReturnsViewResult()
    {
        // Act
        var result = _controller.Index();

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result, typeof(ViewResult));
    }
}
