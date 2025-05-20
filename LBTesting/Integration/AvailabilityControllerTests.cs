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
	public class AvailabilityControllerTests : IDisposable
	{
		private readonly AvailabilityController _controller;
		private readonly ApplicationDbContext _context;

		public AvailabilityControllerTests()
		{
			var options = new DbContextOptionsBuilder<ApplicationDbContext>()
				.UseInMemoryDatabase(databaseName: "AvailabilityTestDb_" + Guid.NewGuid())
				.Options;

			_context = new ApplicationDbContext(options);
			var bookingRepos = new BookingRepos(_context);
			var availabilityRepos = new AvailabilityRepos(_context);
			var calendarManager = new CalendarManager(bookingRepos, availabilityRepos);

			_controller = new AvailabilityController(calendarManager);
		}

		[Fact]
		public async Task GetAvailability_ExistingInstructor_ReturnsOk()
		{
			var availability = new Availability
			{
				InstructorEmail = "instructor1@example.com",
				Start = DateTime.UtcNow.AddDays(1).Date.AddHours(9),
				End = DateTime.UtcNow.AddDays(1).Date.AddHours(17)
			};
			await _context.Availability.AddAsync(availability);
			await _context.SaveChangesAsync();

			var result = await _controller.GetAvailability("instructor1@example.com");

			var okResult = Assert.IsType<OkObjectResult>(result);
			var availabilities = Assert.IsAssignableFrom<System.Collections.Generic.IEnumerable<Availability>>(okResult.Value);
			Assert.Contains(availabilities, a => a.InstructorEmail == "instructor1@example.com");
		}

		[Fact]
		public async Task GetAvailability_NonExistentInstructor_ReturnsNotFound()
		{
			var result = await _controller.GetAvailability("noone@example.com");
			var notFound = Assert.IsType<NotFoundObjectResult>(result);
			Assert.Equal("No availability found for this instructor.", notFound.Value);
		}

		[Fact]
		public async Task AddAvailability_Valid_ReturnsCreated()
		{
			var availability = new Availability
			{
				InstructorEmail = "instructor2@example.com",
				Start = DateTime.UtcNow.AddDays(2).Date.AddHours(10),
				End = DateTime.UtcNow.AddDays(2).Date.AddHours(12)
			};

			var result = await _controller.AddAvailability(availability);

			var created = Assert.IsType<CreatedAtActionResult>(result);
			var returnedAvailability = Assert.IsType<Availability>(created.Value);
			Assert.Equal(availability.InstructorEmail, returnedAvailability.InstructorEmail);
		}

		[Fact]
		public async Task RemoveAvailability_Existing_ReturnsNoContent()
		{
			var availability = new Availability
			{
				InstructorEmail = "instructor3@example.com",
				Start = DateTime.UtcNow.AddDays(3).Date.AddHours(8),
				End = DateTime.UtcNow.AddDays(3).Date.AddHours(10)
			};
			await _context.Availability.AddAsync(availability);
			await _context.SaveChangesAsync();

			var result = await _controller.RemoveAvailability(availability.availabilityId);

			Assert.IsType<NoContentResult>(result);
		}

		[Fact]
		public async Task GetAllInstructorAvailability_ReturnsAll()
		{
			var availability = new Availability
			{
				InstructorEmail = "instructor4@example.com",
				Start = DateTime.UtcNow.AddDays(4).Date.AddHours(9),
				End = DateTime.UtcNow.AddDays(4).Date.AddHours(11)
			};
			await _context.Availability.AddAsync(availability);
			await _context.SaveChangesAsync();

			var result = await _controller.GetAllInstructorAvailability();

			var okResult = Assert.IsType<OkObjectResult>(result);
			var availabilities = Assert.IsAssignableFrom<System.Collections.Generic.IEnumerable<Availability>>(okResult.Value);
			Assert.Contains(availabilities, a => a.InstructorEmail == "instructor4@example.com");
		}

		public void Dispose()
		{
			_context.Dispose();
		}
	}
}
