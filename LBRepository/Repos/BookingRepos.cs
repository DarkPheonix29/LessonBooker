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
			var overlappingBookings = await _context.Bookings
				.AnyAsync(b => b.InstructorEmail == booking.InstructorEmail &&
							   b.Start < booking.End &&
							   booking.Start < b.End);

			if (overlappingBookings)
			{
				return false; // Conflict with another booking
			}

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
	}
}
