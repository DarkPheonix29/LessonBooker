using LBRepository;
using LBRepository.Repos;
using LBCore.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using Xunit;

namespace LBTesting.Integration
{
	public class AvailabilityReposTests : IDisposable
	{
		private readonly ApplicationDbContext _context;
		private readonly AvailabilityRepos _repo;

		public AvailabilityReposTests()
		{
			var options = new DbContextOptionsBuilder<ApplicationDbContext>()
				.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
				.Options;
			_context = new ApplicationDbContext(options);
			_repo = new AvailabilityRepos(_context);
		}

		[Fact]
		public async Task AddAvailabilityAsync_AddsAvailability()
		{
			var availability = new Availability
			{
				InstructorEmail = "instructor@example.com",
				Start = DateTime.UtcNow.Date.AddDays(1).AddHours(9),
				End = DateTime.UtcNow.Date.AddDays(1).AddHours(17)
			};

			await _repo.AddAvailabilityAsync(availability);

			var found = await _context.Availability.FirstOrDefaultAsync(a => a.InstructorEmail == "instructor@example.com");
			Assert.NotNull(found);
			Assert.Equal(availability.Start, found.Start);
		}

		[Fact]
		public async Task GetAvailabilityByInstructorAsync_ReturnsAvailability()
		{
			var availability = new Availability
			{
				InstructorEmail = "instructor2@example.com",
				Start = DateTime.UtcNow.Date.AddDays(2).AddHours(10),
				End = DateTime.UtcNow.Date.AddDays(2).AddHours(12)
			};
			_context.Availability.Add(availability);
			await _context.SaveChangesAsync();

			var result = await _repo.GetAvailabilityByInstructorAsync("instructor2@example.com");
			Assert.Single(result);
			Assert.Equal("instructor2@example.com", result[0].InstructorEmail);
		}

		[Fact]
		public async Task GetAvailabilityByInstructorAsync_NoResults_ReturnsEmptyList()
		{
			var result = await _repo.GetAvailabilityByInstructorAsync("noone@example.com");
			Assert.Empty(result);
		}

		[Fact]
		public async Task RemoveAvailabilityAsync_RemovesAvailability()
		{
			var availability = new Availability
			{
				InstructorEmail = "instructor3@example.com",
				Start = DateTime.UtcNow.Date.AddDays(3).AddHours(8),
				End = DateTime.UtcNow.Date.AddDays(3).AddHours(10)
			};
			_context.Availability.Add(availability);
			await _context.SaveChangesAsync();

			await _repo.RemoveAvailabilityAsync(availability.availabilityId);

			var found = await _context.Availability.FindAsync(availability.availabilityId);
			Assert.Null(found);
		}

		[Fact]
		public async Task RemoveAvailabilityAsync_NonExistent_DoesNotThrow()
		{
			var ex = await Record.ExceptionAsync(() => _repo.RemoveAvailabilityAsync(99999));
			Assert.Null(ex);
		}

		[Fact]
		public async Task GetAllAvailabilityAsync_ReturnsAll()
		{
			_context.Availability.Add(new Availability
			{
				InstructorEmail = "instructor4@example.com",
				Start = DateTime.UtcNow.Date.AddDays(4).AddHours(9),
				End = DateTime.UtcNow.Date.AddDays(4).AddHours(11)
			});
			await _context.SaveChangesAsync();

			var availabilities = await _repo.GetAllAvailabilityAsync();
			Assert.NotEmpty(availabilities);
		}

		public void Dispose()
		{
			_context.Dispose();
		}
	}
}
