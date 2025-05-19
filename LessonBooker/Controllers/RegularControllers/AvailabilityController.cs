using LBCore.Interfaces;
using LBCore.Models;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace LessonBooker.Controllers.RegularControllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class AvailabilityController : ControllerBase
	{
		private readonly IAvailabilityRepos _availabilityRepos;

		public AvailabilityController(IAvailabilityRepos availabilityRepos)
		{
			_availabilityRepos = availabilityRepos;
		}

		// GET: api/availability/{instructorEmail}
		[HttpGet("{instructorEmail}")]
		public async Task<IActionResult> GetAvailability(string instructorEmail)
		{
			var availability = await _availabilityRepos.GetAvailabilityByInstructorAsync(instructorEmail);
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
			await _availabilityRepos.AddAvailabilityAsync(availability);
			return CreatedAtAction(nameof(GetAvailability), new { instructorEmail = availability.InstructorEmail }, availability);
		}

		// DELETE: api/availability/{id}
		[HttpDelete("{id}")]
		public async Task<IActionResult> RemoveAvailability(int id)
		{
			await _availabilityRepos.RemoveAvailabilityAsync(id);
			return NoContent();
		}

		[HttpGet("all-availability")]
		public async Task<IActionResult> GetAllInstructorAvailability()
		{
			var availabilityList = await _availabilityRepos.GetAllAvailabilityAsync();
			return Ok(availabilityList);
		}

	}
}
