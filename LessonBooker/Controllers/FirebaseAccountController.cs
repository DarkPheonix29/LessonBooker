﻿using FirebaseAdmin.Auth;
using LBCore.Managers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ScheduleApp.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class AccountController : ControllerBase
	{
		private readonly FirebaseManager _firebaseManager;

		public AccountController(FirebaseManager firebaseManager)
		{
			_firebaseManager = firebaseManager;
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


		[HttpPost("signup")]
		public async Task<IActionResult> SignUp([FromBody] SignUpRequest request)
		{
			try
			{
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

				return Ok(new { message = "Logged in successfully.", uid });
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
	}


	public class SignUpRequest
	{
		public string Email { get; set; }
		public string Password { get; set; }
	}

	public class TokenRequest
	{
		public string IdToken { get; set; }
	}
}
