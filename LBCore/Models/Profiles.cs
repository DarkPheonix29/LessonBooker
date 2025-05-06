using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LBCore.Models
{
	public class Profiles
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int ProfileId { get; set; }

		[Required]
		public string Role { get; set; }
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

		[Required]
		public string Email { get; set; }
	}
}
