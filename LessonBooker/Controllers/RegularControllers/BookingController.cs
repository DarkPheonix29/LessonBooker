using LBCore.Managers;
using LBCore.Models;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using LBCore.Interfaces;

namespace LessonBooker.Controllers.RegularControllers
{
	[Route("api/[controller]")]
	[ApiController]
	[Authorize] // Require authentication for all actions
	public class BookingController : ControllerBase
	{
		private readonly CalendarManager _calendarManager;
		private readonly IFirebaseAccountRepos _firebaseAccountRepos;

		public BookingController(CalendarManager calendarManager, IFirebaseAccountRepos firebaseAccountRepos)
		{
			_calendarManager = calendarManager;
			_firebaseAccountRepos = firebaseAccountRepos;
		}

		// Helper: Get current user's role
		private async Task<string?> GetCurrentUserRoleAsync()
		{
			var uid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (string.IsNullOrEmpty(uid))
				return null;
			return await _firebaseAccountRepos.GetUserRoleAsync(uid);
		}

		// GET: api/bookings/instructor/{instructorEmail}
		[HttpGet("instructor/{instructorEmail}")]
		public async Task<IActionResult> GetBookingsByInstructor(string instructorEmail)
		{
			// Both instructor and student can access
			var role = await GetCurrentUserRoleAsync();
			if (role != "instructor" && role != "student")
				return Forbid();

			var bookings = await _calendarManager.GetBookingsByInstructorAsync(instructorEmail);
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
			// Both instructor and student can access
			var role = await GetCurrentUserRoleAsync();
			if (role != "instructor" && role != "student")
				return Forbid();

			var bookings = await _calendarManager.GetBookingsByStudentAsync(studentEmail);
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
			// Only student can add
			var role = await GetCurrentUserRoleAsync();
			if (role != "student")
				return Forbid();

			var success = await _calendarManager.AddBookingAsync(booking);
			if (!success)
			{
				return BadRequest("Booking conflict with another instructor or time slot.");
			}
			return CreatedAtAction(nameof(GetBookingsByInstructor), new { instructorEmail = booking.InstructorEmail }, booking);
		}

		// DELETE: api/bookings/{id}
		[HttpDelete("{id}")]
		public async Task<IActionResult> RemoveBooking(int id)
		{
			// Only student can remove
			var role = await GetCurrentUserRoleAsync();
			if (role != "student")
				return Forbid();

			await _calendarManager.RemoveBookingAsync(id);
			return NoContent();
		}

		[HttpGet("all-bookings")]
		public async Task<IActionResult> GetAllBookings()
		{
			// Both instructor and student can access
			var role = await GetCurrentUserRoleAsync();
			if (role != "instructor" && role != "student")
				return Forbid();

			var bookings = await _calendarManager.GetAllBookingsAsync();
			return Ok(bookings);
		}
	}
}
