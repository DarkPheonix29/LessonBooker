using FirebaseAdmin.Auth;
using LBCore.Managers;
using LBCore.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Google.Cloud.Firestore;
using LBCore;

namespace LessonBooker.Controllers.FirebaseControllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class AccountController : ControllerBase
	{
		private readonly FirebaseManager _firebaseManager;
		private readonly IFirebaseKeyRepos _firebaseKeyRepos;
		private readonly FirestoreDb _firestoreDb;

		public AccountController(FirebaseManager firebaseManager, IFirebaseKeyRepos firebaseKeyRepos)
		{
			_firebaseManager = firebaseManager;
			_firebaseKeyRepos = firebaseKeyRepos;
			_firestoreDb = FirestoreFactory.GetFirestoreDb();
		}

		/// <summary>
		/// Verifies the provided Firebase ID token.
		/// </summary>
		[HttpPost("verify")]
		public async Task<IActionResult> VerifyToken([FromBody] TokenRequest request)
		{
			try
			{
				bool verified = await _firebaseManager.VerifyTokenAsync(request.IdToken);
				if (!verified)
				{
					return Unauthorized(new { message = "Invalid or expired token." });
				}

				var uid = await _firebaseManager.LoginAsync(request.IdToken);
				var userRecord = await FirebaseAuth.DefaultInstance.GetUserAsync(uid);
				var role = userRecord.CustomClaims != null && userRecord.CustomClaims.ContainsKey("role")
						   ? userRecord.CustomClaims["role"].ToString()
						   : "student";

				return Ok(new { message = "Token verified successfully", uid, role });
			}
			catch (Exception ex)
			{
				return Unauthorized(new { message = $"Error: {ex.Message}" });
			}
		}

		/// <summary>
		/// Signs up a new user with a registration key validation.
		/// </summary>
		[HttpPost("signup")]
		public async Task<IActionResult> SignUp([FromBody] SignUpRequest request)
		{
			try
			{
				bool isKeyValid = await _firebaseKeyRepos.UseRegistrationKeyAsync(request.RegistrationKey);
				if (!isKeyValid)
				{
					return BadRequest(new { message = "Invalid or used registration key." });
				}

				var user = await _firebaseManager.SignUpAsync(request.Email, request.Password, "student");
				return Ok(new { message = "User signed up successfully.", uid = user.Uid });
			}
			catch (Exception ex)
			{
				return BadRequest(new { message = ex.Message });
			}
		}

		[HttpPost("login")]
		public async Task<IActionResult> Login([FromBody] TokenRequest request)
		{
			try
			{
				var uid = await _firebaseManager.LoginAsync(request.IdToken);

				var userDoc = _firestoreDb.Collection("users").Document(uid);
				var userSnapshot = await userDoc.GetSnapshotAsync();

				if (!userSnapshot.Exists)
				{
					return Unauthorized(new { message = "User not found." });
				}

				var userData = userSnapshot.ToDictionary();
				var role = userData.ContainsKey("role") ? userData["role"].ToString() : "student";

				var userRecord = await FirebaseAuth.DefaultInstance.GetUserAsync(uid);

				// No cookie logic here; frontend should use the ID token as a Bearer token
				return Ok(new { message = "Logged in successfully.", role, uid, email = userRecord.Email });
			}
			catch (Exception ex)
			{
				return Unauthorized(new { message = ex.Message });
			}
		}

		[Authorize]
		[HttpPost("logout")]
		public async Task<IActionResult> Logout()
		{
			try
			{
				var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("user_id")?.Value;
				if (!string.IsNullOrEmpty(userId))
				{
					await _firebaseManager.LogoutAsync(userId);
				}
				// No cookie to clear
				return Ok(new { message = "Logged out successfully." });
			}
			catch (Exception ex)
			{
				return BadRequest(new { message = ex.Message });
			}
		}

		[Authorize]
		[HttpGet("me")]
		public async Task<IActionResult> Me()
		{
			var uid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("user_id")?.Value;
			var email = User.FindFirst(ClaimTypes.Email)?.Value;

			// Fetch role from Firestore
			var userDoc = _firestoreDb.Collection("users").Document(uid);
			var userSnapshot = await userDoc.GetSnapshotAsync();
			string role = null;
			if (userSnapshot.Exists)
			{
				var userData = userSnapshot.ToDictionary();
				role = userData.ContainsKey("role") ? userData["role"].ToString() : null;
			}

			return Ok(new { uid, email, role });
		}

	}

	public class SignUpRequest
	{
		public string Email { get; set; }
		public string Password { get; set; }
		public string RegistrationKey { get; set; }
	}

	public class TokenRequest
	{
		public string IdToken { get; set; }
	}
}
