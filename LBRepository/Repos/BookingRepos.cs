using LBCore.Interfaces;
using LBCore.Models;
using LBRepository;
using Microsoft.EntityFrameworkCore;

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
		if (!IsValidDuration(booking))
		{
			return false;
		}

		if (!await FitsInAvailabilityAsync(booking))
		{
			return false;
		}

		if (await HasOverlappingBookingsAsync(booking))
		{
			return false;
		}

		await _context.Bookings.AddAsync(booking);
		await _context.SaveChangesAsync();
		return true;
	}

	private bool IsValidDuration(Booking booking)
	{
		var duration = booking.End - booking.Start;
		return duration.TotalHours == 1 || duration.TotalHours == 2;
	}

	private async Task<bool> FitsInAvailabilityAsync(Booking booking)
	{
		var availabilityBlocks = await _context.Availability
			.Where(a => a.InstructorEmail == booking.InstructorEmail &&
						a.Start.Date == booking.Start.Date)
			.ToListAsync();

		return availabilityBlocks.Any(a =>
			booking.Start >= a.Start && booking.End <= a.End);
	}

	private async Task<bool> HasOverlappingBookingsAsync(Booking booking)
	{
		return await _context.Bookings
			.AnyAsync(b => b.InstructorEmail == booking.InstructorEmail &&
						   b.Start < booking.End &&
						   booking.Start < b.End);
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
