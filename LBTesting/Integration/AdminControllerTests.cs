using LBCore.Controllers;
using LBCore.Interfaces;
using LBCore.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace LBTesting.Integration
{
	public class AdminControllerTests
	{
		private readonly Mock<IFirebaseKeyRepos> _mockKeyRepos;
		private readonly Mock<IFirebaseAccountRepos> _mockAccountRepos;
		private readonly Mock<IProfileRepos> _mockProfileRepos;

		// Helper to create controller with admin role
		private AdminController CreateAdminController()
		{
			// Always return "admin" for role checks
			_mockAccountRepos.Setup(r => r.GetUserRoleAsync(It.IsAny<string>())).ReturnsAsync("admin");

			var controller = new AdminController(_mockKeyRepos.Object, _mockAccountRepos.Object, _mockProfileRepos.Object);

			var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
			{
				new Claim(ClaimTypes.NameIdentifier, "admin-uid"),
				new Claim(ClaimTypes.Role, "admin")
			}, "mock"));

			controller.ControllerContext = new ControllerContext
			{
				HttpContext = new DefaultHttpContext { User = user }
			};

			return controller;
		}

		public AdminControllerTests()
		{
			_mockKeyRepos = new Mock<IFirebaseKeyRepos>();
			_mockAccountRepos = new Mock<IFirebaseAccountRepos>();
			_mockProfileRepos = new Mock<IProfileRepos>();
		}

		[Fact]
		public async Task UpdateStudentProfile_ExistingProfile_ReturnsOk()
		{
			var controller = CreateAdminController();
			var profile = new Profiles { Email = "student@example.com" };
			_mockProfileRepos.Setup(r => r.GetProfileByEmailAsync(profile.Email)).ReturnsAsync(profile);
			_mockProfileRepos.Setup(r => r.UpdateProfileAsync(profile)).Returns(Task.CompletedTask);

			var result = await controller.UpdateStudentProfile(profile);

			var okResult = Assert.IsType<OkObjectResult>(result);
			var messageProp = okResult.Value.GetType().GetProperty("Message");
			Assert.NotNull(messageProp);
			Assert.Equal("Profile updated successfully.", messageProp.GetValue(okResult.Value)?.ToString());
		}

		[Fact]
		public async Task UpdateStudentProfile_ProfileNotFound_ReturnsNotFound()
		{
			var controller = CreateAdminController();
			var profile = new Profiles { Email = "notfound@example.com" };
			_mockProfileRepos.Setup(r => r.GetProfileByEmailAsync(profile.Email)).ReturnsAsync((Profiles)null);

			var result = await controller.UpdateStudentProfile(profile);

			var notFound = Assert.IsType<NotFoundObjectResult>(result);
			var messageProp = notFound.Value.GetType().GetProperty("Message");
			Assert.NotNull(messageProp);
			Assert.Equal("Profile not found.", messageProp.GetValue(notFound.Value)?.ToString());
		}

		[Fact]
		public async Task UpdateStudentProfile_Exception_ReturnsBadRequest()
		{
			var controller = CreateAdminController();
			var profile = new Profiles { Email = "student@example.com" };
			_mockProfileRepos.Setup(r => r.GetProfileByEmailAsync(profile.Email)).ThrowsAsync(new Exception("fail"));

			var result = await controller.UpdateStudentProfile(profile);

			var badRequest = Assert.IsType<BadRequestObjectResult>(result);
			var messageProp = badRequest.Value.GetType().GetProperty("Message");
			Assert.NotNull(messageProp);
			Assert.Equal("fail", messageProp.GetValue(badRequest.Value)?.ToString());
		}
	}
}
