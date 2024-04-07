using Microsoft.AspNetCore.Mvc;
using Moq;
using Project.Controllers;
using Project.Models;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Bcpg;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System.Text.Json;
using Microsoft.AspNetCore.Http;



// beginning of a test class
[TestClass]
public class FuelQuoteControllerTests
{
    // Declare a variable of type MockInitializer class
    private MockInitializer _mockInitializer;
    // Declare a variable of type FuelQuoteController class
    private FuelQuoteController _controller;
    private Mock<ITempDataDictionary> _tempDataDictionary;

    [TestInitialize]
    public void Setup()
    {
        // Assign a new instance of the MockIntializer class to the _mockInitializer variable
        _mockInitializer = new MockInitializer();
        _controller = new FuelQuoteController(_mockInitializer.MockContext.Object, _mockInitializer.MockUserManager.Object);
        _tempDataDictionary = new Mock<ITempDataDictionary>();
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

    // GET of GetQuote method with userProfile != null
    [TestMethod]
    public void GetQuote_ReturnsViewResult_ViewBagAddress1()
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
        var result = _controller.GetQuote() as ViewResult;

        // Assert
        Assert.IsNotNull(result);
        // ViewBag is rendered to the view dynamically, the return statement ViewResult does not contain a ViewBag so we must access through _controller
        Assert.AreEqual(address1, _controller.ViewBag.DeliveryAddress);
        Assert.IsInstanceOfType(result, typeof(ViewResult));
        
    }

    // GET of GetQuote method with userProfile == null
    [TestMethod]
    public void GetQuote_ReturnsViewResult_ViewBagNull()
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
        var result = _controller.GetQuote() as ViewResult;

