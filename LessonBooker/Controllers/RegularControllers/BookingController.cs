using LBCore.Interfaces;
using LBCore.Models;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace LessonBooker.Controllers.RegularControllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class BookingController : ControllerBase
	{
		private readonly IBookingRepos _bookingRepos;

		public BookingController(IBookingRepos bookingRepos)
		{
			_bookingRepos = bookingRepos;
		}

		// GET: api/bookings/instructor/{instructorEmail}
		[HttpGet("instructor/{instructorEmail}")]
		public async Task<IActionResult> GetBookingsByInstructor(string instructorEmail)
		{
			var bookings = await _bookingRepos.GetBookingsByInstructorAsync(instructorEmail);
			if (bookings == null || bookings.Count == 0)
			{
				return NotFound("No bookings found for this instructor.");
			}
			return Ok(bookings);
		}

		// GET: api/bookings/student/{studentEmail}
		[HttpGet("student/{studentEmail}")]
		public async Task<IActionResult> GetBookingsByStudent(string studentEmail)
		{
			var bookings = await _bookingRepos.GetBookingsByStudentAsync(studentEmail);
			if (bookings == null || bookings.Count == 0)
			{
				return NotFound("No bookings found for this student.");
			}
			return Ok(bookings);
		}

		// POST: api/bookings
		[HttpPost]
		public async Task<IActionResult> AddBooking([FromBody] Booking booking)
		{
			var success = await _bookingRepos.AddBookingAsync(booking);
			if (!success)
			{
				return BadRequest("Booking conflict with another instructor or time slot.");
			}
			return CreatedAtAction(nameof(GetBookingsByInstructor), new { instructorEmail = booking.InstructorEmail }, booking);
		}

		// DELETE: api/bookings/{id}
		[HttpDelete("{id}")]
		public async Task<IActionResult> RemoveBooking(string id)
		{
			await _bookingRepos.RemoveBookingAsync(id);
			return NoContent();
		}
	}
}
