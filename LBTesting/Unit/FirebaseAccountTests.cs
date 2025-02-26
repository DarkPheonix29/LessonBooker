using FirebaseAdmin.Auth;
using LBCore.Interfaces;
using LBCore.Managers;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

public class FirebaseManagerTests
{
	private readonly Mock<IFirebaseAccountRepos> _mockFirebaseRepo;
	private readonly FirebaseManager _firebaseManager;

	public FirebaseManagerTests()
	{
		_mockFirebaseRepo = new Mock<IFirebaseAccountRepos>();
		_firebaseManager = new FirebaseManager(_mockFirebaseRepo.Object);
	}

	[Fact]
	public async Task SignUpAsync_ShouldThrowException_WhenInvalidEmail()
	{
		// Arrange
		var email = "invalid-email";
		var password = "password123";
		var role = "student";

		_mockFirebaseRepo
			.Setup(repo => repo.SignUpAsync(email, password, role))
			.ThrowsAsync(new Exception("Invalid email format"));

		// Act & Assert
		await Assert.ThrowsAsync<Exception>(() => _firebaseManager.SignUpAsync(email, password, role));
		_mockFirebaseRepo.Verify(repo => repo.SignUpAsync(email, password, role), Times.Once);
	}

	[Fact]
	public async Task LoginAsync_ShouldReturnUid_WhenValidToken()
	{
		// Arrange
		var idToken = "valid_token";
		var expectedUid = "12345";

		_mockFirebaseRepo
			.Setup(repo => repo.LoginAsync(idToken))
			.ReturnsAsync(expectedUid);

		// Act
		var result = await _firebaseManager.LoginAsync(idToken);

		// Assert
		Assert.Equal(expectedUid, result);
		_mockFirebaseRepo.Verify(repo => repo.LoginAsync(idToken), Times.Once);
	}

	[Fact]
	public async Task LoginAsync_ShouldThrowException_WhenInvalidToken()
	{
		// Arrange
		var idToken = "invalid_token";

		_mockFirebaseRepo
			.Setup(repo => repo.LoginAsync(idToken))
			.ThrowsAsync(new Exception("Invalid token"));

		// Act & Assert
		await Assert.ThrowsAsync<Exception>(() => _firebaseManager.LoginAsync(idToken));
		_mockFirebaseRepo.Verify(repo => repo.LoginAsync(idToken), Times.Once);
	}

	[Fact]
	public async Task LogoutAsync_ShouldCallLogoutUser()
	{
		// Arrange
		var uid = "12345";

		_mockFirebaseRepo
			.Setup(repo => repo.LogoutUserAsync(uid))
			.Returns(Task.CompletedTask);

		// Act
		await _firebaseManager.LogoutAsync(uid);

		// Assert
		_mockFirebaseRepo.Verify(repo => repo.LogoutUserAsync(uid), Times.Once);
	}

	[Fact]
	public async Task VerifyTokenAsync_ShouldReturnTrue_WhenValidToken()
	{
		// Arrange
		var idToken = "valid_token";
		var uid = "12345";

		_mockFirebaseRepo
			.Setup(repo => repo.LoginAsync(idToken))
			.ReturnsAsync(uid);

		// Act
		var result = await _firebaseManager.VerifyTokenAsync(idToken);

		// Assert
		Assert.True(result);
		_mockFirebaseRepo.Verify(repo => repo.LoginAsync(idToken), Times.Once);
	}

	[Fact]
	public async Task VerifyTokenAsync_ShouldReturnFalse_WhenInvalidToken()
	{
		// Arrange
		var idToken = "invalid_token";

		_mockFirebaseRepo
			.Setup(repo => repo.LoginAsync(idToken))
			.ThrowsAsync(new Exception("Invalid token"));

		// Act
		var result = await _firebaseManager.VerifyTokenAsync(idToken);

		// Assert
		Assert.False(result);
		_mockFirebaseRepo.Verify(repo => repo.LoginAsync(idToken), Times.Once);
	}
}
