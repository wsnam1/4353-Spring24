using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using Org.BouncyCastle.Bcpg;
using Project.Controllers;
using Project.Models;
using System.Security.Claims;


[TestClass]
public class ProfileControllerTests
{
    private MockInitializer _mockInitializer;
    private ProfileController _controller;
    

    [TestInitialize]
    public void Setup()
    {
        _mockInitializer = new MockInitializer();
        _controller = new ProfileController(_mockInitializer.MockContext.Object, _mockInitializer.MockUserManager.Object);
        
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

    [TestMethod]
    public void Create_ReturnsRedirectToActionResult()
    {
        // Arrange
        var userId = "TestId";
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
                Address1 = "102 Main St",
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
        var result = _controller.Create();

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
    }

    [TestMethod]
    public void Create_ReturnsViewResult()
    {
        // Arrange
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
        var result = _controller.Create();

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result, typeof(ViewResult));
    }

    [TestMethod]
    public async Task Create_ValidModel_RedirectsToHomeIndex()
    {
        // Arrange
        // Clear the model state to prevent any unexpected behaviour in our test
        _controller.ModelState.Clear();

        // Create a new FuelHistory model state with valid data
        var userProfile = new UserProfile
        {
            // Populate with valid data
            FullName = "John Doe",
            Address1 = "102 Main St",
            Address2 = "Apt 4",
            City = "Anytown",
            State = "CA",
            Zipcode = "12345" // Ensure the zipcode adheres to the specified StringLength attribute 
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
        var result = await _controller.Create(userProfile);

        // Assert
        Assert.IsNotNull(result);
        // Check if result is of type RedirectToActionResult
        Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));

        // Change the type of result to access the ControllerName and ActionName properties
        var redirectResult = (RedirectToActionResult)result;

        // Check if the method redirects to the correct controller and action
        Assert.AreEqual("Home", redirectResult.ControllerName);
        Assert.AreEqual("Index", redirectResult.ActionName);
    }

    [TestMethod]
    public async Task Create_InvalidModel_ReturnsViewResult()
    {
        // Arrange
        _controller.ModelState.Clear();

        _controller.ModelState.AddModelError("Error", "Model state is invalid");

        // Create a new FuelHistory model state with valid data
        var userProfile = new UserProfile
        {
            // Populate with valid data
            FullName = "John Doe",
            Address1 = "102 Main St",
            Address2 = "Apt 4",
            City = "Anytown",
            State = "CA",
            Zipcode = "12345" // Ensure the zipcode adheres to the specified StringLength attribute 
        };

        _mockInitializer.MockUserManager
            .Setup(um => um.GetUserId(It.IsAny<ClaimsPrincipal>()))
            .Returns("TestId");

        _mockInitializer.MockContext
            .Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(1));

        // Act
        var result = await _controller.Create(userProfile) as ViewResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.IsFalse(_controller.ModelState.IsValid); // Ensure model state is indeed invalid as intended
        Assert.AreEqual(userProfile, result?.Model); // Check if the method returns the correct view with the original model
    }

    // GET of Edit method where userProfile == null thus redirects to Create method
    [TestMethod]
    public void Edit_ReturnsRedirectToActionResult()
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
        var result = _controller.Edit();

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
    }

    // GET of Edit method where userProfile != null thus returns the View
    [TestMethod]
    public void Edit_ReturnsViewResult()
    {
        // Arrange
        var userId = "TestId";
        _mockInitializer.MockUserManager
            .Setup(um => um.GetUserId(It.IsAny<ClaimsPrincipal>()))
            .Returns(userId);

        // Set up the mock DbSet for UserProfiles
        var mockUserProfiles = new List<UserProfile>
        {
            new UserProfile 
            {
                // Populate with valid data
                UserId = userId,
                FullName = "John Doe",
                Address1 = "102 Main St",
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
        var result = _controller.Edit();

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result, typeof(ViewResult));
    }

    // POST of Edit with valid model redirects to Home controller Index method
    [TestMethod]
    public async Task Edit_ValidModel_RedirectsToHomeIndex()
    {
        // Arrange
        _controller.ModelState.Clear();

        var userId = "TestId";

        _mockInitializer.MockUserManager
            .Setup(um => um.GetUserId(It.IsAny<ClaimsPrincipal>()))
            .Returns(userId);

        // UserProfile data passed to the POST as parameter that will update the record in the DB
        var editedUserProfile = new UserProfile
        {
            // Populate with valid data
            UserId = userId,
            FullName = "Jean Penso",
            Address1 = "200 South St",
            Address2 = "Apt 1",
            City = "Houston",
            State = "TX",
            Zipcode = "12345" // Ensure the zipcode adheres to the specified StringLength attribute 

        };

        // Set up the mock DbSet for UserProfiles
        var mockUserProfiles = new List<UserProfile>
        {
            new UserProfile
            {
                // Populate with valid data
                UserId = userId,
                FullName = "John Doe",
                Address1 = "102 Main St",
                Address2 = "Apt 4",
                City = "Anytown",
                State = "CA",
                Zipcode = "7777" // Ensure the zipcode adheres to the specified StringLength attribute 
            }
        }.AsQueryable();

        var mockUserProfilesDbSet = CreateMockDbSet(mockUserProfiles);

        // Set up the mock DbSet on the mock ApplicationDbContext
        _mockInitializer.MockContext
            .Setup(c => c.UserProfiles)
            .Returns(mockUserProfilesDbSet.Object);

        // Simulate a SaveChangesAsync operation to a mock db context. Make it return the integer 1, indicating 1 record was changed or added
        _mockInitializer.MockContext
            .Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(1));

        // Act
        // Call the Index method
        var result = await _controller.Edit(editedUserProfile);

        // Assert
        Assert.IsNotNull(result);
        // Check if result is of type RedirectToActionResult
        Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));

        // Change the type of result to access the ControllerName and ActionName properties
        var redirectResult = (RedirectToActionResult)result;

        // Check if the method redirects to the correct controller and action
        Assert.AreEqual("Home", redirectResult.ControllerName);
        Assert.AreEqual("Index", redirectResult.ActionName);
    }

    // POST of Edit with valid model redirects to Home controller Index method
    [TestMethod]
    public async Task Edit_InvalidValidModel_ReturnsViewResult()
    {
        // Arrange
        _controller.ModelState.Clear();

        _controller.ModelState.AddModelError("Error", "Model state is invalid");

        var userId = "TestId";

        _mockInitializer.MockUserManager
            .Setup(um => um.GetUserId(It.IsAny<ClaimsPrincipal>()))
            .Returns(userId);

        // UserProfile data passed to the POST as parameter that will update the record in the DB
        var editedUserProfile = new UserProfile
        {
            // Populate with valid data
            UserId = userId,
            FullName = "Jean Penso",
            Address1 = "200 South St",
            Address2 = "Apt 1",
            City = "Houston",
            State = "TX",
            Zipcode = "12345" // Ensure the zipcode adheres to the specified StringLength attribute 

        };

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

        // Simulate a SaveChangesAsync operation to a mock db context. Make it return the integer 1, indicating 1 record was changed or added
        _mockInitializer.MockContext
            .Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(1));

        // Act
        // Call the Index method
        var result = await _controller.Edit(editedUserProfile);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsFalse(_controller.ModelState.IsValid); // Ensure model state is indeed invalid as intended
        // Check if result is of type RedirectToActionResult
        Assert.IsInstanceOfType(result, typeof(ViewResult));
    }
}
