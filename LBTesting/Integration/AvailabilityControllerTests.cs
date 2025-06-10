using LessonBooker.Controllers.RegularControllers;
using LBCore.Managers;
using LBCore.Models;
using LBCore.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;
using System.Collections.Generic;
using System;

namespace LBTesting.Integration
{
	public class AvailabilityControllerTests
	{
		private static AvailabilityController CreateController(
			string role,
			CalendarManager? calendarManager = null,
			IFirebaseAccountRepos? firebaseAccountRepos = null)
		{
			var bookingRepos = new Mock<IBookingRepos>();
			var availabilityRepos = new Mock<IAvailabilityRepos>();
			var hubContext = new Mock<IHubContext<LessonBooker.Hubs.CalendarHub>>();

			if (calendarManager == null)
			{
				calendarManager = new CalendarManager(bookingRepos.Object, availabilityRepos.Object);
			}

			if (firebaseAccountRepos == null)
			{
				var firebaseMock = new Mock<IFirebaseAccountRepos>();
				firebaseMock.Setup(x => x.GetUserRoleAsync(It.IsAny<string>())).ReturnsAsync(role);
				firebaseAccountRepos = firebaseMock.Object;
			}

			var controller = new AvailabilityController(calendarManager, firebaseAccountRepos, hubContext.Object);

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
		public async Task GetAvailability_InstructorRole_ReturnsOk()
		{
			var availabilities = new List<Availability> { new Availability { availabilityId = 1, InstructorEmail = "i", Start = DateTime.UtcNow, End = DateTime.UtcNow.AddHours(1) } };
			var availabilityRepos = new Mock<IAvailabilityRepos>();
			availabilityRepos.Setup(x => x.GetAvailabilityByInstructorAsync(It.IsAny<string>())).ReturnsAsync(availabilities);
			var calendarManager = new CalendarManager(new Mock<IBookingRepos>().Object, availabilityRepos.Object);

			var controller = CreateController("instructor", calendarManager);

			var result = await controller.GetAvailability("i");
			Assert.IsType<OkObjectResult>(result);
		}

		[Fact]
		public async Task GetAvailability_ForbiddenRole_ReturnsForbid()
		{
			var controller = CreateController("other");
			var result = await controller.GetAvailability("i");
			Assert.IsType<ForbidResult>(result);
		}

		[Fact]
		public async Task GetAvailability_NoAvailability_ReturnsNotFound()
		{
			var availabilityRepos = new Mock<IAvailabilityRepos>();
			availabilityRepos.Setup(x => x.GetAvailabilityByInstructorAsync(It.IsAny<string>())).ReturnsAsync(new List<Availability>());
			var calendarManager = new CalendarManager(new Mock<IBookingRepos>().Object, availabilityRepos.Object);

			var controller = CreateController("instructor", calendarManager);

			var result = await controller.GetAvailability("i");
			Assert.IsType<NotFoundObjectResult>(result);
		}

		[Fact]
		public async Task AddAvailability_InstructorRole_Success()
		{
			var availabilityRepos = new Mock<IAvailabilityRepos>();
			availabilityRepos.Setup(x => x.AddAvailabilityAsync(It.IsAny<Availability>())).Returns(Task.CompletedTask);
			var calendarManager = new CalendarManager(new Mock<IBookingRepos>().Object, availabilityRepos.Object);

			var firebaseMock = new Mock<IFirebaseAccountRepos>();
			firebaseMock.Setup(x => x.GetUserRoleAsync(It.IsAny<string>())).ReturnsAsync("instructor");

			 // Inline mock for SignalR
			var mockClients = new Mock<IHubClients>();
			var mockClientProxy = new Mock<IClientProxy>();
			mockClientProxy
				.Setup(x => x.SendCoreAsync(It.IsAny<string>(), It.IsAny<object[]>(), default))
				.Returns(Task.CompletedTask);
			mockClients.Setup(x => x.All).Returns(mockClientProxy.Object);
			var mockHubContext = new Mock<IHubContext<LessonBooker.Hubs.CalendarHub>>();
			mockHubContext.Setup(x => x.Clients).Returns(mockClients.Object);

			var controller = new AvailabilityController(calendarManager, firebaseMock.Object, mockHubContext.Object);

			var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
			{
				new Claim(ClaimTypes.NameIdentifier, "test-uid"),
				new Claim(ClaimTypes.Role, "instructor")
			}, "mock"));

