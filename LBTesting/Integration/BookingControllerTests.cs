using LessonBooker.Controllers.RegularControllers;
using LBCore.Managers;
using LBCore.Models;
using LBRepository;
using LBRepository.Repos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using Xunit;

namespace LBTesting.Integration
{
	public class BookingControllerTests : IDisposable
	{
		private readonly BookingController _controller;
		private readonly ApplicationDbContext _context;

		public BookingControllerTests()
		{
			var options = new DbContextOptionsBuilder<ApplicationDbContext>()
				.UseInMemoryDatabase(databaseName: "BookingTestDb_" + Guid.NewGuid())
				.Options;

			_context = new ApplicationDbContext(options);
			var bookingRepos = new BookingRepos(_context);
			var availabilityRepos = new AvailabilityRepos(_context);
			var calendarManager = new CalendarManager(bookingRepos, availabilityRepos);

			_controller = new BookingController(calendarManager);
		}

		[Fact]
		public async Task GetBookingsByInstructor_Existing_ReturnsOk()
		{
			var booking = new Booking
			{
				StudentEmail = "student1@example.com",
				InstructorEmail = "instructor1@example.com",
				Start = DateTime.UtcNow.AddDays(1).Date.AddHours(10),
				End = DateTime.UtcNow.AddDays(1).Date.AddHours(11)
			};
			await _context.Bookings.AddAsync(booking);
			await _context.SaveChangesAsync();

			var result = await _controller.GetBookingsByInstructor("instructor1@example.com");

			var okResult = Assert.IsType<OkObjectResult>(result);
			var bookings = Assert.IsAssignableFrom<System.Collections.Generic.IEnumerable<Booking>>(okResult.Value);
			Assert.Contains(bookings, b => b.InstructorEmail == "instructor1@example.com");
		}

		[Fact]
		public async Task GetBookingsByInstructor_None_ReturnsNotFound()
		{
			var result = await _controller.GetBookingsByInstructor("noone@example.com");
			var notFound = Assert.IsType<NotFoundObjectResult>(result);
			Assert.Equal("No bookings found for this instructor.", notFound.Value);
		}

		[Fact]
		public async Task GetBookingsByStudent_Existing_ReturnsOk()
		{
			var booking = new Booking
			{
				StudentEmail = "student2@example.com",
				InstructorEmail = "instructor2@example.com",
				Start = DateTime.UtcNow.AddDays(2).Date.AddHours(12),
				End = DateTime.UtcNow.AddDays(2).Date.AddHours(13)
			};
			await _context.Bookings.AddAsync(booking);
			await _context.SaveChangesAsync();

			var result = await _controller.GetBookingsByStudent("student2@example.com");

			var okResult = Assert.IsType<OkObjectResult>(result);
			var bookings = Assert.IsAssignableFrom<System.Collections.Generic.IEnumerable<Booking>>(okResult.Value);
			Assert.Contains(bookings, b => b.StudentEmail == "student2@example.com");
		}

		[Fact]
		public async Task GetBookingsByStudent_None_ReturnsNotFound()
		{
			var result = await _controller.GetBookingsByStudent("noone@example.com");
			var notFound = Assert.IsType<NotFoundObjectResult>(result);
			Assert.Equal("No bookings found for this student.", notFound.Value);
		}

		[Fact]
		public async Task AddBooking_Valid_ReturnsCreated()
		{
			// Add availability for the instructor
			var availability = new LBCore.Models.Availability
			{
				InstructorEmail = "instructor3@example.com",
				Start = DateTime.UtcNow.AddDays(3).Date.AddHours(9),
				End = DateTime.UtcNow.AddDays(3).Date.AddHours(17)
			};
			await _context.Availability.AddAsync(availability);
			await _context.SaveChangesAsync();

			var booking = new Booking
			{
				StudentEmail = "student3@example.com",
				InstructorEmail = "instructor3@example.com",
				Start = availability.Start.AddHours(1),
				End = availability.Start.AddHours(2)
			};

			var result = await _controller.AddBooking(booking);

			var created = Assert.IsType<CreatedAtActionResult>(result);
			var returnedBooking = Assert.IsType<Booking>(created.Value);
			Assert.Equal(booking.StudentEmail, returnedBooking.StudentEmail);
		}

		[Fact]
		public async Task AddBooking_Conflict_ReturnsBadRequest()
		{
			// Add availability for the instructor
			var availability = new LBCore.Models.Availability
			{
				InstructorEmail = "instructor4@example.com",
				Start = DateTime.UtcNow.AddDays(4).Date.AddHours(9),
				End = DateTime.UtcNow.AddDays(4).Date.AddHours(17)
			};
			await _context.Availability.AddAsync(availability);
			await _context.SaveChangesAsync();

			// Add a booking that will conflict
			var booking1 = new Booking
			{
				StudentEmail = "student4@example.com",
				InstructorEmail = "instructor4@example.com",
				Start = availability.Start.AddHours(1),
				End = availability.Start.AddHours(2)
			};
			await _context.Bookings.AddAsync(booking1);
			await _context.SaveChangesAsync();

			// Fix for the CS1501 error: Replace the incorrect usage of AddHours with the correct one.
			var booking2 = new Booking
			{
				StudentEmail = "student5@example.com",
				InstructorEmail = "instructor4@example.com",
				Start = availability.Start.AddHours(1.5), 
				End = availability.Start.AddHours(2.5)  
			};

			var result = await _controller.AddBooking(booking2);

			var badRequest = Assert.IsType<BadRequestObjectResult>(result);
			Assert.Equal("Booking conflict with another instructor or time slot.", badRequest.Value);
		}

		[Fact]
		public async Task RemoveBooking_Existing_ReturnsNoContent()
		{
			var booking = new Booking
			{
				StudentEmail = "student6@example.com",
				InstructorEmail = "instructor6@example.com",
				Start = DateTime.UtcNow.AddDays(6).Date.AddHours(10),
				End = DateTime.UtcNow.AddDays(6).Date.AddHours(11)
			};
			await _context.Bookings.AddAsync(booking);
			await _context.SaveChangesAsync();

			var result = await _controller.RemoveBooking(booking.bookingId); // Fix: Pass bookingId as an int

			Assert.IsType<NoContentResult>(result);
		}

		[Fact]
		public async Task GetAllBookings_ReturnsAll()
		{
			var booking = new Booking
			{
				StudentEmail = "student7@example.com",
				InstructorEmail = "instructor7@example.com",
				Start = DateTime.UtcNow.AddDays(7).Date.AddHours(10),
				End = DateTime.UtcNow.AddDays(7).Date.AddHours(11)
			};
			await _context.Bookings.AddAsync(booking);
			await _context.SaveChangesAsync();

			var result = await _controller.GetAllBookings();

			var okResult = Assert.IsType<OkObjectResult>(result);
			var bookings = Assert.IsAssignableFrom<System.Collections.Generic.IEnumerable<Booking>>(okResult.Value);
			Assert.Contains(bookings, b => b.StudentEmail == "student7@example.com");
		}

		public void Dispose()
		{
			_context.Dispose();
		}
	}
}
