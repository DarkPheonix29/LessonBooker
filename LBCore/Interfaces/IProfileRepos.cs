using System.Collections.Generic;
using System.Threading.Tasks;
using LBCore.Models;

namespace LBCore.Interfaces
{
	public interface IProfileRepos
	{
		Task<Profiles> GetProfileByEmailAsync(string email);
		Task<IEnumerable<Profiles>> GetAllProfilesAsync();
		Task AddProfileAsync(Profiles profile);
		Task UpdateProfileAsync(Profiles profile);
	}
}
