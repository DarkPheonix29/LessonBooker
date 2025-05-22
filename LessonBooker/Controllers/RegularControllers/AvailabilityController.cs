using LBCore.Managers;
using LBCore.Models;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace LessonBooker.Controllers.RegularControllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class AvailabilityController : ControllerBase
	{
		private readonly CalendarManager _calendarManager;

		public AvailabilityController(CalendarManager calendarManager)
		{
			_calendarManager = calendarManager;
		}

		// GET: api/availability/{instructorEmail}
		[HttpGet("{instructorEmail}")]
		public async Task<IActionResult> GetAvailability(string instructorEmail)
		{
			var availability = await _calendarManager.GetAvailabilityByInstructorAsync(instructorEmail);
			if (availability == null || availability.Count == 0)
			{
				return NotFound("No availability found for this instructor.");
			}
			return Ok(availability);
		}

		// POST: api/availability
		[HttpPost]
		public async Task<IActionResult> AddAvailability([FromBody] Availability availability)
		{
			await _calendarManager.AddAvailabilityAsync(availability);
			return CreatedAtAction(nameof(GetAvailability), new { instructorEmail = availability.InstructorEmail }, availability);
		}

		// DELETE: api/availability/{id}
		[HttpDelete("{id}")]
		public async Task<IActionResult> RemoveAvailability(int id)
		{
			await _calendarManager.RemoveAvailabilityAsync(id);
			return NoContent();
		}

		[HttpGet("all-availability")]
		public async Task<IActionResult> GetAllInstructorAvailability()
		{
			var availabilityList = await _calendarManager.GetAllAvailabilityAsync();
			return Ok(availabilityList);
		}
	}
}
