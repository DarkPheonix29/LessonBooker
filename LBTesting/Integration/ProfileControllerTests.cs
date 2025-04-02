using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LBAPI.Controllers;
using LBCore.Models;
using LBCore.Managers;
using LBRepository;
using Xunit;
using System.Threading.Tasks;
using LBRepository.Repos;
using System;

namespace LBTesting.Integration
{
	public class ProfileControllerTests : IDisposable
	{
		private readonly ProfileController _controller;
		private readonly ApplicationDbContext _context;

		public ProfileControllerTests()
		{
			var options = new DbContextOptionsBuilder<ApplicationDbContext>()
				.UseInMemoryDatabase(databaseName: "TestDatabase_" + Guid.NewGuid()) // Unique database name per test run
				.Options;

			_context = new ApplicationDbContext(options);
			var profileRepos = new ProfileRepos(_context);
			var accountManager = new AccountManager(profileRepos); // Updated with new repos
			_controller = new ProfileController(accountManager);
		}

		[Fact]
		public async Task GetProfileByEmail_ExistingProfile_ReturnsOk()
		{
			var email = "test@example.com";
			var profile = new Profiles
			{
				ProfileId = 1,
				Email = email,
				DisplayName = "Test User",
				PhoneNumber = "1234567890",
				Address = "123 Test Street",
				PickupAddress = "456 Pickup Lane",
				DateOfBirth = new DateTime(1990, 1, 1)
			};

			await _context.AddAsync(profile);
			await _context.SaveChangesAsync();

			var result = await _controller.GetProfileByEmail(email);

			var okResult = Assert.IsType<OkObjectResult>(result);
			var returnValue = Assert.IsType<Profiles>(okResult.Value);
			Assert.Equal(email, returnValue.Email);
		}

		[Fact]
		public async Task GetProfileByEmail_NonExistentProfile_ReturnsNotFound()
		{
			var result = await _controller.GetProfileByEmail("nonexistent@example.com");

			Assert.IsType<NotFoundResult>(result);
		}

		[Fact]
		public async Task AddProfile_ValidProfile_ReturnsCreated()
		{
			var profile = new Profiles
			{
				ProfileId = 2,
				Email = "newuser@example.com",
				DisplayName = "New User",
				PhoneNumber = "0987654321",
				Address = "789 New Street",
				PickupAddress = "101 Pickup Ave",
				DateOfBirth = new DateTime(1992, 2, 2)
			};

			var result = await _controller.AddProfile(profile);

			var createdResult = Assert.IsType<CreatedAtActionResult>(result);
			var returnValue = Assert.IsType<Profiles>(createdResult.Value);
			Assert.Equal(profile.Email, returnValue.Email);
		}

		[Fact]
		public async Task UpdateProfile_ExistingProfile_ReturnsNoContent()
		{
			var profile = new Profiles
			{
				ProfileId = 3,
				Email = "updateuser@example.com",
				DisplayName = "Update User",
				PhoneNumber = "1231231234",
				Address = "Updated Address",
				PickupAddress = "Updated Pickup",
				DateOfBirth = new DateTime(1995, 3, 3)
			};

			await _context.AddAsync(profile);
			await _context.SaveChangesAsync();

			profile.DisplayName = "Updated Display Name";

			var result = await _controller.UpdateProfile(profile.ProfileId, profile);

			Assert.IsType<NoContentResult>(result);
		}

		[Fact]
		public async Task UpdateProfile_ProfileNotFound_ReturnsBadRequest()
		{
			var profile = new Profiles
			{
				ProfileId = 99,
				Email = "notfound@example.com",
				DisplayName = "Not Found User",
				PhoneNumber = "0000000000",
				Address = "No Address",
				PickupAddress = "No Pickup",
				DateOfBirth = new DateTime(2000, 1, 1)
			};

			var result = await _controller.UpdateProfile(99, profile);

			Assert.IsType<BadRequestObjectResult>(result);
		}

		// Dispose of the context after tests
		public void Dispose()
		{
			_context.Dispose();
		}
	}
}
