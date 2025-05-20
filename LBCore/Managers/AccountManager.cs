using LBCore.Interfaces;
using LBCore.Models;
using System;

namespace LBCore.Managers
{
	public class AccountManager
	{
		private readonly IProfileRepos _profileRepos;

		public AccountManager(IProfileRepos profileRepos)
		{
			_profileRepos = profileRepos;
		}

		// Get profile by email
		public async Task<Profiles> GetProfileByEmailAsync(string email)
		{
			if (string.IsNullOrWhiteSpace(email))
			{
				throw new ArgumentException("Email must not be empty.", nameof(email));
			}

			var profile = await _profileRepos.GetProfileByEmailAsync(email);
			if (profile == null)
			{
				throw new InvalidOperationException("Profile not found.");
			}
			return profile;
		}

		// Get all profiles
		public async Task<IEnumerable<Profiles>> GetAllProfilesAsync()
		{
			return await _profileRepos.GetAllProfilesAsync();
		}

		// Add a new profile
		public async Task AddProfileAsync(Profiles profile)
		{
			ValidateProfile(profile);

			var existingProfile = await _profileRepos.GetProfileByEmailAsync(profile.Email);
			if (existingProfile != null)
			{
				throw new InvalidOperationException("Profile already exists with this email.");
			}
			await _profileRepos.AddProfileAsync(profile);
		}

		// Update an existing profile
		public async Task UpdateProfileAsync(Profiles profile)
		{
			ValidateProfile(profile);

			var existingProfile = await _profileRepos.GetProfileByEmailAsync(profile.Email);
			if (existingProfile == null)
			{
				throw new InvalidOperationException("Profile not found.");
			}
			await _profileRepos.UpdateProfileAsync(profile);
		}

		private void ValidateProfile(Profiles profile)
		{
			if (profile == null)
			{
				throw new ArgumentNullException(nameof(profile), "Profile must not be null.");
			}

			if (string.IsNullOrWhiteSpace(profile.Email))
			{
				throw new ArgumentException("Email is required.", nameof(profile.Email));
			}
			if (string.IsNullOrWhiteSpace(profile.DisplayName))
			{
				throw new ArgumentException("DisplayName is required.", nameof(profile.DisplayName));
			}
			if (string.IsNullOrWhiteSpace(profile.PhoneNumber))
			{
				throw new ArgumentException("PhoneNumber is required.", nameof(profile.PhoneNumber));
			}
			if (string.IsNullOrWhiteSpace(profile.Address))
			{
				throw new ArgumentException("Address is required.", nameof(profile.Address));
			}
			if (string.IsNullOrWhiteSpace(profile.PickupAddress))
			{
				throw new ArgumentException("PickupAddress is required.", nameof(profile.PickupAddress));
			}
			if (profile.DateOfBirth == default)
			{
				throw new ArgumentException("DateOfBirth is required.", nameof(profile.DateOfBirth));
			}
		}
	}
}
