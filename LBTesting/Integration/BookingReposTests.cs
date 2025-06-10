using LBRepository;
using LBRepository.Repos;
using LBCore.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using Xunit;
using System.Linq;
using System.Collections.Generic;

namespace LBTesting.Integration
{
	public class BookingReposTests : IDisposable
	{
		private readonly ApplicationDbContext _context;
		private readonly BookingRepos _repo;

		public BookingReposTests()
		{
			var options = new DbContextOptionsBuilder<ApplicationDbContext>()
				.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
				.Options;
			_context = new ApplicationDbContext(options);
			_repo = new BookingRepos(_context);
		}

		[Fact]
		public async Task GetBookingsByInstructorAsync_ReturnsBookings()
		{
			var booking = new Booking
			{
				StudentEmail = "student@example.com",
				InstructorEmail = "instructor1@example.com",
				Start = DateTime.UtcNow.Date.AddHours(10),
				End = DateTime.UtcNow.Date.AddHours(11)
			};
			_context.Bookings.Add(booking);
			await _context.SaveChangesAsync();

			var result = await _repo.GetBookingsByInstructorAsync("instructor1@example.com");
			Assert.Single(result);
			Assert.Equal("student@example.com", result[0].StudentEmail);
		}

		[Fact]
		public async Task GetBookingsByStudentAsync_ReturnsBookings()
		{
			var booking = new Booking
			{
				StudentEmail = "student2@example.com",
				InstructorEmail = "instructor2@example.com",
				Start = DateTime.UtcNow.Date.AddHours(12),
				End = DateTime.UtcNow.Date.AddHours(13)
			};
			_context.Bookings.Add(booking);
			await _context.SaveChangesAsync();

			var result = await _repo.GetBookingsByStudentAsync("student2@example.com");
			Assert.Single(result);
			Assert.Equal("instructor2@example.com", result[0].InstructorEmail);
		}

		[Fact]
		public async Task AddBookingAsync_ValidBooking_ReturnsTrue()
		{
			// Add availability for the instructor
			var availability = new Availability
			{
				InstructorEmail = "instructor3@example.com",
				Start = DateTime.UtcNow.Date.AddHours(9),
				End = DateTime.UtcNow.Date.AddHours(17)
			};
			_context.Availability.Add(availability);
			await _context.SaveChangesAsync();

			var booking = new Booking
			{
				StudentEmail = "student3@example.com",
				InstructorEmail = "instructor3@example.com",
				Start = DateTime.UtcNow.Date.AddHours(10),
				End = DateTime.UtcNow.Date.AddHours(11)
			};

			var result = await _repo.AddBookingAsync(booking);
			Assert.True(result);
			Assert.NotNull(await _context.Bookings.FirstOrDefaultAsync(b => b.StudentEmail == "student3@example.com"));
		}

		[Fact]
		public async Task AddBookingAsync_InvalidDuration_ReturnsFalse()
		{
			var availability = new Availability
			{
				InstructorEmail = "instructor4@example.com",
				Start = DateTime.UtcNow.Date.AddHours(9),
				End = DateTime.UtcNow.Date.AddHours(17)
			};
			_context.Availability.Add(availability);
			await _context.SaveChangesAsync();

			var booking = new Booking
			{
				StudentEmail = "student4@example.com",
				InstructorEmail = "instructor4@example.com",
				Start = DateTime.UtcNow.Date.AddHours(10),
				End = DateTime.UtcNow.Date.AddHours(10).AddMinutes(30) // 30 min, invalid
			};

			var result = await _repo.AddBookingAsync(booking);
			Assert.False(result);
		}

		[Fact]
		public async Task AddBookingAsync_NoAvailability_ReturnsFalse()
		{
			var booking = new Booking
			{
				StudentEmail = "student5@example.com",
				InstructorEmail = "instructor5@example.com",
				Start = DateTime.UtcNow.Date.AddHours(10),
				End = DateTime.UtcNow.Date.AddHours(11)
			};

			var result = await _repo.AddBookingAsync(booking);
			Assert.False(result);
		}

		[Fact]
		public async Task AddBookingAsync_OverlappingBooking_ReturnsFalse()
		{
			var availability = new Availability
			{
				InstructorEmail = "instructor6@example.com",
				Start = DateTime.UtcNow.Date.AddHours(9),
				End = DateTime.UtcNow.Date.AddHours(17)
			};
			_context.Availability.Add(availability);
			await _context.SaveChangesAsync();

			var booking1 = new Booking
			{
				StudentEmail = "student6a@example.com",
				InstructorEmail = "instructor6@example.com",
				Start = DateTime.UtcNow.Date.AddHours(10),
				End = DateTime.UtcNow.Date.AddHours(11)
			};
			await _repo.AddBookingAsync(booking1);

			var booking2 = new Booking
			{
				StudentEmail = "student6b@example.com",
				InstructorEmail = "instructor6@example.com",
				Start = DateTime.UtcNow.Date.AddHours(10).AddMinutes(30),
				End = DateTime.UtcNow.Date.AddHours(11).AddMinutes(30)
			};

			var result = await _repo.AddBookingAsync(booking2);
			Assert.False(result);
		}

		[Fact]
		public async Task RemoveBookingAsync_RemovesBooking()
		{
			var booking = new Booking
			{
				StudentEmail = "student7@example.com",
				InstructorEmail = "instructor7@example.com",
				Start = DateTime.UtcNow.Date.AddHours(10),
				End = DateTime.UtcNow.Date.AddHours(11)
			};
			_context.Bookings.Add(booking);
			await _context.SaveChangesAsync();

			await _repo.RemoveBookingAsync(booking.bookingId);

			var found = await _context.Bookings.FindAsync(booking.bookingId);
			Assert.Null(found);
		}

		[Fact]
		public async Task RemoveBookingAsync_NonExistent_DoesNotThrow()
		{
			var ex = await Record.ExceptionAsync(() => _repo.RemoveBookingAsync(99999));
			Assert.Null(ex);
		}

		[Fact]
		public async Task GetAllBookingsAsync_ReturnsAll()
		{
			_context.Bookings.Add(new Booking
			{
				StudentEmail = "student8@example.com",
				InstructorEmail = "instructor8@example.com",
				Start = DateTime.UtcNow.Date.AddHours(10),
				End = DateTime.UtcNow.Date.AddHours(11)
			});
			await _context.SaveChangesAsync();

			var bookings = await _repo.GetAllBookingsAsync();
			Assert.NotEmpty(bookings);
		}

		public void Dispose()
		{
			_context.Dispose();
		}
	}
}