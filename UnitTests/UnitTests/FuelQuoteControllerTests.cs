
using Microsoft.AspNetCore.Mvc;
using Moq;
using Project.Controllers;
using Project.Models;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using MySqlX.XDevAPI.Common;



[TestClass]
public class FuelQuoteControllerTests
{
    // Declare a variable of type MockInitializer class
    private MockInitializer _mockInitializer;
    // Declare a variable of type FuelQuoteController class
    private FuelQuoteController _controller;

    [TestInitialize]
    public void Setup()
    {
        // Assign a new instance of the MockIntializer class to the _mockInitializer variable
        _mockInitializer = new MockInitializer();
        _controller = new FuelQuoteController(_mockInitializer.MockContext.Object, _mockInitializer.MockUserManager.Object);  
    }

    // Create method for MockDbSet to avoid redundancy
    public static Mock<DbSet<T>> CreateMockDbSet<T>(IQueryable<T> data) where T : class
    {
        var mockDbSet = new Mock<DbSet<T>>();
        mockDbSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(data.Provider);
        mockDbSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(data.Expression);
        mockDbSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(data.ElementType);
        mockDbSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());
        return mockDbSet;
    }



    // GET of Index method with userProfile != null
    [TestMethod]
    public void Index_ReturnsViewResult_ViewBagAddress1()
    {
        // Arrange

        // Clear the model state to prevent any unexpected behaviour in our test
        _controller.ModelState.Clear();

        var userId = "TestId";
        var address1 = "123 Main St";

        _mockInitializer.MockUserManager
            .Setup(um => um.GetUserId(It.IsAny<ClaimsPrincipal>()))
            .Returns(userId);

        // Set up the mock DbSet for UserProfiles
        var mockUserProfiles = new List<UserProfile>
        {
            new UserProfile
            {
                UserId = userId,
                FullName = "John Doe",
                Address1 = address1,
                Address2 = "Apt 4",
                City = "Anytown",
                State = "CA",
                Zipcode = "12345" // Ensure the zipcode adheres to the specified StringLength attribute     
            }
        }.AsQueryable();

        var mockUserProfilesDbSet = CreateMockDbSet(mockUserProfiles);

        // Set up the mock DbSet on the mock ApplicationDbContext
        _mockInitializer.MockContext
            .Setup(c => c.UserProfiles)
            .Returns(mockUserProfilesDbSet.Object);

        // Act 
        var result = _controller.Index() as ViewResult;

        // Assert
        Assert.IsNotNull(result);
        // ViewBag is rendered to the view dynamically, the return statement ViewResult does not contain a ViewBag so we must access through _controller
        Assert.AreEqual(address1, _controller.ViewBag.DeliveryAddress);
        Assert.IsInstanceOfType(result, typeof(ViewResult));
        
    }

    // GET of Index method with userProfile == null
    [TestMethod]
    public void Index_ReturnsViewResult_ViewBagNull()
    {
        // Arrange

        // Clear the model state to prevent any unexpected behaviour in our test
        _controller.ModelState.Clear();

        var userId = "TestId";

        _mockInitializer.MockUserManager
            .Setup(um => um.GetUserId(It.IsAny<ClaimsPrincipal>()))
            .Returns(userId);

        // Set up the mock DbSet for UserProfiles
        var mockUserProfiles = new List<UserProfile>
        {
            new UserProfile { }
        }.AsQueryable();

        var mockUserProfilesDbSet = CreateMockDbSet(mockUserProfiles);

        // Set up the mock DbSet on the mock ApplicationDbContext
        _mockInitializer.MockContext
            .Setup(c => c.UserProfiles)
            .Returns(mockUserProfilesDbSet.Object);

        // Act 
        var result = _controller.Index() as ViewResult;

        // Assert
        Assert.IsNotNull(result);
        // ViewBag is rendered to the view dynamically, the return statement ViewResult does not contain a ViewBag so we must access through _controller
        Assert.AreEqual("Null", _controller.ViewBag.DeliveryAddress);
        Assert.IsInstanceOfType(result, typeof(ViewResult));

    }


    // POST of Index method
    [TestMethod]
    public async Task Index_ValidModel_RedirectsToHomeIndex()
    {
        // Arrange
        // Clear the model state to prevent any unexpected behaviour in our test
        _controller.ModelState.Clear();

        // Create a new FuelHistory model state with valid data
        var fuelQuote = new FuelHistory 
        {
            // Populate with valid data
            GallonsRequested = 100,
            DeliveryAddress = "123 Main St",
            DeliveryDate = "2022-01-01",
            SuggestedPrice = 1.99m,
            TotalAmountDue = 199
        };

        // Simulate a GetUserId operation with a mock userManager. Make it return the fake id "TestId"
        _mockInitializer.MockUserManager
            .Setup(um => um.GetUserId(It.IsAny<ClaimsPrincipal>()))
            .Returns("TestId");

        // Simulate a SaveChangesAsync operation to a mock db context. Make it return the integer 1, indicating 1 record was changed or added
        _mockInitializer.MockContext
            .Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(1));

        // Act
        // Call the Index method
        var result = await _controller.Index(fuelQuote);

        // Assert
        Assert.IsNotNull(result);
        // Check if result is of type RedirectToActionResult
        Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));

        // Change the type of result to access the ControllerName and ActionName properties
        var redirectResult = (RedirectToActionResult) result;

        // Check if the method redirects to the correct controller and action
        Assert.AreEqual("Home", redirectResult.ControllerName);
        Assert.AreEqual("Index", redirectResult.ActionName);

    }

    // POST of Index method with invalid model returns view result and userProfile is set to != null so Viewbag returns Address1
    [TestMethod]
    public async Task Index_InvalidModel_ReturnsViewResult_ViewBagAddress1()
    {

        // Arrange
        _controller.ModelState.Clear();

        _controller.ModelState.AddModelError("Error", "Model state is invalid");

        var userId = "TestId";
        var address1 = "123 Main St";

        var fuelQuote = new FuelHistory
        {
            // Populating with potentially valid data, but ModelState is explicitly made invalid in SetUp
            GallonsRequested = 100,
            DeliveryAddress = "123 Main St",
            DeliveryDate = "2022-01-01",
            SuggestedPrice = 2.5m,
            TotalAmountDue = 250m
        };

        // Set up the mock DbSet for UserProfiles
        var mockUserProfiles = new List<UserProfile>
        {
            new UserProfile
            {
                UserId = userId,
                FullName = "John Doe",
                Address1 = address1,
                Address2 = "Apt 4",
                City = "Anytown",
                State = "CA",
                Zipcode = "12345" // Ensure the zipcode adheres to the specified StringLength attribute     
            }
        }.AsQueryable();

        var mockUserProfilesDbSet = CreateMockDbSet(mockUserProfiles);

        _mockInitializer.MockUserManager
            .Setup(um => um.GetUserId(It.IsAny<ClaimsPrincipal>()))
            .Returns(userId);
        _mockInitializer.MockContext
            .Setup(c => c.UserProfiles)
            .Returns(mockUserProfilesDbSet.Object);

        // Act
        var result = await _controller.Index(fuelQuote) as ViewResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.IsFalse(_controller.ModelState.IsValid); // Ensure model state is indeed invalid as intended
        Assert.AreEqual(address1, _controller.ViewBag.DeliveryAddress);
        Assert.AreEqual(fuelQuote, result?.Model); // Check if the method returns the correct view with the original model
    }

    // POST of Index method with invalid model returns view result and userProfile is set to == null so Viewbag returns Null
    [TestMethod]
    public async Task Index_InvalidModel_ReturnsViewResult_ViewBagNull()
    {

        // Arrange
        _controller.ModelState.Clear();

        _controller.ModelState.AddModelError("Error", "Model state is invalid");

        var userId = "TestId";

        var fuelQuote = new FuelHistory
        {
            // Populating with potentially valid data, but ModelState is explicitly made invalid in SetUp
            GallonsRequested = 100,
            DeliveryAddress = "123 Main St",
            DeliveryDate = "2022-01-01",
            SuggestedPrice = 2.5m,
            TotalAmountDue = 250m
        };

        // Set up the mock DbSet for UserProfiles
        var mockUserProfiles = new List<UserProfile>
        {
            new UserProfile { }
        }.AsQueryable();

        var mockUserProfilesDbSet = CreateMockDbSet(mockUserProfiles);

        _mockInitializer.MockUserManager
            .Setup(um => um.GetUserId(It.IsAny<ClaimsPrincipal>()))
            .Returns(userId);
        _mockInitializer.MockContext
            .Setup(c => c.UserProfiles)
            .Returns(mockUserProfilesDbSet.Object);

        // Act
        var result = await _controller.Index(fuelQuote) as ViewResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.IsFalse(_controller.ModelState.IsValid); // Ensure model state is indeed invalid as intended
        Assert.AreEqual("Null", _controller.ViewBag.DeliveryAddress);
        Assert.AreEqual(fuelQuote, result?.Model); // Check if the method returns the correct view with the original model
    }

    [TestMethod]
    public void History_ReturnsViewResult()
    {
        // Act
        var result = _controller.History();

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result, typeof(Task<IActionResult>));

    }
}