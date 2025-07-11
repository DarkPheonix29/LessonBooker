﻿using LBCore.Managers;
using LBCore.Models;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using LBCore.Interfaces;
using LessonBooker.Hubs; // Add this
using Microsoft.AspNetCore.SignalR; // Add this

namespace LessonBooker.Controllers.RegularControllers
{
	[Route("api/[controller]")]
	[ApiController]
	[Authorize] // Require authentication for all actions
	public class AvailabilityController : ControllerBase
	{
		private readonly CalendarManager _calendarManager;
		private readonly IFirebaseAccountRepos _firebaseAccountRepos;
		private readonly IHubContext<CalendarHub> _hubContext; // Add this

		public AvailabilityController(
			CalendarManager calendarManager,
			IFirebaseAccountRepos firebaseAccountRepos,
			IHubContext<CalendarHub> hubContext) // Add this
		{
			_calendarManager = calendarManager;
			_firebaseAccountRepos = firebaseAccountRepos;
			_hubContext = hubContext; // Add this
		}

		// Helper: Get current user's role
		private async Task<string?> GetCurrentUserRoleAsync()
		{
			var uid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (string.IsNullOrEmpty(uid))
			{
				return null;
			}
			return await _firebaseAccountRepos.GetUserRoleAsync(uid);
		}

		// GET: api/availability/{instructorEmail}
		[HttpGet("{instructorEmail}")]
		public async Task<IActionResult> GetAvailability(string instructorEmail)
		{
			// Both instructor and student can access
			var role = await GetCurrentUserRoleAsync();
			if (role != "instructor" && role != "student")
			{
				return Forbid();
			}

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
			// Only instructor can add
			var role = await GetCurrentUserRoleAsync();
			if (role != "instructor")
			{
				return Forbid();
			}

			await _calendarManager.AddAvailabilityAsync(availability);

			// Notify all clients about the update
			await _hubContext.Clients.All.SendAsync("CalendarUpdated");

			return CreatedAtAction(nameof(GetAvailability), new { instructorEmail = availability.InstructorEmail }, availability);
		}

		// DELETE: api/availability/{id}
		[HttpDelete("{id}")]
		public async Task<IActionResult> RemoveAvailability(int id)
		{
			// Only instructor can remove
			var role = await GetCurrentUserRoleAsync();
			if (role != "instructor")
			{
				return Forbid();
			}

			await _calendarManager.RemoveAvailabilityAsync(id);

			// Notify all clients about the update
			await _hubContext.Clients.All.SendAsync("CalendarUpdated");

			return NoContent();
		}

		[HttpGet("all-availability")]
		public async Task<IActionResult> GetAllInstructorAvailability()
		{
			// Both instructor and student can access
			var role = await GetCurrentUserRoleAsync();
			if (role != "instructor" && role != "student" && role != "admin")
			{
				return Forbid();
			}

			var availabilityList = await _calendarManager.GetAllAvailabilityAsync();
			return Ok(availabilityList);
		}
	}
}
