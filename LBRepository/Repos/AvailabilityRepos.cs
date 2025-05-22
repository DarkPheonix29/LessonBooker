using LBCore.Interfaces;
using LBCore.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LBRepository.Repos
{
	public class AvailabilityRepos : IAvailabilityRepos
	{
		private readonly ApplicationDbContext _context;

		public AvailabilityRepos(ApplicationDbContext context)
		{
			_context = context;
		}

		public async Task<List<Availability>> GetAvailabilityByInstructorAsync(string instructorEmail)
		{
			return await _context.Availability
				.Where(a => a.InstructorEmail == instructorEmail)
				.ToListAsync();
		}

		public async Task AddAvailabilityAsync(Availability availability)
		{
			await _context.Availability.AddAsync(availability);
			await _context.SaveChangesAsync();
		}

		public async Task RemoveAvailabilityAsync(int availabilityId)
		{
			var availability = await _context.Availability.FindAsync(availabilityId);
			if (availability != null)
			{
				_context.Availability.Remove(availability);
				await _context.SaveChangesAsync();
			}
		}

		public async Task<List<Availability>> GetAllAvailabilityAsync()
		{
			return await _context.Availability.ToListAsync();
		}
	}
}
