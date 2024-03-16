using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Org.BouncyCastle.Bcpg;
using Project.Areas.Identity.Pages.Account;
using Project.Controllers;
using Project.Models;
using System.Security.Claims;

[TestClass]
public class RegisterModelTests
{
    // Only declare to private variables that are going to be used across test methods
    private MockInitializer _mockInitializer;
    private Mock<UserManager<IdentityUser>> _mockUserManager;
    private Mock<IUserStore<IdentityUser>> _mockUserStore;
    private RegisterModel _registerModel;

    [TestInitialize]
    public void Setup()
    {
        _mockInitializer = new MockInitializer();
        _mockUserStore = new Mock<IUserStore<IdentityUser>>();

        // Create a mock of IUserStore<IdentityUser> that also implements IUserEmailStore<IdentityUser>
        _mockUserStore.As<IUserEmailStore<IdentityUser>>()
            .Setup(x => x.GetEmailAsync(It.IsAny<IdentityUser>(), CancellationToken.None))
            .ReturnsAsync("test@example.com");

        // We can't get the mocked UserManager from the MockIntializer class because for this one we have to a mocked IUserStore that also implements IUserEmailStore
        // Create a mock of UserManager<IdentityUser>
        _mockUserManager = new Mock<UserManager<IdentityUser>>(
            _mockUserStore.Object, null, null, null, null, null, null, null, null);

        // Configure the mocked UserManager to return true for SupportsUserEmail
        _mockUserManager.Setup(x => x.SupportsUserEmail).Returns(true);

        // Create mocks for other dependencies
        var mockLogger = new Mock<ILogger<RegisterModel>>();
        var mockEmailSender = new Mock<IEmailSender>();

        // Create an instance of RegisterModel with the mocked dependencies
        _registerModel = new RegisterModel(
            _mockUserManager.Object,
            _mockUserStore.Object,
            _mockInitializer.MockSignInManager.Object,
            mockLogger.Object,
            mockEmailSender.Object);
    }

    [TestMethod]
    public void OnGetAsync_WithReturnUrl_SetsReturnUrl()
    {
        // Arrange
        var returnUrl = "/";
        // Act

        _registerModel.OnGetAsync(returnUrl);

        // Assert
        Assert.AreEqual(returnUrl, _registerModel.ReturnUrl);
    }

    [TestMethod]
    public async Task OnPostAsync_CreateUserSuccess_RedirectsToLoginPage()
    {
        // Arrange
        var returnUrl = "/";

        _mockUserStore.As<IUserEmailStore<IdentityUser>>()
            .Setup(x => x.SetUserNameAsync(It.IsAny<IdentityUser>(), It.IsAny<string>(), CancellationToken.None))
            .Returns(Task.CompletedTask);

        _mockUserManager.Setup(x => x.CreateAsync(It.IsAny<IdentityUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        _registerModel.Input = new RegisterModel.InputModel
        {
            Username = "testuser",
            Password = "password",
            ConfirmPassword = "password"
        };

        // Act
        var result = await _registerModel.OnPostAsync(returnUrl);

        // Assert
        Assert.IsInstanceOfType(result, typeof(RedirectToPageResult));
        var redirectResult = (RedirectToPageResult)result;
        Assert.AreEqual("Login", redirectResult.PageName);

        _mockUserManager.Verify(x => x.CreateAsync(It.IsAny<IdentityUser>(), It.IsAny<string>()), Times.Once);
        _mockUserStore.Verify(x => x.SetUserNameAsync(It.IsAny<IdentityUser>(), It.IsAny<string>(), CancellationToken.None), Times.Once);
    }

    [TestMethod]    
    public async Task OnPostAsync_CreateUserFails_ReturnsSamePageWithErrors()
    {
        // Arrange
        _registerModel.ModelState.Clear();

        var returnUrl = "/";

        var errors = new List<IdentityError>
        {
        new IdentityError { Code = "ErrorCode1", Description = "Error description 1" },
        new IdentityError { Code = "ErrorCode2", Description = "Error description 2" }
        };

        _mockUserManager
            .Setup(x => x.CreateAsync(It.IsAny<IdentityUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed(errors.ToArray()));

        _registerModel.Input = new RegisterModel.InputModel
        {
            Username = "testuser",
            Password = "password"
        };

        // Act
        var result = await _registerModel.OnPostAsync(returnUrl);

        // Assert
        _mockUserStore.Verify(x => x.SetUserNameAsync(It.IsAny<IdentityUser>(), "testuser", CancellationToken.None), Times.Once);
        _mockUserManager.Verify(x => x.CreateAsync(It.IsAny<IdentityUser>(), "password"), Times.Once);

        Assert.IsInstanceOfType(result, typeof(PageResult));
        Assert.IsFalse(_registerModel.ModelState.IsValid);
        foreach (var error in errors)
        {
            Assert.IsTrue(_registerModel.ModelState[string.Empty].Errors.Any(e => e.ErrorMessage == error.Description));
        }
    }

    [TestMethod]
    public void GetEmailStore_ThrowsNotSupportedException()
    {
        // Arrange
        _mockUserManager.Setup(x => x.SupportsUserEmail).Returns(false);
        
        var exceptionThrown = false;

        // Act
        try
        { 
            // Create an instance of the class where UserManager SupportsUserEmail is false, this will return a NotSupported Exception which we catch
            var model = new RegisterModel(_mockUserManager.Object, _mockUserStore.Object, null, null, null);
        }
        catch (NotSupportedException)
        {
            exceptionThrown = true;
        }

        // Assert
        Assert.IsTrue(exceptionThrown, "The default UI requires a user store with email support.");
    }
}

    


