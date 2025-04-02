using LBCore.Managers;
using LBCore.Models;
using LBCore.Interfaces;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace LBTesting.Unit.Regular
{
	public class AccountManagerTests
	{
		private readonly Mock<IProfileRepos> _mockProfileRepos;
		private readonly AccountManager _accountManager;

		public AccountManagerTests()
		{
			_mockProfileRepos = new Mock<IProfileRepos>();
			_accountManager = new AccountManager(_mockProfileRepos.Object); // Updated with new repos
		}

		[Fact]
		public async Task GetProfileByEmailAsync_ProfileExists_ReturnsProfile()
		{
			var email = "test@example.com";
			var expectedProfile = new Profiles
			{
				ProfileId = 1,
				Email = email,
				DisplayName = "Test User",
				PhoneNumber = "1234567890",
				Address = "123 Test Street",
				PickupAddress = "456 Pickup Lane",
				DateOfBirth = new DateTime(1990, 1, 1)
			};

			_mockProfileRepos.Setup(repo => repo.GetProfileByEmailAsync(email))
				.ReturnsAsync(expectedProfile);

			var result = await _accountManager.GetProfileByEmailAsync(email);

			Assert.NotNull(result);
			Assert.Equal(expectedProfile.Email, result.Email);
		}

		[Fact]
		public async Task GetProfileByEmailAsync_ProfileNotFound_ThrowsException()
		{
			var email = "nonexistent@example.com";
			_mockProfileRepos.Setup(repo => repo.GetProfileByEmailAsync(email))
				.ReturnsAsync((Profiles)null); // Simulate profile not found

			var exception = await Assert.ThrowsAsync<Exception>(() => _accountManager.GetProfileByEmailAsync(email));
			Assert.Equal("Profile not found.", exception.Message); // Check exception message
		}

		[Fact]
		public async Task AddProfileAsync_ProfileAlreadyExists_ThrowsException()
		{
			var profile = new Profiles
			{
				ProfileId = 1,
				Email = "test@example.com",
				DisplayName = "Test User",
				PhoneNumber = "1234567890",
				Address = "123 Test Street",
				PickupAddress = "456 Pickup Lane",
				DateOfBirth = new DateTime(1990, 1, 1)
			};

			_mockProfileRepos.Setup(repo => repo.GetProfileByEmailAsync(profile.Email))
				.ReturnsAsync(profile); // Simulate that profile already exists

			var exception = await Assert.ThrowsAsync<Exception>(() => _accountManager.AddProfileAsync(profile));
			Assert.Equal("Profile already exists with this email.", exception.Message); // Check exception message
		}

		[Fact]
		public async Task AddProfileAsync_ProfileDoesNotExist_AddsProfile()
		{
			var profile = new Profiles
			{
				ProfileId = 1,
				Email = "newuser@example.com",
				DisplayName = "New User",
				PhoneNumber = "0987654321",
				Address = "789 New Street",
				PickupAddress = "101 Pickup Ave",
				DateOfBirth = new DateTime(1992, 2, 2)
			};

			_mockProfileRepos.Setup(repo => repo.GetProfileByEmailAsync(profile.Email))
				.ReturnsAsync((Profiles)null);

			_mockProfileRepos.Setup(repo => repo.AddProfileAsync(It.IsAny<Profiles>()))
				.Returns(Task.CompletedTask);

			await _accountManager.AddProfileAsync(profile);

			_mockProfileRepos.Verify(repo => repo.AddProfileAsync(profile), Times.Once);
		}

		[Fact]
		public async Task UpdateProfileAsync_ProfileNotFound_ThrowsException()
		{
			var profile = new Profiles
			{
				ProfileId = 1,
				Email = "nonexistent@example.com",
				DisplayName = "Nonexistent User",
				PhoneNumber = "1234567890",
				Address = "123 Unknown St",
				PickupAddress = "456 Unknown Ave",
				DateOfBirth = new DateTime(1985, 5, 5)
			};

			_mockProfileRepos.Setup(repo => repo.GetProfileByEmailAsync(profile.Email))
				.ReturnsAsync((Profiles)null); // Simulate profile not found

			var exception = await Assert.ThrowsAsync<Exception>(() => _accountManager.UpdateProfileAsync(profile));
			Assert.Equal("Profile not found.", exception.Message); // Check exception message
		}

		[Fact]
		public async Task UpdateProfileAsync_ProfileExists_UpdatesProfile()
		{
			var profile = new Profiles
			{
				ProfileId = 1,
				Email = "test@example.com",
				DisplayName = "Updated User",
				PhoneNumber = "0987654321",
				Address = "Updated Address",
				PickupAddress = "Updated Pickup Address",
				DateOfBirth = new DateTime(1985, 5, 5)
			};

			_mockProfileRepos.Setup(repo => repo.GetProfileByEmailAsync(profile.Email))
				.ReturnsAsync(profile);

			_mockProfileRepos.Setup(repo => repo.UpdateProfileAsync(It.IsAny<Profiles>()))
				.Returns(Task.CompletedTask);

			await _accountManager.UpdateProfileAsync(profile);

			_mockProfileRepos.Verify(repo => repo.UpdateProfileAsync(profile), Times.Once);
		}
	}
}
