using FirebaseAdmin.Auth;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LBCore.Interfaces;
using LBCore.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Google.Cloud.Firestore;

namespace LBCore.Controllers
{
	[ApiController]
	[Route("api/admin")]
	[Authorize] // Require authentication for all actions
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

		// Helper: Check if current user is admin
		private async Task<bool> IsCurrentUserAdminAsync()
		{
			var uid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (string.IsNullOrEmpty(uid))
			{
				return false;
			}

			var role = await _firebaseAccountRepos.GetUserRoleAsync(uid);
			return role == "admin";
		}

		[HttpPost("generate-key")]
		public async Task<IActionResult> GenerateRegistrationKey()
		{
			if (!await IsCurrentUserAdminAsync())
			{
				return Forbid();
			}

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

		[HttpGet("students")]
		public async Task<IActionResult> GetAllStudents()
		{
			if (!await IsCurrentUserAdminAsync())
			{
				return Forbid();
			}

			try
			{
				var profiles = await _profileRepos.GetAllProfilesAsync();
				var firestore = FirestoreFactory.GetFirestoreDb(); // Use your factory or inject FirestoreDb
				var studentsWithUid = new List<Dictionary<string, object>>();

				foreach (var profile in profiles)
				{
					string uid = null;
					string role = null;

					// Query Firestore for the user by email
					var userQuery = await firestore.Collection("users")
						.WhereEqualTo("email", profile.Email)
						.Limit(1)
						.GetSnapshotAsync();

					if (userQuery.Documents.Count > 0)
					{
						var userDoc = userQuery.Documents[0];
						uid = userDoc.Id;
						if (userDoc.ContainsField("role"))
						{
							role = userDoc.GetValue<string>("role");
						}
					}

					// Only add if role is "student"
					if (role == "student")
					{
						var profileWithUid = new Dictionary<string, object>();
						foreach (var prop in profile.GetType().GetProperties())
						{
							profileWithUid[prop.Name] = prop.GetValue(profile);
						}
						profileWithUid["uid"] = uid;
						studentsWithUid.Add(profileWithUid);
					}
				}

				return Ok(studentsWithUid);
			}
			catch (Exception ex)
			{
				return BadRequest(new { Message = ex.Message });
			}
		}

		[HttpPost("revoke-access/{uid}")]
		public async Task<IActionResult> RevokeStudentAccess(string uid)
		{
			if (!await IsCurrentUserAdminAsync())
			{
				return Forbid();
			}

			try
			{
				await _firebaseAccountRepos.DeleteUserAsync(uid);
				return Ok(new { Message = "User deleted from Firebase Authentication." });
			}
			catch (Exception ex)
			{
				return BadRequest(new { Message = ex.Message });
			}
		}


		[HttpPut("update-profile")]
		public async Task<IActionResult> UpdateStudentProfile([FromBody] Profiles profile)
		{
			if (!await IsCurrentUserAdminAsync())
			{
				return Forbid();
			}

			try
			{
				var existingProfile = await _profileRepos.GetProfileByEmailAsync(profile.Email);
				if (existingProfile == null)
				{
					return NotFound(new { Message = "Profile not found." });
				}

				await _profileRepos.UpdateProfileAsync(profile);
				return Ok(new { Message = "Profile updated successfully." });
			}
			catch (Exception ex)
			{
				return BadRequest(new { Message = ex.Message });
			}
		}

		[HttpGet("keys")]
		public async Task<IActionResult> GetAllKeys()
		{
			if (!await IsCurrentUserAdminAsync())
			{
				return Forbid();
			}

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
