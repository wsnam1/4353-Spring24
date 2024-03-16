using Microsoft.AspNetCore.Mvc;
using Moq;
using Project.Controllers;
using Project.Models;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Project.Data;
using Microsoft.Extensions.Options;


[TestClass]
public class ApplicationDbContextTests
{

    [TestMethod]
    public async Task CanAddAndGetFuelHistory()
    {
        // Arrange: Create a new in-memory database context
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabaseFuelHistory") // Use a unique name to ensure a fresh database for each test
            .Options;

        // Use using statement to ensure the context is disposed after the test
        using (var context = new ApplicationDbContext(options))
        {
            var fuelHistory = new FuelHistory
            {
                // Populate with valid data
                UserId = "TestId",
                GallonsRequested = 100,
                DeliveryAddress = "123 Main St",
                DeliveryDate = "2022-01-01",
                SuggestedPrice = 1.99m,
                TotalAmountDue = 199
            };

            // Act: Add a new FuelHistory record and save changes
            context.FuelHistories.Add(fuelHistory);
            await context.SaveChangesAsync();

            // Assert: Verify that the FuelHistory can be retrieved from the database
            var retrievedFuelHistory = await context.FuelHistories.FindAsync(fuelHistory.UserId);
            Assert.IsNotNull(retrievedFuelHistory);
            // Add more assertions here as needed to verify the properties of the retrieved entity
        }
    }

    [TestMethod]
    public async Task CanAddAndGetUserProfile()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabaseUserProfile") // Ensure unique name for isolated testing
            .Options;

        using (var context = new ApplicationDbContext(options))
        {
            var userProfile = new UserProfile
            {
                UserId = "TestId",
                FullName = "John Doe",
                Address1 = "102 Main St",
                Address2 = "Apt 4",
                City = "Anytown",
                State = "CA",
                Zipcode = "12345" // Ensure the zipcode adheres to the specified StringLength attribute     
            };

            // Act
            context.UserProfiles.Add(userProfile);
            await context.SaveChangesAsync();

            // Assert
            var retrievedUserProfile = await context.UserProfiles.FindAsync(userProfile.UserId);
            Assert.IsNotNull(retrievedUserProfile);
            // Additional assertions to verify properties can be placed here
        }
    }
}


