using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LBCore.Models
{
    public class Profiles
    {
		[Key]
		public int ProfileId { get; set; }
		[Required]
		public string Email { get; set; }
		[Required]
		public string DisplayName { get; set; }
		[Required]
		public string PhoneNumber { get; set; }
		[Required]
		public string Address { get; set; }
		[Required]
		public string PickupAddress { get; set; }
		[Required]
		public DateTime DateOfBirth { get; set; }
	}
}
