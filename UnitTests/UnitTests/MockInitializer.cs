using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;
using Project.Data;


public class MockInitializer
{
    // These properties can be accessed by any class but only be set by the MockInitializer class, hence the private setter
    public Mock<UserManager<IdentityUser>> MockUserManager { get; private set; }
    public Mock<ApplicationDbContext> MockContext { get; private set; }
    public Mock<SignInManager<IdentityUser>> MockSignInManager { get; private set; }

    // Once an instance of the MockInitializer class is created, call the constructor and methods to set the public properties
    public MockInitializer()
    {
        SetupMockContext();
        SetupMockUserManager();
        SetupSignInManager();
    }

    // Assign a mock of db context to the MockContext public property
    private void SetupMockContext()
    {
        // Create an in-memory database context for testing purposes without relying on a real database connection
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb")
            .Options;

        // Mock the userManager and Db context needed with dependency injection in the controller
        MockContext = new Mock<ApplicationDbContext>(options);
    }

    // Assign a mock of usermanager to the MockUserManager public property
    private void SetupMockUserManager()
    {
        // Create a mock user store parameter to pass to the user manager constructor, this is a required field in the constructor
        var mockUserStore = new Mock<IUserStore<IdentityUser>>();

        // Mock the user manager and pass all the neccesary parameters to the constructor
        MockUserManager = new Mock<UserManager<IdentityUser>>(
            mockUserStore.Object, // IUserStore<TUser>
            null, // IOptions<IdentityOptions>
            null, // IPasswordHasher<TUser>
            null, // IEnumerable<IUserValidator<TUser>>
            null, // IEnumerable<IPasswordValidator<TUser>>
            null, // ILookupNormalizer
            null, // IdentityErrorDescriber
            null, // IServiceProvider
            null  // ILogger<UserManager<TUser>>
            );
    }

    private void SetupSignInManager()
    {
        var contextAccessor = new Mock<IHttpContextAccessor>();
        var userPrincipalFactory = new Mock<IUserClaimsPrincipalFactory<IdentityUser>>();

        // Mocking the SignInManager and ILogger
        MockSignInManager = new Mock<SignInManager<IdentityUser>>(
            MockUserManager.Object,
            contextAccessor.Object,
            userPrincipalFactory.Object,
            null,
            null,
            null,
            null
            );
    }
}


