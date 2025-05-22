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
using Moq;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using LBCore.Interfaces;

namespace LBTesting.Integration
{
	public class ProfileControllerTests : IDisposable
	{
		private readonly ApplicationDbContext _context;

		public ProfileControllerTests()
		{
			var options = new DbContextOptionsBuilder<ApplicationDbContext>()
				.UseInMemoryDatabase(databaseName: "TestDatabase_" + Guid.NewGuid())
				.Options;

			_context = new ApplicationDbContext(options);
		}

		// Helper to create controller with a specific role
		private ProfileController CreateControllerWithRole(string role)
		{
			var profileRepos = new ProfileRepos(_context);
			var accountManager = new AccountManager(profileRepos);

			var firebaseAccountReposMock = new Mock<IFirebaseAccountRepos>();
			firebaseAccountReposMock.Setup(r => r.GetUserRoleAsync(It.IsAny<string>())).ReturnsAsync(role);

			var controller = new ProfileController(accountManager, firebaseAccountReposMock.Object);

			var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
			{
				new Claim(ClaimTypes.NameIdentifier, "test-uid"),
				new Claim(ClaimTypes.Role, role)
			}, "mock"));

			controller.ControllerContext = new ControllerContext
			{
				HttpContext = new DefaultHttpContext { User = user }
			};

			return controller;
		}

		[Fact]
		public async Task GetProfileByEmail_ExistingProfile_ReturnsOk()
		{
			var controller = CreateControllerWithRole("student"); // Any authenticated user
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

			var result = await controller.GetProfileByEmail(email);

			var okResult = Assert.IsType<OkObjectResult>(result);
			var returnValue = Assert.IsType<Profiles>(okResult.Value);
			Assert.Equal(email, returnValue.Email);
		}

		[Fact]
		public async Task GetProfileByEmail_NonExistentProfile_ReturnsNotFound()
		{
			var controller = CreateControllerWithRole("student"); // Any authenticated user
			var result = await controller.GetProfileByEmail("nonexistent@example.com");
			Assert.IsType<NotFoundObjectResult>(result);
		}

		[Fact]
		public async Task GetAllProfiles_ReturnsAllProfiles()
		{
			var controller = CreateControllerWithRole("admin"); // Admin only
			var profile1 = new Profiles
			{
				ProfileId = 1,
				Email = "user1@example.com",
				DisplayName = "User One",
				PhoneNumber = "1111111111",
				Address = "Address 1",
				PickupAddress = "Pickup 1",
				DateOfBirth = new DateTime(1991, 1, 1)
			};
			var profile2 = new Profiles
			{
				ProfileId = 2,
				Email = "user2@example.com",
				DisplayName = "User Two",
				PhoneNumber = "2222222222",
				Address = "Address 2",
				PickupAddress = "Pickup 2",
				DateOfBirth = new DateTime(1992, 2, 2)
			};

			await _context.AddAsync(profile1);
			await _context.AddAsync(profile2);
			await _context.SaveChangesAsync();

			var result = await controller.GetAllProfiles();

			var okResult = Assert.IsType<OkObjectResult>(result);
			var profiles = Assert.IsAssignableFrom<System.Collections.Generic.IEnumerable<Profiles>>(okResult.Value);
			Assert.Contains(profiles, p => p.Email == profile1.Email);
			Assert.Contains(profiles, p => p.Email == profile2.Email);
		}

		[Fact]
		public async Task AddProfile_ValidProfile_ReturnsCreated()
		{
			var controller = CreateControllerWithRole("student"); // Any authenticated user
			var profile = new Profiles
			{
				ProfileId = 3,
				Email = "newuser@example.com",
				DisplayName = "New User",
				PhoneNumber = "0987654321",
				Address = "789 New Street",
				PickupAddress = "101 Pickup Ave",
				DateOfBirth = new DateTime(1992, 2, 2)
			};

			var result = await controller.AddProfile(profile);

			var createdResult = Assert.IsType<CreatedAtActionResult>(result);
			var returnValue = Assert.IsType<Profiles>(createdResult.Value);
			Assert.Equal(profile.Email, returnValue.Email);
		}

		[Fact]
		public async Task AddProfile_NullProfile_ReturnsBadRequest()
		{
			var controller = CreateControllerWithRole("student"); // Any authenticated user
			var result = await controller.AddProfile(null);
			var badRequest = Assert.IsType<BadRequestObjectResult>(result);
			Assert.Equal("Profile data is required.", badRequest.Value);
		}

		[Fact]
		public async Task AddProfile_DuplicateEmail_ReturnsConflict()
		{
			var controller = CreateControllerWithRole("student"); // Any authenticated user
			var profile = new Profiles
			{
				ProfileId = 4,
				Email = "duplicate@example.com",
				DisplayName = "Dup User",
				PhoneNumber = "3333333333",
				Address = "Dup Address",
				PickupAddress = "Dup Pickup",
				DateOfBirth = new DateTime(1993, 3, 3)
			};

			await controller.AddProfile(profile);

			// Try to add again with the same email
			var duplicateProfile = new Profiles
			{
				ProfileId = 5,
				Email = "duplicate@example.com",
				DisplayName = "Dup User 2",
				PhoneNumber = "4444444444",
				Address = "Dup Address 2",
				PickupAddress = "Dup Pickup 2",
				DateOfBirth = new DateTime(1994, 4, 4)
			};

			var result = await controller.AddProfile(duplicateProfile);

			Assert.IsType<ConflictObjectResult>(result);
		}

		[Fact]
		public async Task UpdateProfile_ExistingProfile_ReturnsNoContent()
		{
			var controller = CreateControllerWithRole("admin"); // Admin only
			var profile = new Profiles
			{
				ProfileId = 6,
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

			var result = await controller.UpdateProfile(profile.ProfileId, profile);

			Assert.IsType<NoContentResult>(result);
		}

		[Fact]
		public async Task UpdateProfile_ProfileIdMismatch_ReturnsBadRequest()
		{
			var controller = CreateControllerWithRole("admin"); // Admin only
			var profile = new Profiles
			{
				ProfileId = 7,
				Email = "mismatch@example.com",
				DisplayName = "Mismatch User",
				PhoneNumber = "5555555555",
				Address = "Mismatch Address",
				PickupAddress = "Mismatch Pickup",
				DateOfBirth = new DateTime(1996, 6, 6)
			};

			var result = await controller.UpdateProfile(999, profile);

			var badRequest = Assert.IsType<BadRequestObjectResult>(result);
			Assert.Equal("Profile ID mismatch.", badRequest.Value);
		}

		[Fact]
		public async Task UpdateProfile_ProfileNotFound_ReturnsNotFound()
		{
			var controller = CreateControllerWithRole("admin"); // Admin only
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

			var result = await controller.UpdateProfile(99, profile);

			Assert.IsType<NotFoundObjectResult>(result);
		}

		public void Dispose()
		{
			_context.Dispose();
		}
	}
}
