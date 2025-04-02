using System.Threading.Tasks;
using LBCore.Models;
using LBCore.Managers;
using Microsoft.AspNetCore.Mvc;

namespace LBAPI.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class ProfileController : ControllerBase
	{
		private readonly AccountManager _accountManager;

		public ProfileController(AccountManager accountManager)
		{
			_accountManager = accountManager;
		}

		// Get profile by email
		[HttpGet("{email}")]
		public async Task<IActionResult> GetProfileByEmail(string email)
		{
			try
			{
				var profile = await _accountManager.GetProfileByEmailAsync(email);
				return Ok(profile);
			}
			catch (System.Exception)
			{
				return NotFound(); // Profile not found
			}
		}

		// Get all profiles
		[HttpGet]
		public async Task<IActionResult> GetAllProfiles()
		{
			var profiles = await _accountManager.GetAllProfilesAsync();
			return Ok(profiles);
		}

		// Add a new profile
		[HttpPost]
		public async Task<IActionResult> AddProfile([FromBody] Profiles profile)
		{
			if (profile == null)
			{
				return BadRequest("Profile data is required.");
			}

			try
			{
				await _accountManager.AddProfileAsync(profile);
				return CreatedAtAction(nameof(GetProfileByEmail), new { email = profile.Email }, profile);
			}
			catch (System.Exception ex)
			{
				if (ex.Message.Contains("Profile already exists"))
				{
					return Conflict(new { message = "Profile already exists with this email." });
				}

				return StatusCode(500, "Internal server error");
			}
		}


		// Update an existing profile
		[HttpPut("{profileId}")]
		public async Task<IActionResult> UpdateProfile(int profileId, [FromBody] Profiles profile)
		{
			if (profileId != profile.ProfileId)
			{
				return BadRequest("Profile ID mismatch.");
			}

			try
			{
				await _accountManager.UpdateProfileAsync(profile);
				return NoContent(); // Successful update, no content returned
			}
			catch (System.Exception ex)
			{
				if (ex.Message.Contains("Profile not found"))
				{
					return BadRequest(new { message = "Profile not found." });
				}

				return StatusCode(500, "Internal server error");
			}
		}
	}
}
