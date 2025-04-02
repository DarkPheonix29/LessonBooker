using LBCore.Interfaces;
using LBCore.Models;
using Microsoft.EntityFrameworkCore;

namespace LBRepository.Repos
{
	public class ProfileRepos : IProfileRepos
	{
		private readonly ApplicationDbContext _context;

		public ProfileRepos(ApplicationDbContext context)
		{
			_context = context;
		}

		// Get profile by email
		public async Task<Profiles> GetProfileByEmailAsync(string email)
		{
			var profile = await _context.Profile
				.FirstOrDefaultAsync(p => p.Email == email);

			if (profile == null)
			{
				throw new Exception("Profile not found.");
			}

			return profile;
		}

		// Get all profiles
		public async Task<IEnumerable<Profiles>> GetAllProfilesAsync()
		{
			return await _context.Profile.ToListAsync();
		}

		// Add a new profile
		public async Task AddProfileAsync(Profiles profile)
		{
			var existingProfile = await _context.Profile
				.FirstOrDefaultAsync(p => p.Email == profile.Email);

			if (existingProfile != null)
			{
				throw new Exception("Profile already exists with this email.");
			}

			await _context.Profile.AddAsync(profile);
			await _context.SaveChangesAsync();
		}

		// Update an existing profile
		public async Task UpdateProfileAsync(Profiles profile)
		{
			var existingProfile = await _context.Profile
				.FirstOrDefaultAsync(p => p.ProfileId == profile.ProfileId);

			if (existingProfile == null)
			{
				throw new Exception("Profile not found.");
			}

			// Update profile fields
			existingProfile.Email = profile.Email;
			existingProfile.DisplayName = profile.DisplayName;
			existingProfile.PhoneNumber = profile.PhoneNumber;
			existingProfile.Address = profile.Address;
			existingProfile.PickupAddress = profile.PickupAddress;
			existingProfile.DateOfBirth = profile.DateOfBirth;

			await _context.SaveChangesAsync();
		}
	}
}
