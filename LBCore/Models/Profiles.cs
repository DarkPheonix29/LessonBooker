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
		public string DisplayName { get; set; }
		public string PhoneNumber { get; set; }
		public string Address { get; set; }
		public string PickupAddress { get; set; }
		public DateTime DateOfBirth { get; set; }
	}
}
