using FirebaseAdmin.Auth;
using LBCore.Managers;
using LBCore.Interfaces;
using Microsoft.AspNetCore.Authentication;
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
		private readonly IFirebaseKeyRepos _firebaseKeyRepos;  // Inject the FirebaseKeyRepos service
		private readonly FirestoreDb _firestoreDb;

		public AccountController(FirebaseManager firebaseManager, IFirebaseKeyRepos firebaseKeyRepos)
		{
			_firebaseManager = firebaseManager;
			_firebaseKeyRepos = firebaseKeyRepos;  // Initialize the service
			_firestoreDb = FirestoreFactory.GetFirestoreDb();

		}

		/// <summary>
		/// Verifies the provided Firebase ID token and signs in the user.
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

				var claims = new List<Claim>
				{
					new Claim(ClaimTypes.NameIdentifier, uid),
					new Claim(ClaimTypes.Email, userRecord.Email),
					new Claim(ClaimTypes.Role, role)
				};

				var identity = new ClaimsIdentity(claims, "Firebase");
				var principal = new ClaimsPrincipal(identity);

				await HttpContext.SignInAsync("Firebase", principal, new AuthenticationProperties
				{
					IsPersistent = true,
					ExpiresUtc = DateTime.UtcNow.AddDays(30)
				});

				return Ok(new { message = "Token verified successfully", uid });
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
				// Check if the registration key is valid
				bool isKeyValid = await _firebaseKeyRepos.UseRegistrationKeyAsync(request.RegistrationKey);
				if (!isKeyValid)
				{
					return BadRequest(new { message = "Invalid or used registration key." });
				}

				// For now, we only assign the "student" role.
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
				// Step 1: Verify the ID token
				var uid = await _firebaseManager.LoginAsync(request.IdToken);

				// Step 2: Get the user's role from Firestore
				var userDoc = _firestoreDb.Collection("users").Document(uid);
				var userSnapshot = await userDoc.GetSnapshotAsync();

				if (!userSnapshot.Exists)
				{
					return Unauthorized(new { message = "User not found." });
				}

				var userData = userSnapshot.ToDictionary();
				var role = userData.ContainsKey("role") ? userData["role"].ToString() : "student"; // Default to "student" if role is not found

				// Step 3: Create claims based on the user data
				var userRecord = await FirebaseAuth.DefaultInstance.GetUserAsync(uid);
				var claims = new List<Claim>
		{
			new Claim(ClaimTypes.NameIdentifier, uid),
			new Claim(ClaimTypes.Email, userRecord.Email),
			new Claim(ClaimTypes.Role, role)
		};

				var identity = new ClaimsIdentity(claims, "Firebase");
				var principal = new ClaimsPrincipal(identity);

				await HttpContext.SignInAsync("Firebase", principal, new AuthenticationProperties
				{
					IsPersistent = true,
					ExpiresUtc = DateTime.UtcNow.AddDays(30)
				});

				return Ok(new { message = "Logged in successfully.", role }); // Return the role to the frontend
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
				var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
				if (!string.IsNullOrEmpty(userId))
				{
					await _firebaseManager.LogoutAsync(userId);
				}
				await HttpContext.SignOutAsync();
				return Ok(new { message = "Logged out successfully." });
			}
			catch (Exception ex)
			{
				return BadRequest(new { message = ex.Message });
			}
		}

		[HttpGet("me")]
		public IActionResult Me()
		{
			var uid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			var email = User.FindFirst(ClaimTypes.Email)?.Value;
			var role = User.FindFirst(ClaimTypes.Role)?.Value;
			return Ok(new { uid, email, role });
		}

	}

	public class SignUpRequest
	{
		public string Email { get; set; }
		public string Password { get; set; }
		public string RegistrationKey { get; set; }  // Added field for registration key
	}

	public class TokenRequest
	{
		public string IdToken { get; set; }
	}
}
