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
	public class BookingControllerTests
	{
		private static BookingController CreateController(
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

			var controller = new BookingController(calendarManager, firebaseAccountRepos, hubContext.Object);

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
		public async Task GetBookingsByInstructor_StudentRole_ReturnsOk()
		{
			var bookings = new List<Booking> { new Booking { bookingId = 1, InstructorEmail = "i", StudentEmail = "s", Start = DateTime.UtcNow, End = DateTime.UtcNow.AddHours(1) } };
			var bookingRepos = new Mock<IBookingRepos>();
			bookingRepos.Setup(x => x.GetBookingsByInstructorAsync(It.IsAny<string>())).ReturnsAsync(bookings);
			var calendarManager = new CalendarManager(bookingRepos.Object, new Mock<IAvailabilityRepos>().Object);

			var controller = CreateController("student", calendarManager);

			var result = await controller.GetBookingsByInstructor("i");
			Assert.IsType<OkObjectResult>(result);
		}

		[Fact]
		public async Task GetBookingsByInstructor_ForbiddenRole_ReturnsForbid()
		{
			var controller = CreateController("other");
			var result = await controller.GetBookingsByInstructor("i");
			Assert.IsType<ForbidResult>(result);
		}

		[Fact]
		public async Task GetBookingsByInstructor_NoBookings_ReturnsNotFound()
		{
			var bookingRepos = new Mock<IBookingRepos>();
			bookingRepos.Setup(x => x.GetBookingsByInstructorAsync(It.IsAny<string>())).ReturnsAsync(new List<Booking>());
			var calendarManager = new CalendarManager(bookingRepos.Object, new Mock<IAvailabilityRepos>().Object);

			var controller = CreateController("student", calendarManager);

			var result = await controller.GetBookingsByInstructor("i");
			Assert.IsType<NotFoundObjectResult>(result);
		}

		[Fact]
		public async Task AddBooking_StudentRole_Success()
		{
			var bookingRepos = new Mock<IBookingRepos>();
			bookingRepos.Setup(x => x.AddBookingAsync(It.IsAny<Booking>())).ReturnsAsync(true);
			var calendarManager = new CalendarManager(bookingRepos.Object, new Mock<IAvailabilityRepos>().Object);

			var firebaseMock = new Mock<IFirebaseAccountRepos>();
			firebaseMock.Setup(x => x.GetUserRoleAsync(It.IsAny<string>())).ReturnsAsync("student");

			// Properly mock hub context
			var mockClients = new Mock<IHubClients>();
			var mockClientProxy = new Mock<IClientProxy>();
			mockClientProxy
				.Setup(x => x.SendCoreAsync(It.IsAny<string>(), It.IsAny<object[]>(), default))
				.Returns(Task.CompletedTask);
			mockClients.Setup(x => x.All).Returns(mockClientProxy.Object);
			var mockHubContext = new Mock<IHubContext<LessonBooker.Hubs.CalendarHub>>();
			mockHubContext.Setup(x => x.Clients).Returns(mockClients.Object);

			var controller = new BookingController(calendarManager, firebaseMock.Object, mockHubContext.Object);

			controller.ControllerContext = new ControllerContext
			{
				HttpContext = new DefaultHttpContext
				{
					User = new ClaimsPrincipal(new ClaimsIdentity(new[]
					{
						new Claim(ClaimTypes.NameIdentifier, "test-uid"),
						new Claim(ClaimTypes.Role, "student")
					}, "mock"))
				}
			};

			var booking = new Booking { bookingId = 1, InstructorEmail = "i", StudentEmail = "s", Start = DateTime.UtcNow, End = DateTime.UtcNow.AddHours(1) };
			var result = await controller.AddBooking(booking);
			Assert.IsType<CreatedAtActionResult>(result);
		}

		[Fact]
		public async Task AddBooking_StudentRole_Conflict()
		{
			var booking = new Booking { bookingId = 1, InstructorEmail = "i", StudentEmail = "s", Start = DateTime.UtcNow, End = DateTime.UtcNow.AddHours(1) };
			var bookingRepos = new Mock<IBookingRepos>();
			bookingRepos.Setup(x => x.AddBookingAsync(It.IsAny<Booking>())).ReturnsAsync(false);
			var calendarManager = new CalendarManager(bookingRepos.Object, new Mock<IAvailabilityRepos>().Object);

			var controller = CreateController("student", calendarManager);

			var result = await controller.AddBooking(booking);
			var badRequest = Assert.IsType<BadRequestObjectResult>(result);
			Assert.Contains("Booking conflict", badRequest.Value.ToString());
		}

		[Fact]
		public async Task AddBooking_NonStudentRole_ReturnsForbid()
		{
			var controller = CreateController("instructor");
			var booking = new Booking();
			var result = await controller.AddBooking(booking);
			Assert.IsType<ForbidResult>(result);
		}

		[Fact]
		public async Task RemoveBooking_AdminRole_ReturnsNoContent()
		{
			var bookingRepos = new Mock<IBookingRepos>();
			bookingRepos.Setup(x => x.RemoveBookingAsync(It.IsAny<int>())).Returns(Task.CompletedTask);
			var calendarManager = new CalendarManager(bookingRepos.Object, new Mock<IAvailabilityRepos>().Object);

			var controller = CreateController("admin", calendarManager);

			var result = await controller.RemoveBooking(1);
			Assert.IsType<NoContentResult>(result);
		}

		[Fact]
		public async Task RemoveBooking_NonAdminRole_ReturnsForbid()
		{
			var controller = CreateController("student");
			var result = await controller.RemoveBooking(1);
			Assert.IsType<ForbidResult>(result);
		}

		[Fact]
		public async Task GetAllBookings_StudentRole_ReturnsOk()
		{
			var bookingRepos = new Mock<IBookingRepos>();
			bookingRepos.Setup(x => x.GetAllBookingsAsync()).ReturnsAsync(new List<Booking>());
			var calendarManager = new CalendarManager(bookingRepos.Object, new Mock<IAvailabilityRepos>().Object);

			var controller = CreateController("student", calendarManager);

			var result = await controller.GetAllBookings();
			Assert.IsType<OkObjectResult>(result);
		}

		[Fact]
		public async Task GetAllBookings_ForbiddenRole_ReturnsForbid()
		{
			var controller = CreateController("other");
			var result = await controller.GetAllBookings();
			Assert.IsType<ForbidResult>(result);
		}
	}
}