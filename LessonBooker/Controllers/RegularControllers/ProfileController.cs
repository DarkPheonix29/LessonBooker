using System.Threading.Tasks;
using LBCore.Models;
using LBCore.Managers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace LBAPI.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class ProfileController : ControllerBase
	{
		private readonly AccountManager _accountManager;
		private readonly ILogger<ProfileController> _logger;

		public ProfileController(AccountManager accountManager, ILogger<ProfileController> logger)
		{
			_accountManager = accountManager;
			_logger = logger;
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
				_logger.LogError(ex, "Error retrieving profile for email: {Email}", email);
				return StatusCode(500, "Internal server error");
			}
		}


		// Get all profiles
		[HttpGet]
		public async Task<IActionResult> GetAllProfiles()
		{
			var profiles = await _accountManager.GetAllProfilesAsync();
			return Ok(profiles);
		}

		[HttpPost]
		public async Task<IActionResult> AddProfile([FromBody] Profiles profile)
		{
			if (profile == null)
			{
				_logger.LogWarning("Profile object received is null.");
				return BadRequest("Profile data is required.");
			}

			try
			{
				_logger.LogInformation("Attempting to add profile with email: {Email}", profile.Email);

				await _accountManager.AddProfileAsync(profile);
				_logger.LogInformation("Profile created successfully for email: {Email}", profile.Email);

				return CreatedAtAction(nameof(GetProfileByEmail), new { email = profile.Email }, profile);
			}
			catch (Exception ex)
			{
				if (ex.Message.Contains("Profile already exists"))
				{
					return Conflict(new { message = "Profile already exists with this email." });
				}
				_logger.LogError(ex, "Error occurred while creating profile for email: {Email}", profile.Email);
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
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error occurred while updating profile with ID: {ProfileId}", profileId);

				if (ex.Message.Contains("Profile not found"))
				{
					return NotFound(new { message = "Profile not found." });
				}

				return StatusCode(500, "Internal server error");
			}
		}
	}
}