			controller.ControllerContext = new ControllerContext
			{
				HttpContext = new DefaultHttpContext { User = user }
			};

			var availability = new Availability { availabilityId = 1, InstructorEmail = "i", Start = DateTime.UtcNow, End = DateTime.UtcNow.AddHours(1) };
			var result = await controller.AddAvailability(availability);
			Assert.IsType<CreatedAtActionResult>(result);
		}

		[Fact]
		public async Task AddAvailability_NonInstructorRole_ReturnsForbid()
		{
			var controller = CreateController("student");
			var availability = new Availability();
			var result = await controller.AddAvailability(availability);
			Assert.IsType<ForbidResult>(result);
		}

		[Fact]
		public async Task RemoveAvailability_InstructorRole_Success()
		{
			var availabilityRepos = new Mock<IAvailabilityRepos>();
			availabilityRepos.Setup(x => x.RemoveAvailabilityAsync(It.IsAny<int>())).Returns(Task.CompletedTask);
			var calendarManager = new CalendarManager(new Mock<IBookingRepos>().Object, availabilityRepos.Object);

			var firebaseMock = new Mock<IFirebaseAccountRepos>();
			firebaseMock.Setup(x => x.GetUserRoleAsync(It.IsAny<string>())).ReturnsAsync("instructor");

			// Inline mock for SignalR
			var mockClients = new Mock<IHubClients>();
			var mockClientProxy = new Mock<IClientProxy>();
			mockClientProxy
				.Setup(x => x.SendCoreAsync(It.IsAny<string>(), It.IsAny<object[]>(), default))
				.Returns(Task.CompletedTask);
			mockClients.Setup(x => x.All).Returns(mockClientProxy.Object);
			var mockHubContext = new Mock<IHubContext<LessonBooker.Hubs.CalendarHub>>();
			mockHubContext.Setup(x => x.Clients).Returns(mockClients.Object);

			var controller = new AvailabilityController(calendarManager, firebaseMock.Object, mockHubContext.Object);

			var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
			{
				new Claim(ClaimTypes.NameIdentifier, "test-uid"),
				new Claim(ClaimTypes.Role, "instructor")
			}, "mock"));

			controller.ControllerContext = new ControllerContext
			{
				HttpContext = new DefaultHttpContext { User = user }
			};

			var result = await controller.RemoveAvailability(1);
			Assert.IsType<NoContentResult>(result);
		}

		[Fact]
		public async Task RemoveAvailability_NonInstructorRole_ReturnsForbid()
		{
			var controller = CreateController("student");
			var result = await controller.RemoveAvailability(1);
			Assert.IsType<ForbidResult>(result);
		}

		[Fact]
		public async Task GetAllInstructorAvailability_InstructorRole_ReturnsOk()
		{
			var availabilityRepos = new Mock<IAvailabilityRepos>();
			availabilityRepos.Setup(x => x.GetAllAvailabilityAsync()).ReturnsAsync(new List<Availability>());
			var calendarManager = new CalendarManager(new Mock<IBookingRepos>().Object, availabilityRepos.Object);

			var controller = CreateController("instructor", calendarManager);

			var result = await controller.GetAllInstructorAvailability();
			Assert.IsType<OkObjectResult>(result);
		}

		[Fact]
		public async Task GetAllInstructorAvailability_ForbiddenRole_ReturnsForbid()
		{
			var controller = CreateController("other");
			var result = await controller.GetAllInstructorAvailability();
			Assert.IsType<ForbidResult>(result);
		}
	}
}