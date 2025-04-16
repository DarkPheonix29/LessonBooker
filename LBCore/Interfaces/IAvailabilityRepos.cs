using LBCore.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LBCore.Interfaces
{
	public interface IAvailabilityRepos
	{
		Task<List<Availability>> GetAvailabilityByInstructorAsync(string instructorEmail);
		Task AddAvailabilityAsync(Availability availability);
		Task RemoveAvailabilityAsync(string availabilityId);
	}
}
