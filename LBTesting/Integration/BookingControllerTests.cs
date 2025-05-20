using LBRepository;
using LBRepository.Repos;
using LBCore.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using Xunit;
using System.Linq;

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
		public async Task AddBookingAsync_ValidBooking_ReturnsTrueAndPersists()
		{
			var booking = new Booking
			{
				StudentEmail = "student@example.com",
				InstructorEmail = "instructor@example.com",
				Start = DateTime.UtcNow.Date.AddDays(1).AddHours(10),
				End = DateTime.UtcNow.Date.AddDays(1).AddHours(11)
			};

			_context.Availability.Add(new Availability
			{
				InstructorEmail = "instructor@example.com",
				Start = booking.Start.Date.AddHours(9),
				End = booking.Start.Date.AddHours(17)
			});
			await _context.SaveChangesAsync();

			var result = await _repo.AddBookingAsync(booking);

			Assert.True(result);
			Assert.NotNull(await _context.Bookings.FirstOrDefaultAsync(b => b.StudentEmail == "student@example.com"));
		}

		[Fact]
		public async Task AddBookingAsync_InvalidDuration_ReturnsFalse()
		{
			var booking = new Booking
			{
				StudentEmail = "student@example.com",
				InstructorEmail = "instructor@example.com",
				Start = DateTime.UtcNow.Date.AddDays(1).AddHours(10),
				End = DateTime.UtcNow.Date.AddDays(1).AddHours(10).AddMinutes(30) // 30 min, invalid
			};

			_context.Availability.Add(new Availability
			{
				InstructorEmail = "instructor@example.com",
				Start = booking.Start.Date.AddHours(9),
				End = booking.Start.Date.AddHours(17)
			});
			await _context.SaveChangesAsync();

			var result = await _repo.AddBookingAsync(booking);

			Assert.False(result);
			Assert.Null(await _context.Bookings.FirstOrDefaultAsync(b => b.StudentEmail == "student@example.com"));
		}

		[Fact]
		public async Task AddBookingAsync_NoAvailability_ReturnsFalse()
		{
			var booking = new Booking
			{
				StudentEmail = "student@example.com",
				InstructorEmail = "instructor@example.com",
				Start = DateTime.UtcNow.Date.AddDays(1).AddHours(10),
				End = DateTime.UtcNow.Date.AddDays(1).AddHours(11)
			};

			// No availability added
			var result = await _repo.AddBookingAsync(booking);

			Assert.False(result);
		}

		[Fact]
		public async Task AddBookingAsync_OverlappingBooking_ReturnsFalse()
		{
			var start = DateTime.UtcNow.Date.AddDays(2).AddHours(10);
			var end = start.AddHours(1);

			_context.Availability.Add(new Availability
			{
				InstructorEmail = "instructor@example.com",
				Start = start.Date.AddHours(9),
				End = start.Date.AddHours(17)
			});
			await _context.SaveChangesAsync();

			var booking1 = new Booking
			{
				StudentEmail = "student1@example.com",
				InstructorEmail = "instructor@example.com",
				Start = start,
				End = end
			};
			await _repo.AddBookingAsync(booking1);

			var booking2 = new Booking
			{
				StudentEmail = "student2@example.com",
				InstructorEmail = "instructor@example.com",
				Start = start.AddMinutes(30),
				End = end.AddMinutes(30)
			};

			var result = await _repo.AddBookingAsync(booking2);
			Assert.False(result);
		}

		[Fact]
		public async Task GetBookingsByInstructorAsync_ReturnsBookings()
		{
			var booking = new Booking
			{
				StudentEmail = "student@example.com",
				InstructorEmail = "instructor2@example.com",
				Start = DateTime.UtcNow.Date.AddDays(3).AddHours(10),
				End = DateTime.UtcNow.Date.AddDays(3).AddHours(11)
			};
			_context.Bookings.Add(booking);
			await _context.SaveChangesAsync();

			var bookings = await _repo.GetBookingsByInstructorAsync("instructor2@example.com");
			Assert.Single(bookings);
			Assert.Equal("student@example.com", bookings[0].StudentEmail);
		}

		[Fact]
		public async Task GetBookingsByStudentAsync_ReturnsBookings()
		{
			var booking = new Booking
			{
				StudentEmail = "student3@example.com",
				InstructorEmail = "instructor3@example.com",
				Start = DateTime.UtcNow.Date.AddDays(4).AddHours(10),
				End = DateTime.UtcNow.Date.AddDays(4).AddHours(11)
			};
			_context.Bookings.Add(booking);
			await _context.SaveChangesAsync();

			var bookings = await _repo.GetBookingsByStudentAsync("student3@example.com");
			Assert.Single(bookings);
			Assert.Equal("instructor3@example.com", bookings[0].InstructorEmail);
		}

		[Fact]
		public async Task RemoveBookingAsync_RemovesBooking()
		{
			var booking = new Booking
			{
				StudentEmail = "student4@example.com",
				InstructorEmail = "instructor4@example.com",
				Start = DateTime.UtcNow.Date.AddDays(5).AddHours(10),
				End = DateTime.UtcNow.Date.AddDays(5).AddHours(11)
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
			// Should not throw if booking does not exist
			var ex = await Record.ExceptionAsync(() => _repo.RemoveBookingAsync(99999));
			Assert.Null(ex);
		}

		[Fact]
		public async Task GetAllBookingsAsync_ReturnsAll()
		{
			_context.Bookings.Add(new Booking
			{
				StudentEmail = "student5@example.com",
				InstructorEmail = "instructor5@example.com",
				Start = DateTime.UtcNow.Date.AddDays(6).AddHours(10),
				End = DateTime.UtcNow.Date.AddDays(6).AddHours(11)
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
