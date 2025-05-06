using LBCore.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LBCore.Interfaces
{
	public interface IBookingRepos
	{
		Task<List<Booking>> GetBookingsByInstructorAsync(string instructorEmail);
		Task<List<Booking>> GetBookingsByStudentAsync(string studentEmail);
		Task<bool> AddBookingAsync(Booking booking);
		Task RemoveBookingAsync(string bookingId);
	}
}
