using LBCore.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LBRepository
{
	public class ApplicationDbContext : DbContext
	{
		public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
			: base(options)
		{
		}

		// Define your DbSets here
		public DbSet<Profiles> Profile { get; set; }
		public DbSet<Booking> Bookings { get; set; }
		public DbSet<Availability> Availability { get; set; }
	}

}