        // Assert
        Assert.IsNotNull(result);
        // ViewBag is rendered to the view dynamically, the return statement ViewResult does not contain a ViewBag so we must access through _controller
        Assert.AreEqual("Null", _controller.ViewBag.DeliveryAddress);
        Assert.IsInstanceOfType(result, typeof(ViewResult));

    }


    // POST of GetQuote method
    [TestMethod]
    public void GetQuote_ValidModel_ReturnsRedirectToAction()
    {
        // Arrange
        // Clear the model state to prevent any unexpected behaviour in our test
        _controller.ModelState.Clear();

        // Variables to use across the test method
        var userId = "TestId";
        var address1 = "123 Main St";

        // Mock a user operation to GetUserId
        _mockInitializer.MockUserManager
            .Setup(um => um.GetUserId(It.IsAny<ClaimsPrincipal>()))
            .Returns(userId);

        // Set up the mock DbSet for UserProfiles to later mock a SingleOrDefault operation on the UserProfiles table
        var mockUserProfiles = new List<UserProfile>
        {
            new UserProfile
            {
                UserId = userId,
                FullName = "John Doe",
                Address1 = address1,
                Address2 = "Apt 4",
                City = "Anytown",
                State = "TX",
                Zipcode = "12345" // Ensure the zipcode adheres to the specified StringLength attribute     
            }
        }.AsQueryable();

        var mockUserProfilesDbSet = CreateMockDbSet(mockUserProfiles);

        // Mock the SingleOrDefault operation on the UserProfiles table
        _mockInitializer.MockContext
            .Setup(c => c.UserProfiles)
            .Returns(mockUserProfilesDbSet.Object);


        // The data that we are going to pass to the method
        var fuelQuote = new FuelHistory
        {
            // Populate with valid data
            UserId = userId,
            GallonsRequested = 100,
            DeliveryAddress = "123 Main St",
            DeliveryDate = "2022-01-01",
        };


        // Create a new FuelHistory model state with valid data to check if the user has a fuel quote history
        var mockFuelHistories = new List<FuelHistory>
        {
            new FuelHistory
            {
                // Populate with valid data
                UserId = userId,
                GallonsRequested = 100,
                DeliveryAddress = "123 Main St",
                DeliveryDate = "2022-01-01",
            }
        }.AsQueryable();

        var mockFuelHistoriesDbSet = CreateMockDbSet(mockFuelHistories);

        _mockInitializer.MockContext
            .Setup(c => c.FuelHistories)
            .Returns(mockFuelHistoriesDbSet.Object);

        // Create an instance of TempDataDictionary and assign it to the controller's TempData property
        var tempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());
        _controller.TempData = tempData;

        // Act
        // Call the Index method
        var result = _controller.GetQuote(fuelQuote);

        // Assert
        Assert.IsNotNull(result);

        // Assert the pricing calculation
        var pricing = new Pricing();
        Assert.AreEqual(pricing.CalculatePrice(fuelQuote.GallonsRequested, true, true), fuelQuote.SuggestedPrice);
        Assert.AreEqual(fuelQuote.SuggestedPrice * fuelQuote.GallonsRequested, fuelQuote.TotalAmountDue);

        // Check if result is of type RedirectToActionResult
        Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));

        // Change the type of result to access the ControllerName and ActionName properties
        var redirectResult = (RedirectToActionResult)result;

        // Check if the method redirects to the correct controller and action
        Assert.AreEqual("FuelQuote", redirectResult.ControllerName);
        Assert.AreEqual("SubmitQuote", redirectResult.ActionName);
    }

    // POST of Index method with invalid model returns view result and userProfile is set to != null so Viewbag returns Address1
    [TestMethod]
    public void GetQuote_InvalidModel_ReturnsViewResult_ViewBagAddress1()
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
        var result = _controller.GetQuote(fuelQuote) as ViewResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.IsFalse(_controller.ModelState.IsValid); // Ensure model state is indeed invalid as intended
        Assert.AreEqual(address1, _controller.ViewBag.DeliveryAddress);
        Assert.AreEqual(fuelQuote, result?.Model); // Check if the method returns the correct view with the original model
    }

    // POST of Index method with invalid model returns view result and userProfile is set to == null so Viewbag returns Null
    [TestMethod]
    public void GetQuote_InvalidModel_ReturnsViewResult_ViewBagNull()
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
        { }.AsQueryable();

        var mockUserProfilesDbSet = CreateMockDbSet(mockUserProfiles);

        _mockInitializer.MockUserManager
            .Setup(um => um.GetUserId(It.IsAny<ClaimsPrincipal>()))
            .Returns(userId);

        _mockInitializer.MockContext
            .Setup(c => c.UserProfiles)
            .Returns(mockUserProfilesDbSet.Object);

        // Act
        var result = _controller.GetQuote(fuelQuote) as ViewResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.IsFalse(_controller.ModelState.IsValid); // Ensure model state is indeed invalid as intended
        Assert.AreEqual("Null", _controller.ViewBag.DeliveryAddress);
        Assert.AreEqual(fuelQuote, result?.Model); // Check if the method returns the correct view with the original model
    }

    [TestMethod]
    public void SubmitQuote_TempDataNotNull_ReturnsViewResult_ViewBagAddress1()
    {
        var userId = "TestId";
        var address1 = "123 Main St";

        // Arrange
        var fuelQuote = new FuelHistory
        {
            // Populating with potentially valid data, but ModelState is explicitly made invalid in SetUp
            GallonsRequested = 100,
            DeliveryAddress = "123 Main St",
            DeliveryDate = "2022-01-01",
            SuggestedPrice = 2.5m,
            TotalAmountDue = 250m
        };

        var fuelQuoteJson = JsonSerializer.Serialize(fuelQuote);
        var tempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());
        tempData["FuelQuoteJson"] = fuelQuoteJson;
        _controller.TempData = tempData;

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
        var result = _controller.SubmitQuote() as ViewResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(address1, _controller.ViewBag.DeliveryAddress);
        Assert.IsInstanceOfType(result, typeof(ViewResult));
    }

    [TestMethod]
    public void SubmitQuote_TempDataNotNull_ReturnsViewResult_ViewBagNull()
    {
        var userId = "TestId";

        // Arrange
        var fuelQuote = new FuelHistory
        {
            // Populating with potentially valid data, but ModelState is explicitly made invalid in SetUp
            GallonsRequested = 100,
            DeliveryAddress = "123 Main St",
            DeliveryDate = "2022-01-01",
            SuggestedPrice = 2.5m,
            TotalAmountDue = 250m
        };

        var fuelQuoteJson = JsonSerializer.Serialize(fuelQuote);
        var tempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());
        tempData["FuelQuoteJson"] = fuelQuoteJson;
        _controller.TempData = tempData;

        // Set up the mock DbSet for UserProfiles
        var mockUserProfiles = new List<UserProfile>
        { }.AsQueryable();

        var mockUserProfilesDbSet = CreateMockDbSet(mockUserProfiles);

        _mockInitializer.MockUserManager
            .Setup(um => um.GetUserId(It.IsAny<ClaimsPrincipal>()))
            .Returns(userId);

        _mockInitializer.MockContext
            .Setup(c => c.UserProfiles)
            .Returns(mockUserProfilesDbSet.Object);

        // Act
        var result = _controller.SubmitQuote() as ViewResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("Null", _controller.ViewBag.DeliveryAddress);
        Assert.IsInstanceOfType(result, typeof(ViewResult));
    }

    [TestMethod]
    public void SubmitQuote_TempDataNull_ReturnsRedirectToAction()
    {
        // Arrange
        var tempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());
        _controller.TempData = tempData;

        // Act
        var result = _controller.SubmitQuote() as RedirectToActionResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("Index", result.ActionName);
        Assert.AreEqual("FuelQuote", result.ControllerName);
    }

    // POST of GetQuote method
    [TestMethod]
    public async Task SubmitQuote_ValidModel_ReturnsRedirectToAction()
    {
        // Arrange
        // Clear the model state to prevent any unexpected behaviour in our test
        _controller.ModelState.Clear();

        // Variables to use across the test method
        var userId = "TestId";
        var address1 = "123 Main St";

        // Mock a user operation to GetUserId
        _mockInitializer.MockUserManager
            .Setup(um => um.GetUserId(It.IsAny<ClaimsPrincipal>()))
            .Returns(userId);

        // Set up the mock DbSet for UserProfiles to later mock a SingleOrDefault operation on the UserProfiles table
        var mockUserProfiles = new List<UserProfile>
        {
            new UserProfile
            {
                UserId = userId,
                FullName = "John Doe",
                Address1 = address1,
                Address2 = "Apt 4",
                City = "Anytown",
                State = "TX",
                Zipcode = "12345" // Ensure the zipcode adheres to the specified StringLength attribute     
            }
        }.AsQueryable();

        var mockUserProfilesDbSet = CreateMockDbSet(mockUserProfiles);

        // Mock the SingleOrDefault operation on the UserProfiles table
        _mockInitializer.MockContext
            .Setup(c => c.UserProfiles)
            .Returns(mockUserProfilesDbSet.Object);

        // Simulate a SaveChangesAsync operation to a mock db context. Make it return the integer 1, indicating 1 record was changed or added
        _mockInitializer.MockContext
            .Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(1));


        // The data that we are going to pass to the method
        var fuelQuote = new FuelHistory
        {
            // Populate with valid data
            UserId = userId,
            GallonsRequested = 100,
            DeliveryAddress = "123 Main St",
            DeliveryDate = "2022-01-01",
        };

        // Act
        // Call the Index method
        var result = await _controller.SubmitQuote(fuelQuote);

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

    // POST of Index method with invalid model returns view result and userProfile is set to != null so Viewbag returns Address1
    [TestMethod]
    public async Task SubmitQuote_InvalidModel_ReturnsViewResult_ViewBagAddress1()
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

        // Simulate a SaveChangesAsync operation to a mock db context. Make it return the integer 1, indicating 1 record was changed or added
        _mockInitializer.MockContext
            .Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(1));

        // Act
        var result = await _controller.SubmitQuote(fuelQuote);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsFalse(_controller.ModelState.IsValid); // Ensure model state is indeed invalid as intended
        Assert.AreEqual(address1, _controller.ViewBag.DeliveryAddress);
        Assert.IsInstanceOfType(result, typeof(ViewResult));
    }

    // POST of Index method with invalid model returns view result and userProfile is set to != null so Viewbag returns Address1
    [TestMethod]
    public async Task SubmitQuote_InvalidModel_ReturnsViewResult_ViewBagNull()
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
        { }.AsQueryable();

        var mockUserProfilesDbSet = CreateMockDbSet(mockUserProfiles);

        _mockInitializer.MockUserManager
            .Setup(um => um.GetUserId(It.IsAny<ClaimsPrincipal>()))
            .Returns(userId);

        _mockInitializer.MockContext
            .Setup(c => c.UserProfiles)
            .Returns(mockUserProfilesDbSet.Object);

        // Simulate a SaveChangesAsync operation to a mock db context. Make it return the integer 1, indicating 1 record was changed or added
        _mockInitializer.MockContext
            .Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(1));

        // Act
        var result = await _controller.SubmitQuote(fuelQuote);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsFalse(_controller.ModelState.IsValid); // Ensure model state is indeed invalid as intended
        Assert.AreEqual("Null", _controller.ViewBag.DeliveryAddress);
        Assert.IsInstanceOfType(result, typeof(ViewResult));
    }

    [TestMethod]
    public void History_ReturnsViewResult()
    {
        var userId = "TestId";

        // Set up the mock DbSet for UserProfiles
        var mockFuelHistories = new List<FuelHistory>
        {
            new FuelHistory
            {
                // Populate with valid data
                UserId = userId,
                GallonsRequested = 100,
                DeliveryAddress = "123 Main St",
                DeliveryDate = "2022-01-01",
                SuggestedPrice = 1.99m,
                TotalAmountDue = 199
            },
            new FuelHistory
            {
                // Populate with valid data
                UserId = userId,
                GallonsRequested = 100,
                DeliveryAddress = "123 Main St",
                DeliveryDate = "2022-01-01",
                SuggestedPrice = 1.99m,
                TotalAmountDue = 199
            }
        }.AsQueryable();

        var mockFuelHistoriesDbSet = CreateMockDbSet(mockFuelHistories);

        _mockInitializer.MockUserManager
            .Setup(um => um.GetUserId(It.IsAny<ClaimsPrincipal>()))
            .Returns(userId);

        _mockInitializer.MockContext
            .Setup(c => c.FuelHistories)
            .Returns(mockFuelHistoriesDbSet.Object);

        // Act
        var result = _controller.History();

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result, typeof(ViewResult));
        var viewResult = (ViewResult)result;
        Assert.IsInstanceOfType(viewResult.Model, typeof(List<FuelHistory>));
    }
}