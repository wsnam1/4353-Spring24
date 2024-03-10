﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Project.Areas.Identity.Pages.Account;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Humanizer;

[TestClass]
public class LoginModelTests
{
    private MockInitializer _mockInitializer;
    private Mock<ILogger<LoginModel>> _mockLogger;
    private LoginModel _loginModel;
    private DefaultHttpContext _httpContext;

    [TestInitialize]
    public void Setup()
    {
        _mockInitializer = new MockInitializer();

        _mockLogger = new Mock<ILogger<LoginModel>>();

        // Setting up the PageModel
        var serviceProvider = new ServiceCollection()
        .AddLogging()
        .BuildServiceProvider();

        var httpContext = new DefaultHttpContext()
        {
            RequestServices = serviceProvider
        };

        var actionContext = new ActionContext(httpContext, new RouteData(), new PageActionDescriptor());

        _loginModel = new LoginModel(_mockInitializer.MockSignInManager.Object, _mockLogger.Object)
        {
            PageContext = new PageContext(actionContext),
            Url = new UrlHelper(actionContext)
        };


        // Set HttpContext so that UrlHelper can work
        _httpContext = new DefaultHttpContext();
        _loginModel.Url = new UrlHelper(new ActionContext(_httpContext, new RouteData(), new PageActionDescriptor()));
    }

    /*[TestMethod]
    public async Task OnGetAsync_WithReturnUrl_SetsReturnUrl()
    {
        // Arrange
        var returnUrl = "/Home";

        // Act
        await _loginModel.OnGetAsync(returnUrl);

        // Assert
        Assert.AreEqual(returnUrl, _loginModel.ReturnUrl);
    }*/

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
            .Setup(manager => manager.PasswordSignInAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
            .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);

        // Act
        // Call the OnPostAsync method an store the result type in the result variable
        var result = await _loginModel.OnPostAsync();

        // Assert
        // Check that the type is LocalRedirectResult
        Assert.IsInstanceOfType(result, typeof(LocalRedirectResult));
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
            .Setup(manager => manager.PasswordSignInAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
            .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Failed);

        // Act
        // Call the OnPostAsync method an store the result type in the result variable
        var result = await _loginModel.OnPostAsync();

        // Assert
        // Assert that typeof result is PageResult
        Assert.IsInstanceOfType(result, typeof(PageResult));
        // Assert that error count of the ModelState is greater than 0
        Assert.IsTrue(_loginModel.ModelState.ErrorCount > 0);
    }
}
