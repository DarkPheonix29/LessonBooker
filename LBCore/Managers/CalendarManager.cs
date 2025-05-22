using LBCore.Interfaces;
using LBCore.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LBCore.Managers
{
	public class CalendarManager
	{
		private readonly IBookingRepos _bookingRepos;
		private readonly IAvailabilityRepos _availabilityRepos;

		public CalendarManager(IBookingRepos bookingRepos, IAvailabilityRepos availabilityRepos)
		{
			_bookingRepos = bookingRepos;
			_availabilityRepos = availabilityRepos;
		}

		// Booking Methods

		public async Task<List<Booking>> GetBookingsByInstructorAsync(string instructorEmail)
		{
			if (string.IsNullOrWhiteSpace(instructorEmail))
			{
				throw new ArgumentException("Instructor email must not be empty.", nameof(instructorEmail));
			}

			return await _bookingRepos.GetBookingsByInstructorAsync(instructorEmail);
		}

		public async Task<List<Booking>> GetBookingsByStudentAsync(string studentEmail)
		{
			if (string.IsNullOrWhiteSpace(studentEmail))
			{
				throw new ArgumentException("Student email must not be empty.", nameof(studentEmail));
			}

			return await _bookingRepos.GetBookingsByStudentAsync(studentEmail);
		}

		public async Task<bool> AddBookingAsync(Booking booking)
		{
			ValidateBooking(booking);

			// The repository already checks for duration, overlap, and availability,
			// but you can add extra validation here if needed.
			return await _bookingRepos.AddBookingAsync(booking);
		}

		public async Task RemoveBookingAsync(int bookingId)
		{
			if (bookingId <= 0)
			{
				throw new ArgumentException("Booking ID must be positive.", nameof(bookingId));
			}

			await _bookingRepos.RemoveBookingAsync(bookingId);
		}

		public async Task<List<Booking>> GetAllBookingsAsync()
		{
			return await _bookingRepos.GetAllBookingsAsync();
		}

		// Availability Methods

		public async Task<List<Availability>> GetAvailabilityByInstructorAsync(string instructorEmail)
		{
			if (string.IsNullOrWhiteSpace(instructorEmail))
			{
				throw new ArgumentException("Instructor email must not be empty.", nameof(instructorEmail));
			}

			return await _availabilityRepos.GetAvailabilityByInstructorAsync(instructorEmail);
		}

		public async Task AddAvailabilityAsync(Availability availability)
		{
			ValidateAvailability(availability);
			await _availabilityRepos.AddAvailabilityAsync(availability);
		}

		public async Task RemoveAvailabilityAsync(int availabilityId)
		{
			if (availabilityId <= 0)
			{
				throw new ArgumentException("Availability ID must be positive.", nameof(availabilityId));
			}

			await _availabilityRepos.RemoveAvailabilityAsync(availabilityId);
		}

		public async Task<List<Availability>> GetAllAvailabilityAsync()
		{
			return await _availabilityRepos.GetAllAvailabilityAsync();
		}

		// Validation Helpers

		private void ValidateBooking(Booking booking)
		{
			if (booking == null)
			{
				throw new ArgumentNullException(nameof(booking), "Booking must not be null.");
			}

			if (string.IsNullOrWhiteSpace(booking.StudentEmail))
			{
				throw new ArgumentException("Student email is required.", nameof(booking.StudentEmail));
			}
			if (string.IsNullOrWhiteSpace(booking.InstructorEmail))
			{
				throw new ArgumentException("Instructor email is required.", nameof(booking.InstructorEmail));
			}
			if (booking.Start == default)
			{
				throw new ArgumentException("Booking start time is required.", nameof(booking.Start));
			}
			if (booking.End == default)
			{
				throw new ArgumentException("Booking end time is required.", nameof(booking.End));
			}
			if (booking.End <= booking.Start)
			{
				throw new ArgumentException("Booking end time must be after start time.");
			}
		}

		private void ValidateAvailability(Availability availability)
		{
			if (availability == null)
			{
				throw new ArgumentNullException(nameof(availability), "Availability must not be null.");
			}

			if (string.IsNullOrWhiteSpace(availability.InstructorEmail))
			{
				throw new ArgumentException("Instructor email is required.", nameof(availability.InstructorEmail));
			}
			if (availability.Start == default)
			{
				throw new ArgumentException("Availability start time is required.", nameof(availability.Start));
			}
			if (availability.End == default)
			{
				throw new ArgumentException("Availability end time is required.", nameof(availability.End));
			}
			if (availability.End <= availability.Start)
			{
				throw new ArgumentException("Availability end time must be after start time.");
			}
		}
	}
}
