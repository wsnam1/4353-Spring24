using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Project.Areas.Identity.Pages.Account;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

[TestClass]
public class LoginModelTests
{
    // Set variables that need to be used across test methods only, NOT variables that are used only in the Setup method
    private MockInitializer _mockInitializer;
    private Mock<ITempDataDictionary> _tempDataDictionary;
    private Mock<ITempDataDictionaryFactory> _tempDataDictionaryFactory;
    private DefaultHttpContext _httpContext;
    private LoginModel _loginModel;

    [TestInitialize]
    public void Setup()
    { 
        // Setup the mock initializer
        _mockInitializer = new MockInitializer();

        // Setup the logger
        var mockLogger = new Mock<ILogger<LoginModel>>();

        // Create a mock of ITempDataDictionary
        _tempDataDictionary = new Mock<ITempDataDictionary>();

        // Create a mock of ITempDataDictionaryFactory
        _tempDataDictionaryFactory = new Mock<ITempDataDictionaryFactory>();

        // Create a mock IAuthenticationService
        var mockAuthService = new Mock<IAuthenticationService>();

        // Setting up the PageModel with a ServiceCollection that includes the mocked IAuthenticationService
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton(mockAuthService.Object); // Add the mock IAuthenticationService to the services which we will pass to HttpContext

        // Build services with the mocked authentication service and addlogging
        var serviceProvider = services.BuildServiceProvider();

        // Create an instance HttpContext where we set the RequestServices property to serviceProvider
        // We don't set httpContext in the mock intializer class because 
        _httpContext = new DefaultHttpContext()
        {
            RequestServices = serviceProvider
        };

        var actionContext = new ActionContext(_httpContext, new RouteData(), new PageActionDescriptor());

        _loginModel = new LoginModel(_mockInitializer.MockSignInManager.Object, mockLogger.Object)
        {
            PageContext = new PageContext(actionContext),
            Url = new UrlHelper(actionContext)
        };
    }

    // GET of LoginModel to check if ReturnUrl property is set to returnUrl
    [TestMethod]
    public async Task OnGetAsync_WithReturnUrl_SetsReturnUrl()
    {
        // Arrange
        var returnUrl = "/";

        // Act
        await _loginModel.OnGetAsync(returnUrl);

        // Assert
        Assert.AreEqual(returnUrl, _loginModel.ReturnUrl);
    }

    // GET of LoginModel to check if method sets a model state error when there's an error in TempData
    [TestMethod]
    public async Task OnGetAsync_WithErrorMessage_AddsModelError()
    {
        // Arrange
        var errorMessage = "Test Error message";

        _tempDataDictionary.Setup(d => d["ErrorMessage"]).Returns(null);

        _tempDataDictionaryFactory.Setup(f => f.GetTempData(It.IsAny<HttpContext>())).Returns(_tempDataDictionary.Object);

        // Assign the mocked ITempDataDictionaryFactory to the LoginModel
        _loginModel.TempData = _tempDataDictionaryFactory.Object.GetTempData(_httpContext);

        // Add an error message to the Error Message property of the model
        _loginModel.ErrorMessage = errorMessage;

        // Act
        await _loginModel.OnGetAsync();

        // Assert
        Assert.IsFalse(_loginModel.ModelState.IsValid);
        var modelStateError = _loginModel.ModelState[string.Empty].Errors.FirstOrDefault();
        Assert.IsNotNull(modelStateError);
        Assert.AreEqual(errorMessage, modelStateError.ErrorMessage);
    }

    // GET of LoginModel to check if method does not set a model state error when ErrorMessage in TempData is null
    [TestMethod]
    public async Task OnGetAsync_WithoutErrorMessage_DoesNotAddModelError()
    {
        // Arrange

        _tempDataDictionary.Setup(d => d["ErrorMessage"]).Returns(null);

        _tempDataDictionaryFactory.Setup(f => f.GetTempData(It.IsAny<HttpContext>())).Returns(_tempDataDictionary.Object);

        // Assign the mocked ITempDataDictionaryFactory to the LoginModel
        _loginModel.TempData = _tempDataDictionaryFactory.Object.GetTempData(_httpContext);

        // Act
        await _loginModel.OnGetAsync();

        // Assert
        Assert.IsTrue(_loginModel.ModelState.IsValid);
    }

    [TestMethod]
    public async Task OnPostAsync_WithValidModelState_ReturnsRedirect()
    {
        // Arrange
        // Access the properties of the InputModel class (Username, Password, RememberMe) within the LoginModel class
        _loginModel.Input = new LoginModel.InputModel
        {
            // Correct credentials don't matter because the method will ReturnAsync Success
            Username = "testUser",
            Password = "correctPassword",
            RememberMe = false
        };

        // Mock the SignInManager and pass the parameters to the PasswordSignInAsync method
        // Params: .PasswordSignInAsync(Input.Username, Input.Password, Input.RememberMe, lockoutOnFailure: false);
        // Make the mock call return a SignInResult of Success
        _mockInitializer.MockSignInManager
            .Setup(m => m.PasswordSignInAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
            .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);

        // Act
        // Call the OnPostAsync method an store the result type in the result variable
        var result = await _loginModel.OnPostAsync();

        // Assert
        // Check that the type is LocalRedirectResult
        Assert.IsInstanceOfType(result, typeof(LocalRedirectResult));
        Assert.IsTrue(_loginModel.ModelState.IsValid);
    }

    [TestMethod]
    public async Task OnPostAsync_WithInvalidLoginAttempt_ReturnsPageResultWithModelError()
    {
        // Arrange
        _loginModel.Input = new LoginModel.InputModel
        {
            // Incorrect credentials don't matter because the method will ReturnAsync Failed
            Username = "testUser",
            Password = "wrongPassword",
            RememberMe = false
        };

        // Mock the SignInManager and pass the parameters to the PasswordSignInAsync method
        // Params: .PasswordSignInAsync(Input.Username, Input.Password, Input.RememberMe, lockoutOnFailure: false);
        // Make the mock call return a SignInResult of Failed
        _mockInitializer.MockSignInManager
            .Setup(m => m.PasswordSignInAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
            .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Failed);

        // Act
        // Call the OnPostAsync method an store the result type in the result variable
        var result = await _loginModel.OnPostAsync();

        // Assert
        // Assert that typeof result is PageResult
        Assert.IsInstanceOfType(result, typeof(PageResult));
        // Assert that error count of the ModelState is greater than 0
        Assert.IsFalse(_loginModel.ModelState.IsValid);
    }

    [TestMethod]
    public async Task OnPostAsync_InvalidModel_ReturnsPageResult()
    {
        // Arrange
        _loginModel.ModelState.AddModelError("Error", "Model state is invalid");

        // Act
        // Call the OnPostAsync method an store the result type in the result variable
        var result = await _loginModel.OnPostAsync();

        // Assert that typeof result is PageResult
        Assert.IsInstanceOfType(result, typeof(PageResult));
        // Assert the state of the model
        Assert.IsFalse(_loginModel.ModelState.IsValid);
    }
}

