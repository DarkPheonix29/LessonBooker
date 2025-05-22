using FirebaseAdmin.Auth;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LBCore.Interfaces;
using LBCore.Models;

namespace LBCore.Controllers
{
	[ApiController]
	[Route("api/admin")]
	public class AdminController : ControllerBase
	{
		private readonly IFirebaseKeyRepos _firebaseKeyRepos;
		private readonly IFirebaseAccountRepos _firebaseAccountRepos;
		private readonly IProfileRepos _profileRepos;

		public AdminController(
			IFirebaseKeyRepos firebaseKeyRepos,
			IFirebaseAccountRepos firebaseAccountRepos,
			IProfileRepos profileRepos)
		{
			_firebaseKeyRepos = firebaseKeyRepos;
			_firebaseAccountRepos = firebaseAccountRepos;
			_profileRepos = profileRepos;
		}

		// 1. Create a new registration key
		[HttpPost("generate-key")]
		public async Task<IActionResult> GenerateRegistrationKey()
		{
			try
			{
				var key = await _firebaseKeyRepos.GenerateRegistrationKeyAsync();
				return Ok(new { Key = key });
			}
			catch (Exception ex)
			{
				return BadRequest(new { Message = ex.Message });
			}
		}

		// 2. Get all current students (fetch profiles)
		[HttpGet("students")]
		public async Task<IActionResult> GetAllStudents()
		{
			try
			{
				var profiles = await _profileRepos.GetAllProfilesAsync();
				return Ok(profiles);
			}
			catch (Exception ex)
			{
				return BadRequest(new { Message = ex.Message });
			}
		}

		// 3. Revoke a student's access (log them out and update role)
		[HttpPost("revoke-access/{uid}")]
		public async Task<IActionResult> RevokeStudentAccess(string uid)
		{
			try
			{
				// Log out user
				await _firebaseAccountRepos.LogoutUserAsync(uid);

				// Optionally, update the role to revoke access (e.g., set role to 'revoked' or similar)
				await _firebaseAccountRepos.AssignRoleAsync(uid, "revoked");

				return Ok(new { Message = "Access revoked successfully." });
			}
			catch (Exception ex)
			{
				return BadRequest(new { Message = ex.Message });
			}
		}

		// 4. Update a student's profile
		[HttpPut("update-profile")]
		public async Task<IActionResult> UpdateStudentProfile([FromBody] Profiles profile)
		{
			try
			{
				// Check if the profile exists
				var existingProfile = await _profileRepos.GetProfileByEmailAsync(profile.Email);
				if (existingProfile == null)
				{
					return NotFound(new { Message = "Profile not found." });
				}

				// Update profile information
				await _profileRepos.UpdateProfileAsync(profile);

				return Ok(new { Message = "Profile updated successfully." });
			}
			catch (Exception ex)
			{
				return BadRequest(new { Message = ex.Message });
			}
		}
		// Add this method inside your AdminController class

		[HttpGet("keys")]
		public async Task<IActionResult> GetAllKeys()
		{
			try
			{
				var keys = await _firebaseKeyRepos.GetAllKeysAsync();
				return Ok(keys);
			}
			catch (Exception ex)
			{
				return BadRequest(new { Message = ex.Message });
			}
		}

	}
}
