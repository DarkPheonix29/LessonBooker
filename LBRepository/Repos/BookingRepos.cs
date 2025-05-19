using LBCore.Interfaces;
using LBCore.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LBRepository.Repos
{
	public class BookingRepos : IBookingRepos
	{
		private readonly ApplicationDbContext _context;

		public BookingRepos(ApplicationDbContext context)
		{
			_context = context;
		}

		public async Task<List<Booking>> GetBookingsByInstructorAsync(string instructorEmail)
		{
			return await _context.Bookings
				.Where(b => b.InstructorEmail == instructorEmail)
				.ToListAsync();
		}

		public async Task<List<Booking>> GetBookingsByStudentAsync(string studentEmail)
		{
			return await _context.Bookings
				.Where(b => b.StudentEmail == studentEmail)
				.ToListAsync();
		}

		public async Task<bool> AddBookingAsync(Booking booking)
		{
			var duration = booking.End - booking.Start;
			if (duration.TotalHours != 1 && duration.TotalHours != 2)
				return false; // Only 1 or 2 hour bookings allowed

			// Check if booking fits within availability
			var availabilityBlocks = await _context.Availability
				.Where(a => a.InstructorEmail == booking.InstructorEmail &&
							a.Start.Date == booking.Start.Date)
				.ToListAsync();

			var fitsInAvailability = availabilityBlocks.Any(a =>
				booking.Start >= a.Start && booking.End <= a.End);

			if (!fitsInAvailability)
				return false;

			// Check for overlapping bookings
			var overlappingBookings = await _context.Bookings
				.AnyAsync(b => b.InstructorEmail == booking.InstructorEmail &&
							   b.Start < booking.End &&
							   booking.Start < b.End);

			if (overlappingBookings)
				return false;

			await _context.Bookings.AddAsync(booking);
			await _context.SaveChangesAsync();
			return true;
		}


		public async Task RemoveBookingAsync(string bookingId)
		{
			var booking = await _context.Bookings.FindAsync(bookingId);
			if (booking != null)
			{
				_context.Bookings.Remove(booking);
				await _context.SaveChangesAsync();
			}
		}

		public async Task<List<Booking>> GetAllBookingsAsync()
		{
			return await _context.Bookings.ToListAsync();
		}
	}
}
