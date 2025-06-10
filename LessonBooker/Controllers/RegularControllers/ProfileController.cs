using System.Threading.Tasks;
using LBCore.Models;
using LBCore.Managers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using LBCore.Interfaces;

namespace LBAPI.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	[Authorize] // Require authentication for all actions
	public class ProfileController : ControllerBase
	{
		private readonly AccountManager _accountManager;
		private readonly IFirebaseAccountRepos _firebaseAccountRepos;

		public ProfileController(AccountManager accountManager, IFirebaseAccountRepos firebaseAccountRepos)
		{
			_accountManager = accountManager;
			_firebaseAccountRepos = firebaseAccountRepos;
		}

		// Helper: Get current user's UID
		private string? GetCurrentUid()
		{
			return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
		}

		// Helper: Get current user's role
		private async Task<string?> GetCurrentUserRoleAsync()
		{
			var uid = GetCurrentUid();
			if (string.IsNullOrEmpty(uid))
			{
				return null;
			}
			return await _firebaseAccountRepos.GetUserRoleAsync(uid);
		}

		[HttpGet("{email}")]
		public async Task<IActionResult> GetProfileByEmail(string email)
		{
			try
			{
				var profile = await _accountManager.GetProfileByEmailAsync(email);
				return Ok(profile);
			}
			catch (Exception ex)
			{
				if (ex.Message.Contains("Profile not found"))
				{
					return NotFound(new { message = "Profile not found." });
				}
				return StatusCode(500, "Internal server error");
			}
		}

		// Get all profiles (admin only)
		[HttpGet]
		public async Task<IActionResult> GetAllProfiles()
		{
			var role = await GetCurrentUserRoleAsync();
			if (role != "admin")
			{
				return Forbid();
			}

			var profiles = await _accountManager.GetAllProfilesAsync();
			return Ok(profiles);
		}

		// Only authenticated users (with an account) can add a profile
		[HttpPost]
		public async Task<IActionResult> AddProfile([FromBody] Profiles profile)
		{
			var uid = GetCurrentUid();
			if (string.IsNullOrEmpty(uid))
			{
				return Forbid();
			}

			if (profile == null)
			{
				return BadRequest("Profile data is required.");
			}

			try
			{
				await _accountManager.AddProfileAsync(profile);
				return CreatedAtAction(nameof(GetProfileByEmail), new { email = profile.Email }, profile);
			}
			catch (Exception ex)
			{
				if (ex.Message.Contains("Profile already exists"))
				{
					return Conflict(new { message = "Profile already exists with this email." });
				}
				return StatusCode(500, "Internal server error");
			}
		}

		// Update an existing profile (admin only)
		[HttpPut("{profileId}")]
		public async Task<IActionResult> UpdateProfile(int profileId, [FromBody] Profiles profile)
		{
			var role = await GetCurrentUserRoleAsync();
			if (role != "admin")
				return Forbid();

			if (profileId != profile.ProfileId)
			{
				return BadRequest("Profile ID mismatch.");
			}

			try
			{
				await _accountManager.UpdateProfileAsync(profile);
				return NoContent(); // Successful update, no content returned
			}
			catch (Exception ex)
			{
				if (ex.Message.Contains("Profile not found"))
				{
					return NotFound(new { message = "Profile not found." });
				}

				return StatusCode(500, "Internal server error");
			}
		}
	}
}
