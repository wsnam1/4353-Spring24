using Microsoft.AspNetCore.Mvc.Filters;
using Moq;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Project.Data;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Project.Models;
using Microsoft.EntityFrameworkCore;

[TestClass]
public class ProfileCompletionFilterTests
{
    private MockInitializer _mockInitializer;
    private ProfileCompletionFilterAttribute _filterAttribute;
    private ActionExecutingContext _actionExecutingContext;
    private Mock<ActionExecutionDelegate> _mockActionExecutionDelegate;
    private DefaultHttpContext _httpContext;
    private Mock<IServiceProvider> _mockServiceProvider;

    [TestInitialize]
    public void Setup()
    {
        _mockInitializer = new MockInitializer();
        _filterAttribute = new ProfileCompletionFilterAttribute();
        _mockActionExecutionDelegate = new Mock<ActionExecutionDelegate>();

        _mockServiceProvider = new Mock<IServiceProvider>();

        _httpContext = new DefaultHttpContext()
        {
            RequestServices = _mockServiceProvider.Object
        };

        _actionExecutingContext = new ActionExecutingContext(
            new ActionContext(_httpContext, new RouteData(), new ActionDescriptor()),
            new List<IFilterMetadata>(),
            new Dictionary<string, object>(),
            _filterAttribute);

        _actionExecutingContext.HttpContext.RequestServices = _mockServiceProvider.Object;
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


    // Test filter attribute where a user profile is found so the pipeline continues and the method that called the attribute continues its execution
    [TestMethod]
    public async Task OnActionExecutionAsync_ProfileComplete_ProceedsWithPipeline()
    {
        // Arrange
        var user = new IdentityUser { Id = "1" };
        _mockInitializer.MockUserManager.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
        _mockInitializer.MockUserManager.Setup(x => x.GetUserIdAsync(user)).ReturnsAsync(user.Id);


        var mockUserProfiles = new List<UserProfile>
        {
            new UserProfile
            {
                UserId = user.Id,
                FullName = "John Doe",
                Address1 = "102 Main St",
                Address2 = "Apt 4",
                City = "Anytown",
                State = "CA",
                Zipcode = "12345" // Ensure the zipcode adheres to the specified StringLength attribute     
            }
        }.AsQueryable();

        var mockUserProfilesDbSet = CreateMockDbSet(mockUserProfiles);

        _mockInitializer.MockContext.Setup(x => x.UserProfiles).Returns(mockUserProfilesDbSet.Object);

        _mockServiceProvider.Setup(x => x.GetService(typeof(UserManager<IdentityUser>))).Returns(_mockInitializer.MockUserManager.Object);
        _mockServiceProvider.Setup(x => x.GetService(typeof(ApplicationDbContext))).Returns(_mockInitializer.MockContext.Object);
        

        // Act
        await _filterAttribute.OnActionExecutionAsync(_actionExecutingContext, _mockActionExecutionDelegate.Object);

        // Assert
        _mockActionExecutionDelegate.Verify(x => x(), Times.Once);
    }

    // Test filter attribute where a user profile is not found so the user is redirected to the Create method in the Profile controller
    [TestMethod]
    public async Task OnActionExecutionAsync_ProfileIncomplete_RedirectsToCreateProfileForm()
    {
        // Arrange
        var user = new IdentityUser { Id = "1" };
        _mockInitializer.MockUserManager.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
        _mockInitializer.MockUserManager.Setup(x => x.GetUserIdAsync(user)).ReturnsAsync(user.Id);


        var mockUserProfiles = new List<UserProfile>().AsQueryable();

        var mockUserProfilesDbSet = CreateMockDbSet(mockUserProfiles);

        _mockInitializer.MockContext.Setup(x => x.UserProfiles).Returns(mockUserProfilesDbSet.Object);

        _mockServiceProvider.Setup(x => x.GetService(typeof(UserManager<IdentityUser>))).Returns(_mockInitializer.MockUserManager.Object);
        _mockServiceProvider.Setup(x => x.GetService(typeof(ApplicationDbContext))).Returns(_mockInitializer.MockContext.Object);


        // Act
        await _filterAttribute.OnActionExecutionAsync(_actionExecutingContext, _mockActionExecutionDelegate.Object);

        // Assert
        Assert.IsInstanceOfType(_actionExecutingContext.Result, typeof(RedirectToActionResult));
        var redirectResult = (RedirectToActionResult)_actionExecutingContext.Result;
        Assert.AreEqual("Create", redirectResult.ActionName);
        Assert.AreEqual("Profile", redirectResult.ControllerName);
    }
}