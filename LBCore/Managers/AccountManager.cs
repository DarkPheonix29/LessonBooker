using LBCore.Interfaces;
using LBCore.Models;

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
			var profile = await _profileRepos.GetProfileByEmailAsync(email);
			return profile; // Return null if profile is not found
		}

		// Get all profiles
		public async Task<IEnumerable<Profiles>> GetAllProfilesAsync()
		{
			return await _profileRepos.GetAllProfilesAsync();
		}

		// Add a new profile
		public async Task AddProfileAsync(Profiles profile)
		{
			var existingProfile = await _profileRepos.GetProfileByEmailAsync(profile.Email);
			if (existingProfile != null)
			{
				throw new Exception("Profile already exists with this email.");
			}
			await _profileRepos.AddProfileAsync(profile);
		}

		// Update an existing profile
		public async Task UpdateProfileAsync(Profiles profile)
		{
			var existingProfile = await _profileRepos.GetProfileByEmailAsync(profile.Email);
			if (existingProfile == null)
			{
				throw new Exception("Profile not found.");
			}
			await _profileRepos.UpdateProfileAsync(profile);
		}
	}
}
