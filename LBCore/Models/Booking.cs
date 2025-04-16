using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LBCore.Models
{
	public class Booking
	{
		[Key]
		public string Id { get; set; }
		public string StudentEmail { get; set; }
		public string InstructorEmail { get; set; }
		public DateTime Start { get; set; }
		public DateTime End { get; set; }
	}

}
