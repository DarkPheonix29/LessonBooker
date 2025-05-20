using LBCore.Managers;
using LBCore.Models;
using LBCore.Interfaces;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace LBTesting.Unit.Regular
{
	public class CalendarManagerTests
	{
		private readonly Mock<IBookingRepos> _mockBookingRepos;
		private readonly Mock<IAvailabilityRepos> _mockAvailabilityRepos;
		private readonly CalendarManager _calendarManager;

		public CalendarManagerTests()
		{
			_mockBookingRepos = new Mock<IBookingRepos>();
			_mockAvailabilityRepos = new Mock<IAvailabilityRepos>();
			_calendarManager = new CalendarManager(_mockBookingRepos.Object, _mockAvailabilityRepos.Object);
		}

		// --- Booking Tests ---

		[Fact]
		public async Task GetBookingsByInstructorAsync_ValidEmail_ReturnsBookings()
		{
			var email = "instructor@example.com";
			var bookings = new List<Booking> { new Booking { InstructorEmail = email } };
			_mockBookingRepos.Setup(r => r.GetBookingsByInstructorAsync(email)).ReturnsAsync(bookings);

			var result = await _calendarManager.GetBookingsByInstructorAsync(email);

			Assert.Equal(bookings, result);
		}

		[Theory]
		[InlineData(null)]
		[InlineData("")]
		[InlineData("   ")]
		public async Task GetBookingsByInstructorAsync_InvalidEmail_ThrowsArgumentException(string email)
		{
			await Assert.ThrowsAsync<ArgumentException>(() => _calendarManager.GetBookingsByInstructorAsync(email));
		}

		[Fact]
		public async Task GetBookingsByStudentAsync_ValidEmail_ReturnsBookings()
		{
			var email = "student@example.com";
			var bookings = new List<Booking> { new Booking { StudentEmail = email } };
			_mockBookingRepos.Setup(r => r.GetBookingsByStudentAsync(email)).ReturnsAsync(bookings);

			var result = await _calendarManager.GetBookingsByStudentAsync(email);

			Assert.Equal(bookings, result);
		}

		[Theory]
		[InlineData(null)]
		[InlineData("")]
		[InlineData("   ")]
		public async Task GetBookingsByStudentAsync_InvalidEmail_ThrowsArgumentException(string email)
		{
			await Assert.ThrowsAsync<ArgumentException>(() => _calendarManager.GetBookingsByStudentAsync(email));
		}

		[Fact]
		public async Task AddBookingAsync_ValidBooking_CallsRepoAndReturnsResult()
		{
			var booking = new Booking
			{
				StudentEmail = "student@example.com",
				InstructorEmail = "instructor@example.com",
				Start = DateTime.UtcNow,
				End = DateTime.UtcNow.AddHours(1)
			};
			_mockBookingRepos.Setup(r => r.AddBookingAsync(booking)).ReturnsAsync(true);

			var result = await _calendarManager.AddBookingAsync(booking);

			Assert.True(result);
			_mockBookingRepos.Verify(r => r.AddBookingAsync(booking), Times.Once);
		}

		[Fact]
		public async Task AddBookingAsync_NullBooking_ThrowsArgumentNullException()
		{
			await Assert.ThrowsAsync<ArgumentNullException>(() => _calendarManager.AddBookingAsync(null));
		}

		[Fact]
		public async Task AddBookingAsync_MissingFields_ThrowsArgumentException()
		{
			var booking = new Booking
			{
				StudentEmail = "",
				InstructorEmail = null,
				Start = default,
				End = default
			};
			await Assert.ThrowsAsync<ArgumentException>(() => _calendarManager.AddBookingAsync(booking));
		}

		[Fact]
		public async Task AddBookingAsync_EndBeforeStart_ThrowsArgumentException()
		{
			var booking = new Booking
			{
				StudentEmail = "student@example.com",
				InstructorEmail = "instructor@example.com",
				Start = DateTime.UtcNow,
				End = DateTime.UtcNow.AddHours(-1)
			};
			await Assert.ThrowsAsync<ArgumentException>(() => _calendarManager.AddBookingAsync(booking));
		}

		[Fact]
		public async Task RemoveBookingAsync_ValidId_CallsRepo()
		{
			int bookingId = 123;
			_mockBookingRepos.Setup(r => r.RemoveBookingAsync(bookingId)).Returns(Task.CompletedTask);

			await _calendarManager.RemoveBookingAsync(bookingId);

			_mockBookingRepos.Verify(r => r.RemoveBookingAsync(bookingId), Times.Once);
		}

		[Theory]
		[InlineData(0)]
		[InlineData(-1)]
		public async Task RemoveBookingAsync_InvalidId_ThrowsArgumentException(int bookingId)
		{
			await Assert.ThrowsAsync<ArgumentException>(() => _calendarManager.RemoveBookingAsync(bookingId));
		}

		[Fact]
		public async Task GetAllBookingsAsync_ReturnsBookings()
		{
			var bookings = new List<Booking> { new Booking() };
			_mockBookingRepos.Setup(r => r.GetAllBookingsAsync()).ReturnsAsync(bookings);

			var result = await _calendarManager.GetAllBookingsAsync();

			Assert.Equal(bookings, result);
		}

		// --- Availability Tests ---

		[Fact]
		public async Task GetAvailabilityByInstructorAsync_ValidEmail_ReturnsAvailability()
		{
			var email = "instructor@example.com";
			var avail = new List<Availability> { new Availability { InstructorEmail = email } };
			_mockAvailabilityRepos.Setup(r => r.GetAvailabilityByInstructorAsync(email)).ReturnsAsync(avail);

			var result = await _calendarManager.GetAvailabilityByInstructorAsync(email);

			Assert.Equal(avail, result);
		}

		[Theory]
		[InlineData(null)]
		[InlineData("")]
		[InlineData("   ")]
		public async Task GetAvailabilityByInstructorAsync_InvalidEmail_ThrowsArgumentException(string email)
		{
			await Assert.ThrowsAsync<ArgumentException>(() => _calendarManager.GetAvailabilityByInstructorAsync(email));
		}

		[Fact]
		public async Task AddAvailabilityAsync_ValidAvailability_CallsRepo()
		{
			var availability = new Availability
			{
				InstructorEmail = "instructor@example.com",
				Start = DateTime.UtcNow,
				End = DateTime.UtcNow.AddHours(2)
			};
			_mockAvailabilityRepos.Setup(r => r.AddAvailabilityAsync(availability)).Returns(Task.CompletedTask);

			await _calendarManager.AddAvailabilityAsync(availability);

			_mockAvailabilityRepos.Verify(r => r.AddAvailabilityAsync(availability), Times.Once);
		}

		[Fact]
		public async Task AddAvailabilityAsync_NullAvailability_ThrowsArgumentNullException()
		{
			await Assert.ThrowsAsync<ArgumentNullException>(() => _calendarManager.AddAvailabilityAsync(null));
		}

		[Fact]
		public async Task AddAvailabilityAsync_MissingFields_ThrowsArgumentException()
		{
			var availability = new Availability
			{
				InstructorEmail = "",
				Start = default,
				End = default
			};
			await Assert.ThrowsAsync<ArgumentException>(() => _calendarManager.AddAvailabilityAsync(availability));
		}

		[Fact]
		public async Task AddAvailabilityAsync_EndBeforeStart_ThrowsArgumentException()
		{
			var availability = new Availability
			{
				InstructorEmail = "instructor@example.com",
				Start = DateTime.UtcNow,
				End = DateTime.UtcNow.AddHours(-1)
			};
			await Assert.ThrowsAsync<ArgumentException>(() => _calendarManager.AddAvailabilityAsync(availability));
		}

		[Fact]
		public async Task RemoveAvailabilityAsync_ValidId_CallsRepo()
		{
			int id = 1;
			_mockAvailabilityRepos.Setup(r => r.RemoveAvailabilityAsync(id)).Returns(Task.CompletedTask);

			await _calendarManager.RemoveAvailabilityAsync(id);

			_mockAvailabilityRepos.Verify(r => r.RemoveAvailabilityAsync(id), Times.Once);
		}

		[Theory]
		[InlineData(0)]
		[InlineData(-1)]
		public async Task RemoveAvailabilityAsync_InvalidId_ThrowsArgumentException(int id)
		{
			await Assert.ThrowsAsync<ArgumentException>(() => _calendarManager.RemoveAvailabilityAsync(id));
		}

		[Fact]
		public async Task GetAllAvailabilityAsync_ReturnsAvailability()
		{
			var avail = new List<Availability> { new Availability() };
			_mockAvailabilityRepos.Setup(r => r.GetAllAvailabilityAsync()).ReturnsAsync(avail);

			var result = await _calendarManager.GetAllAvailabilityAsync();

			Assert.Equal(avail, result);
		}
	}
}
